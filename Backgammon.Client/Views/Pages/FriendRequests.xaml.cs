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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backgammon.Client.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para FriendRequests.xaml
    /// </summary>
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
