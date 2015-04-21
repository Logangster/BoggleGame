using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for WaitingForResponse.xaml
    /// </summary>
    public partial class WaitingForResponse : UserControl, ISwitchable
    {
        private BoggleClientModel clientModel;

        /// <summary>
        /// Loads in the client model and sets up a incomingWordEvent handler
        /// </summary>
        public WaitingForResponse()
        {
            InitializeComponent();

            clientModel = (BoggleClientModel) Application.Current.Properties["clientModel"];
            clientModel.incomingWordEvent += HandleGameRequest;        
        }

        /// <summary>
        /// Event handler that handles the incoming words
        /// </summary>
        /// <param name="line">Incoming word</param>
        /// <param name="e">Exception</param>
        private void HandleGameRequest(string line, Exception e)
        {
            Dispatcher.Invoke(new Action(() => GameStart(line, clientModel)));
        }

        /// <summary>
        /// Cancels the connection
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Exception</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            clientModel.CloseClient();
            clientModel.incomingWordEvent = null;
            Switcher.Switch(new ChooseOpponent());
        }

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Starts the game if the command sent in is START
        /// </summary>
        /// <param name="line">Command sent in</param>
        /// <param name="model">The client model</param>
        private void GameStart(string line, BoggleClientModel model)
        { 
            //If start, start a new game
            if (line != null && Regex.IsMatch(line, @"^START"))
            {
                Switcher.Switch(new GameBoard(line));
            }
            // In case of early termination
            else if(line != null && Regex.IsMatch(line, @"^Terminated"))
            {
                Switcher.Switch(new ConnectPage());
            }
        }
    }
}
