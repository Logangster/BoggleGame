using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace BoggleClient
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage : UserControl, ISwitchable
    {
        BoggleClientModel clientModel;

        public ConnectPage()
        {
            InitializeComponent();
            clientModel = new BoggleClientModel();
        }

        /// <summary>
        /// If user has a previous username and address
        /// </summary>
        /// <param name="userName">Name for user</param>
        /// <param name="address">Previous address user used</param>
        public ConnectPage(string userName, string address) : this()
        {
            userNameTextbox.Text = userName;
            ServerAddress.Text = address;
        }

        /// <summary>
        /// Event handler for the connect click
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">exception</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Check to make sure user put in a username
            if (!String.IsNullOrEmpty(userNameTextbox.Text) && !String.IsNullOrEmpty(ServerAddress.Text))
            {
                //Check to see if connection works
                if (!clientModel.Connect(ServerAddress.Text, 2000, userNameTextbox.Text))
                {
                    MessageBox.Show("Failed connection.");
                    return;
                }

                //Save client model in properties so that the other pages may access it
                Application.Current.Properties["clientModel"] = clientModel;
                Switcher.Switch(new WaitingForResponse());
            }
        }

        // Used as part of the iSwitchable interface
        public void UtilizeState(object state)
        {

        }
    }
}
