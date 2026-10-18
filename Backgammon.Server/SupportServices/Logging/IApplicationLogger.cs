using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Server.SupportServices.Logging
{
    public interface IApplicationLogger
    {
        void LogInformation(
            string messageTemplate,
            params object[] argumentValues);

        void LogWarning(
            string messageTemplate,
            params object[] argumentValues);

        void LogError( 
            Exception errorException,
            string messageTemplate,
            params object[] argumentValues);
    }
}
