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
    /// Interaction logic for ViewUnaccessablePages.xaml
    /// </summary>
    public partial class ViewUnaccessablePages : UserControl, ISwitchable
    {
        public ViewUnaccessablePages()
        {
            InitializeComponent();
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        private void ViewGameBoard_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new GameBoard());
        }

        private void ViewReceivedRequest_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new RequestReceived());
        }

        private void ViewEndScreen_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void ViewRematchRequestScreen_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new RematchRequest());
        }
    }
}
