using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;

namespace Agience.Authority.Identity.Data
{
    public class AgiencePersonStore : IUserStore<Person>
    {
        private readonly IAgienceDataAdapter _dataAdapter;

        public AgiencePersonStore(IAgienceDataAdapter dataAdapter)
        {
            _dataAdapter = dataAdapter;
        }

        public async Task<IdentityResult> CreateAsync(Person person, CancellationToken cancellationToken)
        {
            return await _dataAdapter.CreateRecordAsync(person) != null ?
                IdentityResult.Success :
                IdentityResult.Failed(new IdentityError() { Description = "Could not create Person" });
        }

        public Task<IdentityResult> DeleteAsync(Person user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<Person?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var extracted = Person.ExtractProviderAndId(userId);

            if (extracted.HasValue)
            {
                var (provider, providerPersonId) = extracted.Value;
                return await _dataAdapter.GetPersonByExternalProviderIdAsync(provider, providerPersonId); // TODO: Implement cancellationTokens
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
                await _dataAdapter.UpdateRecordAsync(person);
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError() { Description = "Could not update Person" });
            }
            return IdentityResult.Success;
        }
    }
}
