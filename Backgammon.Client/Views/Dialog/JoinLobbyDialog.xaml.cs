using System.Windows;

namespace Backgammon.Client.Views.Dialog
{
    public partial class JoinLobbyDialog : Window
    {
        

        public JoinLobbyDialog()
        {
            InitializeComponent();
        }

        public string RoomCode { get; private set; }

        public bool IsConfirmed { get; private set; }

        private void OnJoinClick(object sender, RoutedEventArgs e)
        {
            RoomCode = textBoxRoomCode.Text;
            IsConfirmed = true;

            Close();
        }
    }
}
