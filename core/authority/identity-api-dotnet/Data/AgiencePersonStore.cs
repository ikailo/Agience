using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using Microsoft.AspNetCore.Identity;


namespace Agience.Authority.Identity.Data
{
    public class AgiencePersonStore : IUserStore<Person>
    {
        private RecordsRepository _repository;

        public AgiencePersonStore(RecordsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IdentityResult> CreateAsync(Person person, CancellationToken cancellationToken)
        {
            var createdRecord = await _repository.CreateRecordAsSystemAsync(person);
            return createdRecord != null
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "Could not create Person" });
        }

        public async Task<Person?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var extracted = Person.ExtractProviderAndId(userId);
            if (extracted.HasValue)
            {
                var (provider, providerPersonId) = extracted.Value;

                var result = await _repository.QueryRecordsAsSystemAsync<Person>(new() { 
                    { "ProviderId", provider }, 
                    { "ProviderPersonId", providerPersonId } 
                });

                return result.FirstOrDefault();

                
            }
            return null;
        }

        public async Task<IdentityResult> UpdateAsync(Person person, CancellationToken cancellationToken)
        {
            try
            {
                await _repository.UpdateRecordAsSystemAsync(person);
                return IdentityResult.Success;
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Could not update Person" });
            }
        }

        public Task<IdentityResult> DeleteAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Person?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetNormalizedUserNameAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetUserNameAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(Person user, string? normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(Person user, string? userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Modify Dispose to be a no-op, allowing DI to handle disposal
        public void Dispose()
        {
            // No-op to allow DI to manage the lifecycle of dependencies
        }
    }
}
