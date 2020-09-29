﻿using AspNetCoreHero.Application.Configurations;
using AspNetCoreHero.Application.Constants;
using AspNetCoreHero.Application.Constants.Permissions;
using AspNetCoreHero.Application.Interfaces;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Infrastructure.Persistence.Contexts;
using AspNetCoreHero.Infrastructure.Persistence.Identity;
using AspNetCoreHero.Infrastructure.Persistence.Repositories;
using AspNetCoreHero.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Infrastructure.Persistence.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddPersistenceInfrastructureForWeb(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddPersistenceContexts(configuration);
            services.AddRepositories();
            services.Configure<MemoryCacheConfiguration>(configuration.GetSection("MemoryCacheConfiguration"));

        }
        public static void AddAuthenticationSchemeForWeb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(o =>
            {
                //Add Authentication to all Controllers by default.
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                o.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(MasterPermissions.Create, policy => { policy.RequireClaim(CustomClaimTypes.Permission, MasterPermissions.Create); });
            });
        }
        private static void AddPersistenceContexts(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IdentityContext>(options =>
                           options.UseSqlServer(
                               configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                })
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddDefaultUI()
            .AddDefaultTokenProviders();
            services.AddDbContext<ApplicationContext>(options =>
              options.UseSqlServer(
                  configuration.GetConnectionString("DefaultConnection"),
                  b => b.MigrationsAssembly(typeof(ApplicationContext).Assembly.FullName)));
        }
        private static void AddRepositories(this IServiceCollection services)
        {
            #region Repositories
            services.AddTransient(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
            services.AddTransient<IProductRepositoryAsync, ProductRepositoryAsync>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            #endregion
        }
       
    }
}
