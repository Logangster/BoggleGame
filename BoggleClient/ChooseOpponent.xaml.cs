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

namespace BoggleClient
{
    /// <summary>
    /// Interaction logic for SendRequest.xaml
    /// </summary>
    public partial class ChooseOpponent : UserControl, ISwitchable
    {
        public ChooseOpponent()
        {
            InitializeComponent();
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends the page back to the connect page with the name and address the client is using
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Exception</param>
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            //If the user already entered information, send it back to the connection page
            if (Application.Current.Properties["clientModel"] != null)
            {
                BoggleClientModel clientModel = (BoggleClientModel)Application.Current.Properties["clientModel"];
                Switcher.Switch(new ConnectPage(clientModel.clientName, clientModel.address));
            }
            else
            {
                Switcher.Switch(new ConnectPage());
            }
            
        }
    }
}
