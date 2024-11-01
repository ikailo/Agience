using Microsoft.EntityFrameworkCore;
using Agience.Authority.Identity.Models;
using Host = Agience.Authority.Identity.Models.Host;
using System.Text.Json;
using Duende.IdentityServer.Models;
using Agience.Authority.Identity.Validators;

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
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<AgentPlugin> AgentPlugins { get; set; }
        public DbSet<Authorizer> Authorizers { get; set; }
        public DbSet<Credential> Credentials { get; set; }
        public DbSet<AgentConnection> AgentConnections { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<Host> Hosts { get; set; }
        public DbSet<HostPlugin> HostPlugins { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<PluginConnection> PluginConnections { get; set; }
        public DbSet<PluginFunction> PluginFunctions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Agency>(a =>
            {
                a.ToTable(nameof(Agencies));
                a.HasKey(a => a.Id);
                a.HasOne(a => a.Director).WithMany(p => p.Agencies).HasForeignKey(a => a.DirectorId);
                a.HasMany(a => a.Agents).WithOne(a => a.Agency).HasForeignKey(a => a.AgencyId);
            });

            modelBuilder.Entity<Agent>(a =>
            {
                a.ToTable(nameof(Agents));
                a.HasKey(a => a.Id);
                a.HasOne(a => a.Agency).WithMany(a => a.Agents).HasForeignKey(a => a.AgencyId);
                a.HasOne(a => a.Host).WithMany(h => h.Agents).HasForeignKey(a => a.HostId);
                a.HasMany(a => a.Connections).WithOne(ac => ac.Agent).HasForeignKey(ac => ac.AgentId);
                a.HasMany(a => a.Plugins).WithMany(p => p.Agents).UsingEntity<AgentPlugin>();
            });

            modelBuilder.Entity<AgentConnection>(ac =>
            {
                ac.ToTable(nameof(AgentConnections));
                ac.HasKey(ac => new { ac.PluginConnectionId, ac.AgentId });
                ac.HasOne(ac => ac.Agent).WithMany(a => a.Connections).HasForeignKey(ac => ac.AgentId);
                ac.HasOne(ac => ac.Credential).WithMany().HasForeignKey(ac => ac.CredentialId);
                ac.HasOne(ac => ac.PluginConnection).WithMany().HasForeignKey(ac => ac.PluginConnectionId);
                ac.HasOne(ac => ac.Authorizer).WithMany().HasForeignKey(ac => ac.AuthorizerId);
            });

            modelBuilder.Entity<AgentPlugin>(ap =>
            {
                ap.ToTable(nameof(AgentPlugins));
                ap.HasKey(ap => new { ap.AgentId, ap.PluginId });
                ap.HasOne(ap => ap.Agent).WithMany().HasForeignKey(ap => ap.AgentId);
                ap.HasOne(ap => ap.Plugin).WithMany().HasForeignKey(ap => ap.PluginId);
            });

            modelBuilder.Entity<Authorizer>(a =>
            {
                a.ToTable(nameof(Authorizers));
                a.HasOne(a => a.Manager).WithMany(p => p.Authorizers).HasForeignKey(a => a.ManagerId);
            });

            modelBuilder.Entity<Credential>(c =>
            {
                c.ToTable(nameof(Credentials));
            });

            modelBuilder.Entity<Function>(f =>
            {
                f.ToTable(nameof(Functions));
                f.HasKey(f => f.Id);
                f.HasMany(f => f.PluginFunctions).WithOne(pf => pf.Function).HasForeignKey(pf => pf.FunctionId);
            });

            modelBuilder.Entity<Host>(h =>
            {
                h.ToTable(nameof(Hosts));
                h.HasKey(h => h.Id);
                h.HasOne(h => h.Operator).WithMany(p => p.Hosts).HasForeignKey(a => a.OperatorId);
                h.HasMany(h => h.Plugins).WithMany(p => p.Hosts).UsingEntity<HostPlugin>();
                h.Property(h => h.Scopes) // List of string
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                       v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                   );
            });

            modelBuilder.Entity<HostPlugin>(hp =>
            {
                hp.ToTable(nameof(HostPlugins));
                hp.HasKey(hp => new { hp.HostId, hp.PluginId });
                hp.HasOne(hp => hp.Host).WithMany().HasForeignKey(hp => hp.HostId);
                hp.HasOne(hp => hp.Plugin).WithMany().HasForeignKey(hp => hp.PluginId);
            });

            modelBuilder.Entity<Key>(k =>
            {
                k.ToTable(nameof(Keys));
                k.HasKey(k => k.Id);
                k.HasOne(k => k.Host).WithMany(h => h.Keys).HasForeignKey(h => h.HostId);
            });

            modelBuilder.Entity<Log>(lg =>
            {
                lg.HasKey(lg => lg.Id);
                lg.ToTable(nameof(Logs));
                lg.HasOne(lg => lg.Agent).WithMany().HasForeignKey(lg => lg.AgentId);
            });

            modelBuilder.Entity<Person>(p =>
            {
                p.ToTable(nameof(People));
                p.HasKey(p => p.Id);
                p.HasMany(p => p.Agencies).WithOne(a => a.Director).HasForeignKey(a => a.DirectorId);
                p.HasMany(p => p.Hosts).WithOne(h => h.Operator).HasForeignKey(p => p.OperatorId);
                p.HasMany(p => p.Plugins).WithOne(p => p.Creator).HasForeignKey(p => p.CreatorId);
            });

            modelBuilder.Entity<Plugin>(p =>
            {
                p.ToTable(nameof(Plugins));
                p.HasKey(p => p.Id);
                p.HasOne(p => p.Creator).WithMany(p => p.Plugins).HasForeignKey(p => p.CreatorId);
                p.HasMany(p => p.Agents).WithMany(a => a.Plugins).UsingEntity<AgentPlugin>();
                p.HasMany(p => p.Hosts).WithMany(h => h.Plugins).UsingEntity<HostPlugin>();
                p.HasMany(p => p.Connections).WithOne(c => c.Plugin).HasForeignKey(c => c.PluginId);
                p.HasMany(p => p.PluginFunctions).WithOne(pf => pf.Plugin).HasForeignKey(pf => pf.PluginId);
                p.HasGeneratedTsVectorColumn(p => p.DescriptionSearchVector, "english", p => new { p.Name, p.Description }).HasIndex(p => p.DescriptionSearchVector).HasMethod("GIN");
            });

            modelBuilder.Entity<PluginConnection>(c =>
            {
                c.ToTable(nameof(PluginConnections));
                c.HasOne(c => c.Plugin).WithMany(p => p.Connections).HasForeignKey(c => c.PluginId);
            });

            modelBuilder.Entity<PluginFunction>(pf =>
            {
                pf.ToTable(nameof(PluginFunctions));
                pf.HasKey(pf => new { pf.PluginId, pf.FunctionId });
                pf.HasOne(pf => pf.Plugin).WithMany(p => p.PluginFunctions).HasForeignKey(pf => pf.PluginId);
                pf.HasOne(pf => pf.Function).WithMany(f => f.PluginFunctions).HasForeignKey(pf => pf.FunctionId);
                pf.Property(pf => pf.IsRoot).IsRequired();
                // pf.HasIndex(pf => new { pf.FunctionId, pf.IsRoot }).IsUnique().HasFilter("IsRoot = true"); // Need to create manually
                // SQL:  CREATE UNIQUE INDEX "IX_PluginFunctions_FunctionId_IsRoot" ON "PluginFunctions" ("FunctionId", "IsRoot") WHERE "IsRoot" = true;
            });

            modelBuilder.Entity<Log>(lg => {
                lg.HasKey(lg => lg.Id);
                lg.ToTable(nameof(Logs));
                lg.HasOne(lg => lg.Agent).WithMany().HasForeignKey(lg =>lg.AgentId);
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
                        ProviderPersonId = providerPersonId
                    }
                );

                var redirectUris = new List<string> { $"https://{webDomain}{webPort}/signin-oidc" };
                var postLogoutUris = new List<string> { $"https://{webDomain}{webPort}/signout-callback-oidc" };

                Hosts.AddRange(new Host
                {
                    // Public Web Host

                    Id = hostId,
                    OperatorId = personId,
                    Name = hostName,
                    RedirectUris = string.Join(' ', redirectUris),
                    PostLogoutUris = string.Join(' ', postLogoutUris),
                    Scopes = new List<string> { "openid", "profile", "email", "manage", "connect" }, // TODO: Allow Configuration
                    Visibility = Core.Models.Entities.Visibility.Public
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