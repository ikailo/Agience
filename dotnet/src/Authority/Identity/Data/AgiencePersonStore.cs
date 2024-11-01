using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Agience.Authority.Identity.Data
{
    public class AgiencePersonStore : IUserStore<Person>
    {
        private IAgienceDataAdapter _dataAdapter;

        public AgiencePersonStore(IAgienceDataAdapter dataAdapter)
        {
            _dataAdapter = dataAdapter;
        }

        public async Task<IdentityResult> CreateAsync(Person person, CancellationToken cancellationToken)
        {
            var createdRecord = await _dataAdapter.CreateRecordAsync(person);
            return createdRecord != null
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "Could not create Person" });
        }

        public Task<IdentityResult> DeleteAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // Modify Dispose to be a no-op, allowing DI to handle disposal
        public void Dispose()
        {
            // No-op to allow DI to manage the lifecycle of dependencies
        }

        public async Task<Person?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var extracted = Person.ExtractProviderAndId(userId);
            if (extracted.HasValue)
            {
                var (provider, providerPersonId) = extracted.Value;
                return await _dataAdapter.GetPersonByExternalProviderIdAsync(provider, providerPersonId, cancellationToken);
            }
            return null;
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

        public async Task<IdentityResult> UpdateAsync(Person person, CancellationToken cancellationToken)
        {
            try
            {
                await _dataAdapter.UpdateRecordAsync(person, cancellationToken);
                return IdentityResult.Success;
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Could not update Person" });
            }
        }
    }
}
