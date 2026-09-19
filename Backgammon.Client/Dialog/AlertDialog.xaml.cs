using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Backgammon.Client.Dialog
{
    public partial class AlertDialog : Window
    {
        public AlertDialog(string title, string message, string ButtonText)
        {
            InitializeComponent();

            TxtTitle.Text = title;
            TxtMessage.Text = message;
            BtnAccept.Content = ButtonText;

        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
