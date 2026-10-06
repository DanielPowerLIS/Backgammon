using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backgammon.Client.Models
{
    public static class RegistrationLimits
    {
        private static readonly int MaximumTextFieldLength = 100;

        public static int TextFieldMaximumLength
        {
            get
            {
                return MaximumTextFieldLength;
            }
        }
    }
}
