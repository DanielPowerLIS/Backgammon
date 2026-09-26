using Backgammon.Client.Views.Pages;
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
using Backgammon.Client.Utils;

using Backgammon.Client.Views.Dialog;
using Backgammon.Client.Views.Game;

namespace Backgammon.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new GamePage());
            //Loaded += MainWindow_Loaded;
        }

        //private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    AlertDialog alert = new AlertDialog(
        //        Properties.Resources.textBlockKickConnectionErrorTitle,
        //        Properties.Resources.textBlockKickConnectionErrorMessage,
        //        Properties.Resources.buttonClose,
        //        AlertType.Error
        //    );

        //    alert.Owner = this;
        //    alert.ShowDialog();
        //}

    }
}
