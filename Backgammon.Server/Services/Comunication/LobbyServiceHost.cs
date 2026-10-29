using System.ServiceModel;

namespace Backgammon.Server.Services.Comunication
{
    internal class LobbyServiceHost
    {
        private readonly ServiceHost _serviceHost;

        public LobbyServiceHost()
        {
            _serviceHost = new ServiceHost(typeof(LobbyService));
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