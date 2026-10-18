using System.Threading.Tasks;

using Backgammon.Client.Comunication;
using Backgammon.Client.RegistrationReference;

namespace Backgammon.Client.Controllers.Registration
{
    internal class RegistrationController
    {
        private readonly RegistrationClient _registrationClient;

        public RegistrationController()
        {
            _registrationClient = new RegistrationClient();
        }

        public Task RegisterAccountAsync(Account accountInformation)
        {
            Task registrationTask = _registrationClient.RegisterAccountAsync(accountInformation);

            return registrationTask;
        }
    }
}