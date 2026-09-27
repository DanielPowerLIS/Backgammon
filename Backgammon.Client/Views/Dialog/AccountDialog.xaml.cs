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

namespace Backgammon.Client.Views.Dialog
{
    public partial class AccountDialog : Window
    {
        public AccountDialog(string heading, string message)
        {
            InitializeComponent();

            HeadingText.Text = heading;
            MessageText.Text = message;
        }

        public void ShowOver(Window owner)
        {
            FrameworkElement ownerContent = (FrameworkElement)owner.Content;
            Point screenOrigin = ownerContent.PointToScreen(new Point(0, 0));
            PresentationSource presentationSource = PresentationSource.FromVisual(ownerContent);
            Point dialogOrigin = presentationSource.CompositionTarget
                .TransformFromDevice.Transform(screenOrigin);

            Owner = owner;
            Left = dialogOrigin.X;
            Top = dialogOrigin.Y;
            Width = ownerContent.ActualWidth;
            Height = ownerContent.ActualHeight;

            ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
