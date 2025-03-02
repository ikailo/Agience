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
using Agience.Core.Interfaces;
using Agience.Authority.Identity.Data.Repositories;
using Microsoft.AspNetCore.HostFiltering;

namespace Agience.Authority.Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var appConfig = new AppConfig();
        builder.Configuration.Bind(appConfig);
        builder.Services.AddSingleton(appConfig);

        if (appConfig.AuthorityPublicUri == null) { throw new ArgumentNullException(nameof(appConfig.AuthorityPublicUri)); }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
        });

        builder.Services.Configure<HostFilteringOptions>(options =>
        {
            // Always allow the public host
            options.AllowedHosts.Add(new Uri(appConfig.AuthorityPublicUri).Host);

            // If Identity is running externally, use the external host. Otherwise use the authority host.
            if (appConfig.LanExternalAuthority && !string.IsNullOrEmpty(appConfig.LanExternalHost) && !options.AllowedHosts.Contains(appConfig.LanExternalHost))
            {
                options.AllowedHosts.Add(appConfig.LanExternalHost);
            }
            else if (!string.IsNullOrEmpty(appConfig.LanAuthorityHost) && !options.AllowedHosts.Contains(appConfig.LanAuthorityHost))
            {
                options.AllowedHosts.Add(appConfig.LanAuthorityHost);
            }

            // Additionally, if WAN is enabled, add the WAN host for broker access.
            if (appConfig.WanEnabled && !string.IsNullOrWhiteSpace(appConfig.WanHost) && !options.AllowedHosts.Contains(appConfig.WanHost))
            {
                options.AllowedHosts.Add(appConfig.WanHost);
            }
        });


        // --- Modified Kestrel Configuration ---
        builder.WebHost.ConfigureKestrel(options =>
        {
            var buildContextPath = Environment.GetEnvironmentVariable("BUILD_CONTEXT_PATH") ?? string.Empty;

            if (appConfig.WanEnabled)
            {
                if (string.IsNullOrWhiteSpace(appConfig.WanPfxPath))
                {
                    throw new ArgumentNullException(nameof(appConfig.WanPfxPath));
                }

                options.ListenAnyIP(appConfig.WanAuthorityPort, listenOptions =>
                {
                    listenOptions.UseHttps(Path.Combine(buildContextPath, appConfig.WanPfxPath));
                });
            }

            // If Identity is running externally, use the external host. Otherwise use the authority host.
            if (appConfig.LanExternalAuthority)
            {
                if (string.IsNullOrWhiteSpace(appConfig.LanExternalPfxPath))
                {
                    throw new ArgumentNullException(nameof(appConfig.LanExternalPfxPath));
                }

                options.ListenAnyIP(appConfig.LanAuthorityPort, listenOptions =>
                {
                    listenOptions.UseHttps(Path.Combine(buildContextPath, appConfig.LanExternalPfxPath));
                });

            }
            else
            {
                if (string.IsNullOrWhiteSpace(appConfig.LanPfxPath))
                {
                    throw new ArgumentNullException(nameof(appConfig.LanPfxPath));
                }

                options.ListenAnyIP(appConfig.LanAuthorityPort, listenOptions =>
                {
                    listenOptions.UseHttps(Path.Combine(buildContextPath, appConfig.LanPfxPath));
                });
            }
        });

        Uri? authorityUri = null;
        Uri? brokerUri = null;

        if (appConfig.LanExternalAuthority)
        {
            authorityUri = new Uri($"https://{appConfig.WanHost}:{appConfig.WanAuthorityPort}");
            brokerUri = new Uri($"https://{appConfig.WanHost}:{appConfig.WanBrokerPort}");
        }
        else
        {
            authorityUri = new Uri($"https://{appConfig.LanAuthorityHost}:{appConfig.LanAuthorityPort}");
            brokerUri = new Uri($"https://{appConfig.LanBrokerHost}:{appConfig.LanBrokerPort}");
        }

        builder.Services.AddAgienceAuthoritySingleton(appConfig.AuthorityPublicUri, appConfig.CustomNtpHost, authorityUri, brokerUri);
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
            options.IssuerUri = appConfig.AuthorityPublicUri;
            options.Discovery.CustomEntries.Add("broker_uri", appConfig.BrokerPublicUri ?? throw new ArgumentNullException(nameof(appConfig.BrokerPublicUri)));
            //options.Discovery.CustomEntries.Add("files_uri", appConfig.ExternalFilesUri ?? throw new ArgumentNullException(nameof(appConfig.ExternalFilesUri)));
            //options.Discovery.CustomEntries.Add("stream_uri", appConfig.ExternalStreamUri ?? throw new ArgumentNullException(nameof(appConfig.ExternalStreamUri)));
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

        builder.Services.AddSingleton(new AgienceIdProvider(appConfig.AuthorityPublicUri));
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

        var connectionString =
             $"Host={appConfig.DatabaseHost};" +
             $"Port={appConfig.LanDatabasePort};" +
             $"Database={appConfig.DatabaseName};" +
             $"Username={appConfig.DatabaseUsername};" +
             $"Password={appConfig.DatabasePassword};" +
             $"SSL Mode=VerifyFull;";

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

                options.ClientId = appConfig.GoogleOAuthClientId ?? string.Empty;
                options.ClientSecret = appConfig.GoogleOAuthClientSecret ?? string.Empty;
                options.Scope.Add(IdentityServerConstants.StandardScopes.OpenId);
                options.Scope.Add(IdentityServerConstants.StandardScopes.Profile);
                options.Scope.Add(IdentityServerConstants.StandardScopes.Email);

            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
           {
               options.Authority = appConfig.AuthorityPublicUri;
               options.RequireHttpsMetadata = true;

               if (appConfig.LanExternalAuthority)
               {
                   options.MetadataAddress = $"https://{appConfig.WanHost}:{appConfig.WanAuthorityPort}/.well-known/openid-configuration";
               }
               else
               {
                   options.MetadataAddress = $"https://{appConfig.LanAuthorityHost}:{appConfig.LanAuthorityPort}/.well-known/openid-configuration";
               }

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidIssuer = appConfig.AuthorityPublicUri,
                   ValidateAudience = true,
                   ValidAudiences = new List<string> { "manage-api", "connect-mqtt" },
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true
               };

               options.Events = new JwtBearerEvents
               {
                   OnTokenValidated = context =>
                  {
                      var identity = context.Principal.Identity as System.Security.Claims.ClaimsIdentity;
                      foreach (var claim in identity.Claims)
                      {
                          Console.WriteLine($"{claim.Type}: {claim.Value}");
                      }
                      return Task.CompletedTask;
                  },
                   OnAuthenticationFailed = context =>
                   {
                       Console.WriteLine("Authentication failed: " + context.Exception.Message);
                       return Task.CompletedTask;
                   },
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