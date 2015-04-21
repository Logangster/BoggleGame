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
    /// Interaction logic for GameEnds.xaml
    /// </summary>
    public partial class GameEnds : UserControl, ISwitchable
    {

        /// <summary>
        /// Constructor that takes in the command, p1score, p2score, p1username, and p2username in order to update the end game board
        /// </summary>
        /// <param name="command"></param>
        /// <param name="p1score1"></param>
        /// <param name="p2score2"></param>
        /// <param name="p1UserName"></param>
        /// <param name="p2UserName"></param>
        public GameEnds(string command, string p1score1, string p2score2, string p1UserName, string p2UserName)
        {
            InitializeComponent();

            // The command is split at the spaces
            string[] commandList = command.Split(' ');
            int index;

            // Scores textboxes are set
            p2Score.Text = p2score2;
            p1Score.Text = p1score1;

            // Names are set
            p1Name.Text = p1UserName;
            p2Name.Text = p2UserName;

            // Strings to store the words passed
            List<string> p1Legal = new List<string>();
            List<string> p2Legal = new List<string>();
            List<string> common = new List<string>();
            List<string> p1Illegal = new List<string>();
            List<string> p2Illegal = new List<string>();

            /*This was the original logic that we tried, however we were unable to get it to work
             * We ended up going with while loops instead.
             * 
             * 
             * 
             *int.TryParse(commandList[1], out index);
           
                
            for (int i = 2; i < 2 + index; i++)    
            {
            
                p1LegalWordList.Text += commandList[i] + "\n";
                
            }
          
            int.TryParse(commandList[2 + index], out newIndex);

            
                
            for (int i = 3 + index; i < 3 + index + newIndex; i++)
            {
            
                p2LegalWordList.Text += commandList[i] + "\n";
                
            }

            index += newIndex;
            int.TryParse(commandList[3 + index], out newIndex);
           
                
            for (int i = 4 + index; i < 4 + index + newIndex; i++)    
            {
            
                commonWordList.Text += commandList[i] + "\n";
                
            }

            index += newIndex;
            int.TryParse(commandList[4 + index], out newIndex);
            
            for (int i = 5 + index; i < 5 + index + newIndex; i++)            
            {
            
                p1IllegalWordList.Text += commandList[i] + "\n";    
            }
            index += newIndex;
            
            int.TryParse(commandList[5 + index], out newIndex);
            for (int i = 6 + index; i < 6 + index + newIndex; i++)
            {
            
                p2IllegalWordList.Text += commandList[i] + "\n";
                
            }*/

            
            // Sets the offset
           int i = 2;

            // While loop to set the p1 Legal words
            while(!int.TryParse(commandList[i], out index))
            {
                p1Legal.Add(commandList[i]);    
                i++;
            }
             
            // When it finds a number, it increments i by one so it is not added to the list
            i += 1;
                
            // while loop to set the p2 legal words list
            while(!int.TryParse(commandList[i], out index))    
            {
                    
                p2Legal.Add(commandList[i]);    
                i++;
                
            }
                
            // When it finds a number, increments i by one so that it is not added to the list
            i += 1;

            // while loop to set the common word list
            while(!int.TryParse(commandList[i], out index))
            {
                common.Add(commandList[i]);
                i++;
            }
                
            // when it finds a number, increments i by one so that it is not added to the list
            i += 1;
            
            // while loop to set the p1 illegal word list
            while(!int.TryParse(commandList[i], out index))
           {
                    
                p1Illegal.Add(commandList[i]);    
                i++;
           }

            // when it finda a number, increments i by one so that it is not added to the list
            i += 1;
            
            // while loop to set the p2 illegal word list
            while(i < commandList.Length)
            {
                p2Illegal.Add(commandList[i]);
                i++;
            }
            

            // outputs each string contained in the p1Legal list
            foreach(string s in p1Legal)
            {
                p1LegalWordList.Text += s + "\n";
            }
            // outputs each string contained in the p2Legal list
            foreach (string s in p2Legal)
            {
                p2LegalWordList.Text += s + "\n";
            }
            // outputs each string contained in the common list
            foreach (string s in common)
            {
                commonWordList.Text += s + "\n";
            }
            // outputs each string contained in the p1Illegal list
            foreach (string s in p1Illegal)
            {
                p1IllegalWordList.Text += s + "\n";
            }
            // outputs each string contained in the p2Illegal list
            foreach (string s in p2Illegal)
            {
                p2IllegalWordList.Text += s + "\n";
            }
            
        }
        // When user clicks "okay", goes back to the choose opponent page
        private void Okay_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new ChooseOpponent());
        }

        // used for the Iswitchable interface
        public void UtilizeState(object state)
        {

        }
    }
}
