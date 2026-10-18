using System.ServiceModel;

using Backgammon.Server.Services;

namespace Backgammon.Server.Services.Comunication
{
    internal class RegistrationServiceHost
    {
        private readonly ServiceHost _serviceHost;

        public RegistrationServiceHost()
        {
            _serviceHost = new ServiceHost(typeof(RegistrationService));
        }

        public void Open()  
        {
            _serviceHost.Open();
        }

        public void Close()
        {
            try
            {
                if (_serviceHost.State == CommunicationState.Faulted)
                {
                    _serviceHost.Abort();
                }
                else
                {
                    _serviceHost.Close();
                }
            }
            finally
            {
                if (_serviceHost.State != CommunicationState.Closed)
                {
                    _serviceHost.Abort();
                }
            }
        }
    }
}