using Microsoft.EntityFrameworkCore;
using Agience.Authority.Identity.Models;
using Host = Agience.Authority.Identity.Models.Host;
using System.Text.Json;
using Agience.Authority.Identity.Validators;
using Agience.Core.Models.Enums;
using Agience.Authority.Identity.Converters;


namespace Agience.Authority.Identity.Data
{
    public class AgienceDbContext : DbContext
    {
        private readonly ILogger<AgienceDbContext> _logger;

        public AgienceDbContext(DbContextOptions<AgienceDbContext> options, ILogger<AgienceDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<AgentLogEntry> AgentLogEntries { get; set; }
        public DbSet<AgentPlugin> AgentPlugins { get; set; }
        public DbSet<AgentTopic> AgentTopics { get; set; }
        public DbSet<Authorizer> Authorizers { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<ConnectionAuthorizer> ConnectionAuthorizers { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<FunctionConnection> FunctionConnections { get; set; }
        public DbSet<Host> Hosts { get; set; }
        public DbSet<HostPlugin> HostPlugins { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<PluginFunction> PluginFunctions { get; set; }
        public DbSet<Topic> Topics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Agent>(a =>
            {
                a.ToTable(nameof(Agents));
                a.HasKey(a => a.Id);
                a.HasOne(a => a.Owner).WithMany(p => p.Agents).HasForeignKey(a => a.OwnerId);
                a.HasOne(a => a.Host).WithMany(h => h.Agents).HasForeignKey(a => a.HostId);
                a.HasOne(a => a.ExecutiveFunction).WithMany().HasForeignKey(a => a.ExecutiveFunctionId);
                a.HasOne(a => a.AutoStartFunction).WithMany().HasForeignKey(a => a.AutoStartFunctionId);
                a.HasMany(a => a.Plugins).WithMany().UsingEntity<AgentPlugin>();
                a.HasMany(a => a.Topics).WithMany(t => t.Agents).UsingEntity<AgentTopic>();
                a.HasMany(a => a.LogEntries).WithOne(lg => lg.Agent).HasForeignKey(lg => lg.AgentId);
            });

            modelBuilder.Entity<AgentLogEntry>(lg =>
            {
                lg.ToTable(nameof(AgentLogEntries));
                lg.HasKey(lg => lg.Id);
            });

            modelBuilder.Entity<AgentPlugin>(ap =>
            {
                ap.ToTable(nameof(AgentPlugins));
                ap.HasKey(ap => ap.Id);
                ap.HasIndex(ap => new { ap.AgentId, ap.PluginId });
            });

            modelBuilder.Entity<AgentTopic>(at =>
            {
                at.ToTable(nameof(AgentTopics));
                at.HasKey(at => at.Id);
                at.HasIndex(at => new { at.AgentId, at.TopicId });
            });

            modelBuilder.Entity<Authorizer>(a =>
            {
                a.ToTable(nameof(Authorizers));
                a.HasKey(a => a.Id);
                a.HasOne(a => a.Owner).WithMany(p => p.Authorizers).HasForeignKey(a => a.OwnerId);
                //a.Property(a => a.Scopes).HasConversion(ListValueConversion.GetValueConverter()).Metadata.SetValueComparer(ListValueConversion.GetValueComparer());
                a.Property(a => a.Visibility).HasConversion(v => v.ToString(), v => Enum.Parse<Visibility>(v)).HasDefaultValue(Visibility.Private);
                a.Property(a => a.AuthType).HasConversion(v => v.ToString(), v => Enum.Parse<AuthorizationType>(v)).HasDefaultValue(AuthorizationType.Public);
            });

            modelBuilder.Entity<Connection>(c =>
            {
                c.ToTable(nameof(Connections));
                c.HasKey(c => c.Id);
                c.HasOne(c => c.Owner).WithMany(a => a.Connections).HasForeignKey(c => c.OwnerId);
                c.HasMany(c => c.Authorizers).WithMany(a => a.Connections).UsingEntity<ConnectionAuthorizer>();
                c.Property(c => c.Scopes).HasConversion(ListValueConversion.GetValueConverter()).Metadata.SetValueComparer(ListValueConversion.GetValueComparer());
                c.Property(c => c.Visibility).HasConversion(v => v.ToString(), v => Enum.Parse<Visibility>(v)).HasDefaultValue(Visibility.Private);
            });

            modelBuilder.Entity<ConnectionAuthorizer>(ca =>
            {
                ca.ToTable(nameof(ConnectionAuthorizers));
                ca.HasKey(ca => ca.Id);
                ca.HasIndex(ca => new { ca.ConnectionId, ca.AuthorizerId });
            });

            modelBuilder.Entity<Credential>(c =>
            {
                c.ToTable(nameof(Credentials));
                c.HasKey(c => c.Id);
                c.HasOne(c => c.Agent).WithMany().HasForeignKey(c => c.AgentId);
                c.HasOne(c => c.Connection).WithMany().HasForeignKey(c => c.ConnectionId);
                c.Property(c => c.Status).HasConversion(v => v.ToString(), v => Enum.Parse<CredentialStatus>(v)).HasDefaultValue(CredentialStatus.NoAuthorizer);
            });

            modelBuilder.Entity<Function>(f =>
            {
                f.ToTable(nameof(Functions));
                f.HasKey(f => f.Id);
                f.HasMany(f => f.Connections).WithMany(c => c.Functions).UsingEntity<FunctionConnection>();
                f.HasMany(f => f.Inputs).WithOne().HasForeignKey(ip => ip.InputFunctionId).OnDelete(DeleteBehavior.Cascade);
                f.HasMany(f => f.Outputs).WithOne().HasForeignKey(op => op.OutputFunctionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FunctionConnection>(fc =>
            {
                fc.ToTable(nameof(FunctionConnections));
                fc.HasKey(fc => fc.Id);
                fc.HasIndex(fc => new { fc.FunctionId, fc.ConnectionId });
            });

            modelBuilder.Entity<Host>(h =>
            {
                h.ToTable(nameof(Hosts));
                h.HasKey(h => h.Id);
                h.HasOne(h => h.Owner).WithMany(p => p.Hosts).HasForeignKey(a => a.OwnerId);
                h.HasMany(h => h.Plugins).WithMany().UsingEntity<HostPlugin>();
                h.HasMany(h => h.Agents).WithOne(a => a.Host).HasForeignKey(a => a.HostId);
                h.Property(h => h.Scopes).HasConversion(ListValueConversion.GetValueConverter()).Metadata.SetValueComparer(ListValueConversion.GetValueComparer());
                h.Property(h => h.Visibility).HasConversion(v => v.ToString(), v => Enum.Parse<Visibility>(v)).HasDefaultValue(Visibility.Private);
            });

            modelBuilder.Entity<HostPlugin>(hp =>
            {
                hp.ToTable(nameof(HostPlugins));
                hp.HasKey(hp => hp.Id);
                hp.HasIndex(hp => new { hp.HostId, hp.PluginId });
            });

            modelBuilder.Entity<Key>(k =>
            {
                k.ToTable(nameof(Keys));
                k.HasKey(k => k.Id);
                k.HasOne(k => k.Host).WithMany(h => h.Keys).HasForeignKey(h => h.HostId);
            });

            modelBuilder.Entity<Parameter>(p =>
            {
                p.ToTable(nameof(Parameters));
                p.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Person>(p =>
            {
                p.ToTable(nameof(People));
                p.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Plugin>(p =>
            {
                p.ToTable(nameof(Plugins));
                p.HasKey(p => p.Id);
                p.HasOne(p => p.Owner).WithMany(p => p.Plugins).HasForeignKey(p => p.OwnerId);
                p.HasMany(p => p.Functions).WithMany().UsingEntity<PluginFunction>();
                p.Property(p => p.Visibility).HasConversion(v => v.ToString(), v => Enum.Parse<Visibility>(v)).HasDefaultValue(Visibility.Private);
                p.Property(p => p.PluginProvider).HasConversion(v => v.ToString(), v => Enum.Parse<PluginProvider>(v)).HasDefaultValue(PluginProvider.Prompt);
                p.Property(p => p.PluginSource).HasConversion(v => v.ToString(), v => Enum.Parse<PluginSource>(v)).HasDefaultValue(PluginSource.UserDefined);

            });

            modelBuilder.Entity<PluginFunction>(pf =>
            {
                pf.ToTable(nameof(PluginFunctions));
                pf.HasKey(pf => pf.Id);
                pf.HasIndex(pf => new { pf.PluginId, pf.FunctionId });
                pf.HasOne(pf => pf.Plugin).WithMany().HasForeignKey(pf => pf.PluginId);
                pf.HasOne(pf => pf.Function).WithMany().HasForeignKey(pf => pf.FunctionId);
                pf.Property(pf => pf.IsRoot).IsRequired();
            });

            modelBuilder.Entity<Topic>(t =>
            {
                t.ToTable(nameof(Topics));
                t.HasKey(t => t.Id);
                t.HasOne(t => t.Owner).WithMany(p => p.Topics).HasForeignKey(t => t.OwnerId);
                t.Property(t => t.Visibility).HasConversion(v => v.ToString(), v => Enum.Parse<Visibility>(v)).HasDefaultValue(Visibility.Private);
            });
        }

        public void SeedDatabase(AgienceIdProvider idProvider, Dictionary<string, string> args)
        {
            if (Environment.GetEnvironmentVariable("AGIENCE_INITIALIZE")?.ToUpper() == "TRUE")
            {
                if (People.Any() || Hosts.Any() || Keys.Any())
                {
                    return;
                }

                // TODO: Use a Model
                var firstName = args["first_name"];
                var lastName = args["last_name"];
                var email = args["email"];
                var providerId = args["provider_id"];
                var providerPersonId = args["provider_person_id"];
                var hostName = args["host_name"];
                var webDomain = args["web_domain"];
                var webPort = args["web_port"];

                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff"); 

                var personId = idProvider.GenerateId(nameof(Person));
                var hostId = idProvider.GenerateId(nameof(Host));
                var keyId = idProvider.GenerateId(nameof(Key));

                var secret = HostSecretValidator.Random32ByteString();

                People.AddRange(
                    new Person
                    {
                        Id = personId,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        ProviderId = providerId,
                        ProviderPersonId = providerPersonId,
                        CreatedDate = DateTime.UtcNow
                    }
                );

                var redirectUris = new List<string> { $"https://{webDomain}{webPort}/signin-oidc" };
                var postLogoutUris = new List<string> { $"https://{webDomain}{webPort}/signout-callback-oidc" };

                Hosts.AddRange(new Host
                {
                    // Public Web Host

                    Id = hostId,
                    OwnerId = personId,
                    Name = hostName,
                    RedirectUris = string.Join(' ', redirectUris),
                    PostLogoutUris = string.Join(' ', postLogoutUris),
                    Scopes = new List<string> { "openid", "profile", "email", "manage", "connect" }, // TODO: Allow Configuration
                    Visibility = Core.Models.Enums.Visibility.Public,
                    CreatedDate = DateTime.UtcNow

                });

                Keys.AddRange(new Key
                {
                    Id = keyId,
                    SaltedValue = HostSecretValidator.HashSecret(secret, HostSecretValidator.Random32ByteString()),
                    Name = $"key_{timestamp}",
                    HostId = hostId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                });

                SaveChanges();

                Console.WriteLine($"HostId={hostId}");
                Console.WriteLine($"HostSecret={secret}");

                Environment.Exit(0);
            }
        }
    }
}