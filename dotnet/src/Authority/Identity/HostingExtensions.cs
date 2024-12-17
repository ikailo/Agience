using Serilog;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Agience.Authority.Identity.Validators;
using Agience.Authority.Identity.Data;
using Agience.Authority.Identity.Services;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Core.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Agience.Core.Interfaces;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Core.Models.Entities.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;


namespace Agience.Authority.Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var appConfig = new AppConfig();
        builder.Configuration.Bind(appConfig);
        builder.Services.AddSingleton(appConfig);

        if (string.IsNullOrWhiteSpace(appConfig.AuthorityUri)) { throw new ArgumentNullException(nameof(appConfig.AuthorityUri)); }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
        });

        builder.Services.AddAgienceAuthoritySingleton(appConfig.AuthorityUri, appConfig.CustomNtpHost, appConfig.AuthorityUriInternal, appConfig.BrokerUriInternal);
        builder.Services.AddHostedService<AgienceAuthorityService>();

        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });        

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClient();

        builder.Services.AddRazorPages();

        builder.Services.AddControllers();
        //builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddSwaggerGen();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddIdentityServer(options =>
        {
            options.IssuerUri = appConfig.AuthorityUri;
            options.Discovery.CustomEntries.Add("broker_uri", appConfig.BrokerUri ?? throw new ArgumentNullException("BrokerUri"));
            options.Discovery.CustomEntries.Add("files_uri", appConfig.BrokerUri ?? throw new ArgumentNullException("FilesUri"));
            //options.Discovery.CustomEntries.Add("stream_uri", appConfig.BrokerUri ?? throw new ArgumentNullException("StreamUri"));
            options.Authentication.CookieLifetime = TimeSpan.FromDays(30); // TODO: Manage sessions better.
            options.Endpoints.EnableIntrospectionEndpoint = true;

            //options.Events.RaiseSuccessEvents = true;
            //options.Events.RaiseFailureEvents = true;
            //options.Events.RaiseErrorEvents = true;
            //options.Events.RaiseInformationEvents = true;

        })
            .AddInMemoryIdentityResources(appConfig.IdentityResources)
            .AddInMemoryApiResources(appConfig.ApiResources)
            .AddInMemoryApiScopes(appConfig.ApiScopes)
            .AddProfileService<ProfileService>()
            .AddClientStore<AgienceHostStore>()
            .AddSecretValidator<HostSecretValidator>()
            .AddJwtBearerClientAuthentication();

        builder.Services.AddSingleton(new AgienceIdProvider(appConfig.AuthorityUri));
        builder.Services.AddSingleton<AgienceKeyMaterialService>();
        builder.Services.AddScoped<StateValidator>();

        // Mailchimp Setup
        var mailchimpApiKey = appConfig.MailchimpApiKey;
        var mailchimpAudienceId = appConfig.MailchimpAudienceId;

        if (!string.IsNullOrEmpty(mailchimpApiKey) && !string.IsNullOrEmpty(mailchimpAudienceId))
        {
            builder.Services.AddSingleton<ICrmService>(sp =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
                var mailchimpTags = appConfig.MailchimpTags?.Split(',') ?? Array.Empty<string>();
                return new MailchimpService(httpClient, mailchimpApiKey, mailchimpAudienceId, mailchimpTags, sp.GetRequiredService<ILogger<MailchimpService>>());
            });
        }

        Uri authorityDbUri = new Uri(appConfig.AuthorityDbUri ?? throw new ArgumentNullException("AuthorityDbUri"));

        var connectionString =
             $"Host={authorityDbUri.Host};" +
             $"Port={authorityDbUri.Port};" +
             $"Database={appConfig.AuthorityDbDatabase};" +
             $"Username={appConfig.AuthorityDbUsername};" +
             $"Password={appConfig.AuthorityDbPassword};";

        if (builder.Environment.EnvironmentName.Equals("debug", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += "SSL Mode=Require;Trust Server Certificate=true;";
        }
        else
        {
            connectionString += "SSL Mode=VerifyFull;";
        }

        builder.Services.AddDbContext<AgienceDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseNpgsql(connectionString);
        });

        builder.Services.AddScoped<AgienceDataAdapter>();        
        builder.Services.AddScoped<RecordsRepository>();        
        builder.Services.AddScoped<IAuthorityRecordsRepository, AuthorityRecordsRepository>();        

        builder.Services.AddSingleton<PluginImportService>();

        builder.Services.AddDistributedMemoryCache(); // Or a distributed cache like Redis
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
            options.Cookie.HttpOnly = true; // Security settings
            options.Cookie.IsEssential = true; // Required for GDPR compliance
        });



        builder.Services.AddScoped<AgiencePersonStore>();
        builder.Services.AddScoped<IUserStore<Models.Person>>(sp => sp.GetRequiredService<AgiencePersonStore>());

        builder.Services.AddAuthentication(options => { })

            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ForwardSignOut = IdentityServerConstants.DefaultCookieAuthenticationScheme;

                options.ClientId = appConfig.GoogleClientId ?? string.Empty;
                options.ClientSecret = appConfig.GoogleClientSecret ?? string.Empty;
                options.Scope.Add(IdentityServerConstants.StandardScopes.OpenId);
                options.Scope.Add(IdentityServerConstants.StandardScopes.Profile);
                options.Scope.Add(IdentityServerConstants.StandardScopes.Email);

            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
           {
               options.Authority = appConfig.AuthorityUri;
               options.RequireHttpsMetadata = true;

               if (appConfig.AuthorityUriInternal != null)
               {
                   options.MetadataAddress = $"{appConfig.AuthorityUriInternal}/.well-known/openid-configuration";
               }

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = appConfig.AuthorityUri,
                   ValidateAudience = true,                   
                   ValidAudiences = new List<string> { "manage-api", "connect-mqtt" },
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true
               };

               options.Events = new JwtBearerEvents
               {
                   OnChallenge = context =>
                   {
                       if (!context.Response.HasStarted)
                       {
                           context.HandleResponse();
                           context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                           context.Response.ContentType = "application/json";
                           var errorResponse = new
                           {
                               error = "Unauthorized",
                               description = context.ErrorDescription
                           };
                           return context.Response.WriteAsJsonAsync(errorResponse);
                       }
                       return Task.CompletedTask;
                   }
               };
           });
        

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("manage", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "manage");
            });

            options.AddPolicy("connect", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "connect");
            });

            options.AddPolicy("host", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("host"); // Require the 'host' role
            });

        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app, AppConfig appConfig)
    {
        app.UseSerilogRequestLogging();

        app.UseHostFiltering();
        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.ToLower() == "local")
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
            app.UseHsts();
        }
        /*
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.ToLower() == "local")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agience API v1");
                //c.RoutePrefix = "manage";
            });
        }
        */
        app.UseSession();

        app.UseIdentityServer();
        app.UseAuthentication();

        app.UseStaticFiles(); // For the wwwroot folder

        /*
        app.UseStaticFiles(new StaticFileOptions
        {
            // TODO: Limit to Hosts ?

            FileProvider = new PhysicalFileProvider(appConfig.FilesRoot ?? throw new ArgumentNullException(nameof(appConfig.FilesRoot))),
            RequestPath = "/files",
            OnPrepareResponse = ctx =>
            {
                // Add the Content-Disposition header to force file download
                var fileName = Path.GetFileName(ctx.File.PhysicalPath);
                ctx.Context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
            }
        });*/



        app.UseRouting();

        app.UseAuthorization();

        app.UseStatusCodePages();

        app.MapControllers();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}