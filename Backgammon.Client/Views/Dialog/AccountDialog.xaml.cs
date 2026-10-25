using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class AccountDialog : Window
    {
        public AccountDialog(string heading, string message)
        {
            InitializeComponent();

            textBlockHeading.Text = heading;
            textBlockMessage.Text = message;
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

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
