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
using Backgammon.Client.Utils;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AlertDialog : Window
    {
        public AlertDialog(string title, string message, string ButtonText, AlertType alertType)
        {
            InitializeComponent();

            Title = Properties.Resources.TextBlock_Backgammon;
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            BtnAccept.Content = ButtonText;

            if (alertType == AlertType.Error)
            {
                SetErrorStyle();
            }
        }
        private void SetErrorStyle()
        {
            SolidColorBrush redBrush =
                new SolidColorBrush(Color.FromRgb(255, 59, 79));

            TxtTitle.Foreground = redBrush;
            BtnAccept.Foreground = redBrush;
            BtnAccept.BorderBrush = redBrush;
        }
        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
