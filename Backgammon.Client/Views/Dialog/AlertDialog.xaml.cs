using System.Windows;
using System.Windows.Media;

using Backgammon.Client.Views.Models;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AlertDialog : Window
    {
        public AlertDialog(string title, string message, AlertType alertType)
        {
            InitializeComponent();

            Title = Properties.Resources.TextBlock_Backgammon;
            textBlockTitle.Text = title;
            textBlockMessage.Text = message;

            if (alertType == AlertType.Error)
            {
                SetErrorStyle();
            }
        }

        private void SetErrorStyle()
        {
            SolidColorBrush redBrush =
                new SolidColorBrush(Color.FromRgb(255, 59, 79));

            textBlockTitle.Foreground = redBrush;
            buttonAccept.Foreground = redBrush;
            buttonAccept.BorderBrush = redBrush;
        }

        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
