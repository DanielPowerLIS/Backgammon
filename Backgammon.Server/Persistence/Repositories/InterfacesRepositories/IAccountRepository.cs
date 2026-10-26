using System.Collections.Generic;
using System.Threading.Tasks;

using Backgammon.Server.Persistence.Data;

namespace Backgammon.Server.Persistence.Repositories.InterfacesRepositories
{
    public interface IAccountRepository
    {
        Task<List<Account>> GetAccountsByIdentifierAsync(
            string accountIdentifier);

        Task<bool> IsEmailRegisteredAsync(string emailAddress);

        Task<bool> IsUsernameRegisteredAsync(string playerUsername);

        Task CreateAccountAsync(Account playerAccount);
    }
}