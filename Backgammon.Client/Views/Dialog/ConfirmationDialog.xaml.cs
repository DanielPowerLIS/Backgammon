using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog(string message)
        {
            InitializeComponent();
            textBlockMessage.Text = message;
        }

        public bool ShowOver(Window owner)
        {
            FrameworkElement ownerContent = (FrameworkElement)owner.Content;

            Point screenOrigin = ownerContent.PointToScreen(new Point(0, 0));

            PresentationSource presentationSource = PresentationSource.FromVisual(ownerContent);

            Point dialogOrigin = presentationSource.CompositionTarget.TransformFromDevice.Transform(screenOrigin);

            Owner = owner;
            Left = dialogOrigin.X;
            Top = dialogOrigin.Y;
            Width = ownerContent.ActualWidth;
            Height = ownerContent.ActualHeight;

            return ShowDialog() == true;
        }

        private void OnConfirmClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
