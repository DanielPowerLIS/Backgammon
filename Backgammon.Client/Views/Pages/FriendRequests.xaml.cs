using System.Windows.Controls;

namespace Backgammon.Client.Views.Pages
{
    public partial class FriendRequests : Page
    {
        public FriendRequests()
        {
            InitializeComponent();

            listViewFriendRequests.Items.Add("Solicitud de amistad 1");
            listViewFriendRequests.Items.Add("Solicitud de amistad 2");
            listViewFriendRequests.Items.Add("Solicitud de amistad 3");
            listViewFriendRequests.Items.Add("Solicitud de amistad 4");
        }
    }
}
