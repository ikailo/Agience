using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Agience.Authority.Manage.Services;
using System.Net.Http.Headers;
using Agience.Core.Extensions;
using Agience.Plugins.Core.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Agience.Core.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Agience.Authority.Manage
{
    internal static class HostingExtensions
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            // Configuration
            var appConfig = new AppConfig();
            builder.Configuration.Bind(appConfig);
            builder.Services.AddSingleton(appConfig);

            // Logging
            builder.Services.AddLogging(loggerBuilder =>
            {
                loggerBuilder.ClearProviders();
                loggerBuilder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    //options.SingleLine = true;
                    //options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                });
            });

            // Validate configuration
            if (string.IsNullOrWhiteSpace(appConfig.AuthorityUri)) { throw new ArgumentNullException(nameof(appConfig.AuthorityUri)); }
            if (string.IsNullOrWhiteSpace(appConfig.HostId)) { throw new ArgumentNullException(nameof(appConfig.HostId)); }
            if (string.IsNullOrWhiteSpace(appConfig.HostSecret)) { throw new ArgumentNullException(nameof(appConfig.HostSecret)); }

            // Agience specific logging
            builder.Services.AddSingleton(sp => new AgienceEventLoggerProvider(sp));

            // This is the host.
            builder.Services.AddAgienceHost(appConfig.AuthorityUri, appConfig.HostId, appConfig.HostSecret, appConfig.CustomNtpHost, appConfig.AuthorityUriInternal, appConfig.BrokerUriInternal, appConfig.HostOpenAiApiKey);

            // This is the service that runs the host.
            builder.Services.AddHostedService<AgienceHostService>();

            // This is the service that connects to the chat.
            builder.Services.AddSingleton<AgienceWebInteractionService>();
            builder.Services.AddSingleton<IAgienceEventLogHandler>(sp => sp.GetRequiredService<AgienceWebInteractionService>());

            // This is the Agience Authority Service that connects to the Authority API.
            builder.Services.AddTransient(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                return new AgienceAuthorityService(httpClientFactory, httpContextAccessor, appConfig.AuthorityUri);
            });

            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            builder.Services.AddHttpClient("authority", client =>
            {
                client.BaseAddress = new Uri(appConfig.AuthorityUriInternal ?? appConfig.AuthorityUri);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    AllowAutoRedirect = false
                });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = appConfig.AuthorityUri;
                options.ClientId = appConfig.HostId;
                options.ClientSecret = appConfig.HostSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Scope.Add("manage");

                options.UsePkce = true;
                options.SaveTokens = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                if (appConfig.AuthorityUriInternal != null)
                {
                    options.MetadataAddress = $"{appConfig.AuthorityUriInternal}/.well-known/openid-configuration";
                }

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context => HandleRedirectToIdentityProvider(context, appConfig.AuthorityUri, appConfig.AuthorityUriInternal),
                    OnRedirectToIdentityProviderForSignOut = context => HandleRedirectToIdentityProvider(context, appConfig.AuthorityUri, appConfig.AuthorityUriInternal)
                };
            });

            return builder;
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseHostFiltering();

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "local")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
                app.UseHsts();
            }

            app.UseStatusCodePages();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapHub<AgienceChatHub>("/chat-hub");
            
            var agienceHost = app.Services.GetRequiredService<Core.Host>();

#pragma warning disable SKEXP0050
            // TODO: Dynamically load these on Host-Welcome
            agienceHost.AddPluginFromType<TimePlugin>("msTime");
            agienceHost.AddPluginFromType<ChatCompletionPlugin>("openAiChatCompletion");
#pragma warning restore SKEXP0050

            return app;
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
