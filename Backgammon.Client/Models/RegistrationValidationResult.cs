using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Client.Models
{
    internal sealed class RegistrationValidationResult
    {
        public bool IsValid { get; set; }

        public string ErrorTitle { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
