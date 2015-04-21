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
    /// Interaction logic for GameBoard.xaml
    /// </summary>
    public partial class GameBoard : UserControl, ISwitchable
    {
        //Model for the client
        private BoggleClientModel clientModel;

        /// <summary>
        /// Creates a new game 
        /// </summary>
        /// <param name="board">Game board to use</param>
        /// <param name="name">Users name</param>
        public GameBoard(string board)
        {
            clientModel = (BoggleClientModel)Application.Current.Properties["clientModel"];

            InitializeComponent();

            p1Name.Text = clientModel.clientName;

            //Set up the event handler for incoming words
            clientModel.incomingWordEvent = null;
            clientModel.incomingWordEvent += ReceiveCommand;

            DisplayBoard(board);
        }
        /// <summary>
        /// Method that takes in the begin command and sets the gameboard text boxes
        /// </summary>
        /// <param name="board">The board setup to use</param>
        private void DisplayBoard(string board)
        {
            // Initializes the scores to 0
            p1Score.Text = "0";
            p2Score.Text = "0";


            // split command and send array to various areas of the code

            string[] startCommand = board.Split(' ');

            // Finds p2 name and puts it into the appropriate text box
            p2Name.Text = startCommand[3];

            // Initializes the time remaining box
            timeRemaining.Text = startCommand[2];

            // Gets the char array for the gameboard
            char[] gameBoard = startCommand[1].ToCharArray();


            // Logic ensures Q is replaced with Qu
            string[] gameBoardString = new string[16];
            // Checks to see if Q was passed and turns it into Qu if so
            for (int i = 0; i < 16; i++)
            {
                if (gameBoard[i] == 'Q')
                {
                    gameBoardString[i] = "Qu";
                }
                else
                {
                    gameBoardString[i] = gameBoard[i].ToString();
                }
            }


            // Sets all the spots to the appropriate letter
            spot11.Text = gameBoardString[0];
            spot12.Text = gameBoardString[1];
            spot13.Text = gameBoardString[2];
            spot14.Text = gameBoardString[3];
            spot21.Text = gameBoardString[4];
            spot22.Text = gameBoardString[5];
            spot23.Text = gameBoardString[6];
            spot24.Text = gameBoardString[7];
            spot31.Text = gameBoardString[8];
            spot32.Text = gameBoardString[9];
            spot33.Text = gameBoardString[10];
            spot34.Text = gameBoardString[11];
            spot41.Text = gameBoardString[12];
            spot42.Text = gameBoardString[13];
            spot43.Text = gameBoardString[14];
            spot44.Text = gameBoardString[15];
        }


      
        /// <summary>
        /// Event handler for incomingWordEvent
        /// </summary>
        /// <param name="command">Command sent in</param>
        /// <param name="e">Exception</param>

        private void ReceiveCommand(string command, Exception e)
        {

            Dispatcher.Invoke(new Action(() => CommandReceived(command, e)));
        }
 

        /// <summary>
        /// Receives the command and deals with it
        /// </summary>
        /// <param name="command">Command sent in</param>
        /// <param name="e">Exception</param>
        private void CommandReceived(string command, Exception e)
        {
            string[] scores = new string[2];

            // If an exception is given, displays a message and closes the client
            if (e != null)
            {
                MessageBox.Show("Server connection has closed.");
                clientModel.CloseClient();
                Switcher.Switch(new ChooseOpponent());
            }
            // If the command is not null, parses the command
            if (command != null)
            {
                //Update time
                if (Regex.IsMatch(command, @"^TIME"))
                {
                    string[] commandList = command.Split(' ');

                    timeRemaining.Text = commandList[1];
                }
                //Update score
                else if (Regex.IsMatch(command, @"^SCORE"))
                {
                    scores = command.Split(' ');

                    p1Score.Text = scores[1];
                    p2Score.Text = scores[2];
                }
                //Game ended
                else if(Regex.IsMatch(command, @"^STOP"))
                {
                    Switcher.Switch(new GameEnds(command, p1Score.Text, p2Score.Text, p1Name.Text, p2Name.Text));

                    clientModel.CloseClient();

                }
                //Ignore command
                else if (Regex.IsMatch(command, @"^IGNORING"))
                {
                    string[] ignoring = command.Split(' ');
                    MessageBox.Show("Invalid command: " + ignoring[1]);
                }
                //Game has been terminated
                else if (Regex.IsMatch(command, @"^TERMINATED"))
                {
                    MessageBox.Show("Opponent has closed connection.");
                    Switcher.Switch(new ChooseOpponent());
                }
            }
        }


        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Submits words
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">Exception</param>
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(InputWord.Text))
            {
                clientModel.SendWord(InputWord.Text);
                //Clear the textbox now that the word has been sent
                InputWord.Text = "";
            }
        }

    }
}
