using MudBlazor.Services;
using DotNetEnv.Configuration;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Agience.Authority.Manage.Components.Global;
using Agience.Authority.Manage.Services;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.HttpOverrides;
using Agience.Authority.Manage.Models;
using System.Security.Claims;

namespace Agience.Authority.Manage
{
    internal class Program
    {
        private static async Task Main(string[] args) // Change return type to Task
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Configuration
            Env.Load($".env.{builder.Environment.EnvironmentName}");

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddDotNetEnv($".env.{builder.Environment.EnvironmentName}")
                .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName.ToLower() == "debug")
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            var appConfig = new AppConfig();
            builder.Configuration.Bind(appConfig);
            builder.Services.AddSingleton(appConfig);

            if ((appConfig.AuthorityUriInternal ?? appConfig.AuthorityUri) == null)
            {
                throw new ArgumentNullException(nameof(appConfig.AuthorityUri));
            }

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // This is the Agience Authority Service that connects to the Authority API.
            builder.Services.AddScoped<AuthorityService>();

            // Authentication and Authorization configuration
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
           .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
           {
               //options.ExpireTimeSpan = TimeSpan.FromDays(30); // Set the cookie expiration to 30 days
               //options.SlidingExpiration = true; // Enable sliding expiration
           })
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
           {
               options.Authority = appConfig.AuthorityUri;
               options.ClientId = appConfig.HostId;
               options.ClientSecret = appConfig.HostSecret;
               options.ResponseType = OpenIdConnectResponseType.Code;
               options.Scope.Add("manage");
               options.Scope.Add("openid");
               options.Scope.Add("profile");
               options.Scope.Add("email");

               //options.UsePkce = true;
               options.SaveTokens = true;
               //options.UseTokenLifetime = false; // Use the cookie's lifetime, not the token's lifetime

               options.TokenValidationParameters = new TokenValidationParameters
               {
                   NameClaimType = "name",
                   RoleClaimType = "role",
                   ValidateIssuer = true,
                   ValidIssuer = appConfig.AuthorityUri,
                   ValidateAudience = true,
                   ValidAudience = appConfig.HostId,
                   ValidateLifetime = true,
                   ClockSkew = TimeSpan.FromMinutes(5) // Allow for small clock drift
               };

               if (appConfig.AuthorityUriInternal != null)
               {
                   options.MetadataAddress = $"{appConfig.AuthorityUriInternal}/.well-known/openid-configuration";
               }

               options.Events = new OpenIdConnectEvents
               {
                   OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context, appConfig.AuthorityUri, appConfig.AuthorityUriInternal),
                   OnRedirectToIdentityProviderForSignOut = context => HandleRedirectToIdentityProvider(context, appConfig.AuthorityUri, appConfig.AuthorityUriInternal),

                   // Add the access_token to claims
                   OnTokenValidated = context =>
                   {
                       var identity = context.Principal?.Identity as ClaimsIdentity;

                       if (identity != null)
                       {
                           var accessToken = context.TokenEndpointResponse?.AccessToken;
                           if (!string.IsNullOrEmpty(accessToken))
                           {
                               identity.AddClaim(new Claim("access_token", accessToken));
                           }

                           var idToken = context.TokenEndpointResponse?.IdToken;
                           if (!string.IsNullOrEmpty(idToken))
                           {
                               identity.AddClaim(new Claim("id_token", idToken));
                           }

                           var refreshToken = context.TokenEndpointResponse?.RefreshToken;
                           if (!string.IsNullOrEmpty(refreshToken))
                           {
                               identity.AddClaim(new Claim("refresh_token", refreshToken));
                           }
                       }

                       return Task.CompletedTask;
                   }
               };
           });

            builder.Services.AddControllersWithViews();

            // Add services to the container.
            builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

            builder.Services.AddScoped<RecordHandler>();
            builder.Services.AddScoped<ContextService>();                        

            builder.Services.AddClipboard();

            var app = builder.Build();

            // Configure the HTTP request pipeline

            if (app.Environment.EnvironmentName != "debug" && app.Environment.EnvironmentName != "development")
            {
                var options = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

                app.UseForwardedHeaders(options);

                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts(); // Enforce HSTS in production
            }

            app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
            app.UseStaticFiles();       // Serve static files (CSS, JS, images, etc.)
            app.UseRouting();           // Define route matching

            // Authentication and session handling
            app.UseAuthentication();    // Set up authentication            
            app.UseAuthorization();     // Enforce authorization rules

            // Anti-forgery middleware after authentication
            app.UseAntiforgery();       // Protect against CSRF attacks

            // Map Controllers for API endpoints
            app.MapControllers();       // Map API controllers

            // Map Razor Components (Blazor Server)
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode(); // Add interactive Blazor rendering mode

            EntityRegistry.RegisterAgienceEntities();


            await app.RunAsync(); // Run the application

        }

        private static Task HandleRedirectToIdentityProvider(RedirectContext context, string authorityUri, string? authorityUriInternal)
        {
            if (authorityUriInternal != null)
            {
                var authorityUriUri = new Uri(authorityUri);

                context.ProtocolMessage.IssuerAddress = new UriBuilder(context.ProtocolMessage.IssuerAddress)
                {
                    Host = authorityUriUri.Host,
                    Port = authorityUriUri.Port
                }.Uri.ToString();
            }

            return Task.CompletedTask;
        }
    }
}