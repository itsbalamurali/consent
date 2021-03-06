﻿using System;
using System.Linq;
using AspNetCore.RouteAnalyzer;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Parsing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddConsentSystemTypeRegistrations(services);
   
            services
                .AddMvc(
                    options =>
                    {
                        options.ReturnHttpNotAcceptable = true;
                        options.RespectBrowserAcceptHeader = true;
                    })
                .AddFeatureFolders()
                .AddApplicationPart(typeof(ConsentController).Assembly)
                .AddControllersAsServices()
                .AddRazorPagesOptions(o =>
                {
                    o.Conventions.AuthorizeFolder("/");
                    o.Conventions.AuthorizeFolder("/Admin", "WebsiteAdmin");
                });
            

            services.AddTransient<IConfigureOptions<MvcJsonOptions>, ConfigureJsonOptions>();

            var identityServerConfiguration =
                Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
            
            services
                .AddAuthentication()
                .AddCookie("Monster")
                .AddOpenIdConnect(
                    options =>
                    {
                        options.SignInScheme = "Monster";
                        options.Authority = identityServerConfiguration.Authority;
                        options.ClientId = "UI";
                        if (Environment.IsDevelopment())
                            options.RequireHttpsMetadata = false;
                        options.SaveTokens = true;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.GetClaimsFromUserInfoEndpoint = true;                        
                    });

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("WebsiteAdmin", p => p.RequireRole("Website Admin"));
                });
                
            services.AddHttpContextAccessor();
            services.AddTransient<IUserProvider, HttpContextUserProvider>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsProvider>();
            services.AddSwaggerGen();
            services.Configure<IdentifierDisplayOptions>(Configuration.GetSection("IdentifierDisplay"));
            AddDataServices(services);
        }


        private void AddDataServices(IServiceCollection services)
        {
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<IConsentRepository, ConsentRepository>();
            services.AddScoped<StudySubjectRepository>();
            services.AddScoped<IStudySubjectRepository, StudySubjectRepository>();
            services.AddScoped<StudyRepository>();
            services.AddScoped<IStudyRepository, StudyRepository>();
            services.AddScoped<ISubjectIdentifierRepository, SubjectIdentifierRepository>();

            services.AddDbContext<ConsentContext>(ConfigureDatabaseOptions);
            services.AddScoped<IStoreProvider, ContextStoreProvider>();
            
            services.AddScoped(typeof(IStore<>), typeof(Store<>));
        }

        private void AddConsentSystemTypeRegistrations(IServiceCollection services)
        {
            services.TryAddScoped<IdentifierDefinitionRegistry>(
                provider =>
                {
                    var definitionParser = new IdentifierDefinitionParser();

                    return new IdentifierDefinitionRegistry(
                        provider.GetService<ConsentContext>().Set<IdentifierDefinitionEntity>()
                            .Select(
                                e =>
                                    new IdentifierDefinition(e.Name, definitionParser.ParseString(e.Definition).Type)));
                }
            );
            services.TryAddScoped<EvidenceDefinitionRegistry>(
                provider =>
                {
                    var parser = new EvidenceDefinitionParser();
                    return new EvidenceDefinitionRegistry(
                        provider.GetService<ConsentContext>().Set<EvidenceDefinitionEntity>()
                            .Select(
                                e => new EvidenceDefinition(e.Name, parser.ParseString(e.Definition).Type)
                            )
                    );
                }
            );
            
            services.AddTransient<IPersonIdentifierDisplayHandlerProvider, PersonIdentifierHandlerProvider>();
        }

        protected virtual void ConfigureDatabaseOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("Consent"));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            if (Configuration.GetIdentityServer().EnableInternalServer)
            {
                app.UseIdentityServer();
            }

            app.UseMvc(r =>
            {
                r.MapRoute("default", "{controller=home}/{action=index}");
            });
            app.UseSwagger();
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
                ui.OAuthClientId("ApiExplorer");
                ui.OAuthAppName("API Explorer");
                ui.DisplayOperationId();
            });
        }
    }
}