using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNetworking;
using System.Timers;
using BB;
using MySql.Data.MySqlClient;
using System.Globalization;



namespace BoggleServer
{
    /// <summary>
    /// This is a game object that the BoggleServer uses in order to 
    /// keep track of multiple boggle games at once. It will take in a word, 
    /// process the word, and update the score based on what the word is worth
    /// and whether it is valid or not. 
    /// 
    /// Authors: Logan Gore and Chris Weeter
    /// </summary>
    class Game
    {
        // BoggleBoard object is created
        BoggleBoard board;

        // Names of the players are stored as strings
        String p1Name, p2Name;

        // Scores of the players are stored as ints
        int p1Score, p2Score;

        // This will hold the amount of game time left
        int time;

        // Hash sets that hold the words Player1 played, the words player2 played, the legal dictionary, the words that both players played, and the illegal words that
        // Player 1 and Player 2 played
        HashSet<String> p1Words, p2Words, legalDictionary, playedWords, p1Illegal, p2Illegal;

        // The sockets that P1 and P2 are using to communicate
        public StringSocket p1, p2;

        // Timer to count down the game time
        Timer secondTimer = new Timer(1000);

        int timeGame;

        /// <summary>
        /// This is a 6 parameter constructor for the Game class. It does not take in a predefined board.
        /// </summary>
        /// <param name="player1Name"> The name passed by Player 1</param>
        /// <param name="player2Name"> The name passed by Player 2</param>
        /// <param name="p1Socket"> The Client socket for Player 1</param>
        /// <param name="p2Socket"> The Client socket for Player 2</param>
        /// <param name="wordDictionary"> The dictionary of legal words contained in a hash set</param>
        /// <param name="gameTime"> The amount of Game Time that the game is going to go. </param>
        public Game(string player1Name, string player2Name, StringSocket p1Socket, StringSocket p2Socket, HashSet<string> wordDictionary, int gameTime)
        {

            // Player 1 and player 2 usernames are set to the usernames passed
            p1Name = player1Name;
            p2Name = player2Name;

            // Player 1 and player 2 scores are set to 0
            p1Score = 0;
            p2Score = 0;

            // Time is sent to the gametime that was passed to the server
            time = gameTime;
            timeGame = gameTime;

            // Legal Dictionary is set to the word dictionary that was passed. All words
            // are converted into ToLower form
            legalDictionary = new HashSet<string>();

            foreach (string word in wordDictionary)
            {
                legalDictionary.Add(word.ToLower());
            }

            // player1 and player2 sockets are set to the client sockets that were passed
            p1 = p1Socket;
            p2 = p2Socket;
            
            // Board is created with a random set of letters
            board = new BoggleBoard();

            // Hash sets are initialized
            p1Words = new HashSet<string>();
            p2Words = new HashSet<string>();
            playedWords = new HashSet<string>();
            p1Illegal = new HashSet<string>();
            p2Illegal = new HashSet<string>();

            // Start command is sent to both of the clients
            p1.BeginSend("START " + board.ToString() + " " + gameTime + " " + player2Name + "\n", (a, b) => { }, false);
            p2.BeginSend("START " + board.ToString() + " " + gameTime + " " + player1Name + "\n", (a, b) => { }, false);

            // Countdown is started on a new thread
            System.Threading.ThreadPool.QueueUserWorkItem(func => gameTimeCountdown());

        }

        /// <summary>
        /// This is a seven parameter constructor for the Game class. 
        /// </summary>
        /// <param name="player1Name"> The name passed by Player 1</param>
        /// <param name="player2Name"> The name passed by Player 2</param>
        /// <param name="p1Socket"> The Client socket for Player 1</param>
        /// <param name="p2Socket"> The Client socket for Player 2</param>
        /// <param name="wordDictionary"> The dictionary of legal words contained in a hash set</param>
        /// <param name="gameTime"> The amount of Game Time that the game is going to go. </param>
        /// <param name="gameBoard"> A game board that is specified by the server when it is created</param>
        public Game(string player1Name, string player2Name, StringSocket p1Socket, StringSocket p2Socket, HashSet<string> wordDictionary, int gameTime, string gameBoard)
        {
            // player 1 and player 2 names are set to the names that were passed
            p1Name = player1Name;
            p2Name = player2Name;

            // Player scores are initialized to 0
            p1Score = 0;
            p2Score = 0;

            // Time is set to the game time passed
            time = gameTime;
            timeGame = gameTime;

            // Legal Dictionary is set to the word dictionary that was passed. All words
            // are converted into ToLower form
            legalDictionary = new HashSet<string>();
            foreach( string word in wordDictionary)
            {
                legalDictionary.Add(word.ToLower());
            }
            


            // Sockets are set to the sockets that were passed
            p1 = p1Socket;
            p2 = p2Socket;

            // Board is created using the letters passed to the game class
            board = new BoggleBoard(gameBoard);

            // Hash sets are initialized for all the word lists that need to be updated as they change
            p1Words = new HashSet<string>();
            p2Words = new HashSet<string>();
            playedWords = new HashSet<string>();
            p1Illegal = new HashSet<string>();
            p2Illegal = new HashSet<string>();

            // Start message is sent to both of the clients to start the game
            p1.BeginSend("START " + board.ToString() + " " + gameTime + " " + player2Name + "\n", (a, b) => { }, null);
            p2.BeginSend("START " + board.ToString() + " " + gameTime + " " + player1Name + "\n", (a, b) => { }, null);


            // Countdown is started on a new thread.
            System.Threading.ThreadPool.QueueUserWorkItem(func => gameTimeCountdown());

        }

        /// <summary>
        /// Method to handle the interval timer. This is passed to a new thread via the constructor
        /// </summary>
        private void gameTimeCountdown()
        {
            // Creates an interval timer that calls an evnt each time a second passes
            secondTimer.Elapsed += OneSecondPasses;
            secondTimer.Enabled = true;

        }

        /// <summary>
        /// This method is the event that happens each time a second passes with the interval timer
        /// </summary>

        private void OneSecondPasses(Object source, ElapsedEventArgs e)
        {
            // Each time a second passes, time is reduced by 1
            time--;

            int p1Id = 0;
            int p2Id = 0;
            int gId;


            p1.BeginSend("TIME " + time + '\n', (a, b) => { }, null);
            p2.BeginSend("TIME " + time + '\n', (a, b) => { }, null);

            // When time reaches zero, sends final score and final message
            if (time == 0)
            {
                if (p1.Connected == true && p2.Connected == true)
                {
                    // These variables are declared in here, as if they were declared
                    // outside of the if statement, then they would be declared and set
                    // each time the interval timer called this event, wasting system resources.
                    // These are strings that will hold the words from each of the hash sets, with
                    // spaces in between each word
                    string p1PlayedWords = "";
                    string p2PlayedWords = "";
                    string commonWords = "";
                    string p1IllegalWords = "";
                    string p2IllegalWords = "";

                    // Sets the interval timer enabled flag to false
                    secondTimer.Enabled = false;

                    // Sends the final score
                    p1.BeginSend("SCORE " + p1Score + " " + p2Score + "\n", (a, b) => { }, false);
                    p2.BeginSend("SCORE " + p2Score + " " + p1Score + "\n", (a, b) => { }, false);


                    // foreach loop to conver the hashet for p2 legal words into a string of all the words separated by a comma
                    foreach (string word in p1Words)
                    {

                        p1PlayedWords = p1PlayedWords + word + " ";
                    }

                    // foreach loop to convert the hashset for p2 legal words into a string of all the words separated by a comma
                    foreach (string word in p2Words)
                    {

                        p2PlayedWords = p2PlayedWords + word + " ";
                    }

                    // foreach loop to convert the hashset for words played in common into a string of all the words separated by a comma
                    foreach (string word in playedWords)
                    {

                        commonWords = commonWords + word + " ";
                    }

                    // foreach loop to convert the hashset for p1 illegal words into a string of all the words separated by a comma
                    foreach (string word in p1Illegal)
                    {

                        p1IllegalWords = p1IllegalWords + word + " ";
                    }

                    // foreach loop to convert the hashset for p2 illegal words into a string of all the words separated by a comma
                    foreach (string word in p2Illegal)
                    {

                        p2IllegalWords = p2IllegalWords + word + " ";
                    }

                    // Sends the final message. Final message includes "STOP" command, and then 
                    p1.BeginSend("STOP " + p1Words.Count + " " + p1PlayedWords.Trim() + " " + p2Words.Count + " " + p2PlayedWords.Trim() + " " + playedWords.Count + " " + commonWords.Trim() + " " + p1Illegal.Count + " " + p1IllegalWords.Trim() + " " + p2Illegal.Count + " " + p2IllegalWords.Trim() + "\n", (a, b) => { }, true);
                    p2.BeginSend("STOP " + p2Words.Count + " " + p2PlayedWords.Trim() + " " + p1Words.Count + " " + p1PlayedWords.Trim() + " " + playedWords.Count + " " + commonWords.Trim() + " " + p2Illegal.Count + " " + p2IllegalWords.Trim() + " " + p1Illegal.Count + " " + p1IllegalWords.Trim() + "\n", (a, b) => { }, true);



                    //Database connection
                    string connectionString = "server=atr.eng.utah.edu;database=cs3500_weeter;uid=cs3500_weeter;password=984751090;Convert Zero Datetime=True";

                    // Connect to the DB
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            conn.Open();

                            // Create a command
                            MySqlCommand command = conn.CreateCommand();


                            // Object to get the data
                            object queryReturn;

                            // Command to select the pId
                            command.CommandText = String.Format("SELECT pId FROM Players WHERE playerName= '{0}'", p1Name);
                            queryReturn = command.ExecuteScalar();

                            // If query is null, inserts the player
                            if (queryReturn == null)
                            {

                                command.CommandText = String.Format("INSERT INTO Players (playerName) VALUES ('{0}')", p1Name);
                                command.ExecuteNonQuery();

                            }




                            // Attempts to select the ID for player 2
                            command.CommandText = String.Format("SELECT pId FROM Players WHERE playerName= '{0}'", p2Name);
                            queryReturn = command.ExecuteScalar();

                            // If not, inserts player 2
                            if (queryReturn == null)
                            {

                                command.CommandText = String.Format("INSERT INTO Players (playerName) VALUES ('{0}')", p2Name);
                                command.ExecuteNonQuery();

                            }

                            // Gets the pID for both players
                            command.CommandText = String.Format("SELECT pId FROM Players WHERE playerName= '{0}'", p1Name);
                            queryReturn = command.ExecuteScalar();


                            int.TryParse(queryReturn.ToString(), out p1Id);


                            command.CommandText = String.Format("SELECT pId FROM Players WHERE playerName= '{0}'", p2Name);
                            queryReturn = command.ExecuteScalar();

                            int.TryParse(queryReturn.ToString(), out p2Id);



                            // Formats the date into something mySql can use
                            string formatForMySql = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            // Inserts the game data into the game table
                            command.CommandText = String.Format("INSERT INTO Games (p1Id, p2Id, p1Score, p2Score, board, gameEnded, timeLimit) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", p1Id, p2Id, p1Score, p2Score, board, formatForMySql, timeGame);
                            command.ExecuteNonQuery();

                            // Gets the game ID
                            command.CommandText = String.Format("SELECT LAST_INSERT_ID()");
                            queryReturn = command.ExecuteScalar();

                            // If the game ID was not successfully received, throws an exception
                            if (queryReturn != null)
                            {
                                int.TryParse(queryReturn.ToString(), out gId);
                            }
                            else
                                throw new Exception("Problem getting game ID from Database");



                            // foreach loop to convert the hashset for p1 legal words into a string of all the words separated by a comma
                            foreach (string word in p1Words)
                            {

                                command.CommandText = String.Format("INSERT INTO Words (gId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p1Id, 1);
                                command.ExecuteNonQuery();


                            }

                            // foreach loop to convert the hashset for p2 legal words into a string of all the words separated by a comma
                            foreach (string word in p2Words)
                            {

                                command.CommandText = String.Format("INSERT INTO Words (gId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p2Id, 1);
                                command.ExecuteNonQuery();

                            }

                            // foreach loop to convert the hashset for words played in common into a string of all the words separated by a comma
                            foreach (string word in playedWords)
                            {

                                command.CommandText = String.Format("INSERT INTO Words (gId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p1Id, 1);
                                command.ExecuteNonQuery();

                                command.CommandText = String.Format("INSERT INTO Words (gId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p2Id, 1);
                                command.ExecuteNonQuery();

                            }

                            // foreach loop to convert the hashset for p1 illegal words into a string of all the words separated by a comma
                            foreach (string word in p1Illegal)
                            {
                                command.CommandText = String.Format("INSERT INTO Words (gId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p1Id, 0);
                                command.ExecuteNonQuery();

                            }

                            // foreach loop to convert the hashset for p2 illegal words into a string of all the words separated by a comma
                            foreach (string word in p2Illegal)
                            {
                                command.CommandText = String.Format("INSERT INTO Words (gameId, word, pId, legal) VALUES ('{0}', '{1}', '{2}', '{3}')", gId, word, p2Id, 0);
                                command.ExecuteNonQuery();

                            }


                            // This thread sleep allows gives the client adequate time to get the message before closing the clients
                            System.Threading.Thread.Sleep(1000);

                            // Closes the sockets after the final message has been terminated
                            p1.Close();
                            p2.Close();


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                         
            }
        }

        public void ParseCommand(string word, StringSocket sendingSocket)
        {
            // Ensures that the word is not null
            if (word != null)
            {
                // Trims the word to make sure there is no extra white space
                word = word.Trim().ToLower();

                // Variables set up to see if the score changed
                int p1TempScore, p2TempScore;

                // bool variables are set up to see which client sent the word
                bool player1;
                bool player2;

                // bool variables are set depending on who sent the word command
                player1 = (sendingSocket == p1);
                player2 = (sendingSocket == p2);

                // String is split at the space in order to evaluate the command
                string[] splitCommand = word.Split(' ');

                // If the command that was passed was "word", it processes the word
                if (splitCommand[0] == "word")
                {
                    // Checks to see if the word is less than 3 characters long before processing it
                    if (splitCommand[1].Length >= 3)
                    {
                        p1TempScore = p1Score;
                        p2TempScore = p2Score;

                        // If player 1 sent the word, processes the word as such
                        if (player1 == true)
                        {
                            processScore(splitCommand[1], p1Words, p2Words, p1Illegal, p2Illegal, ref p1Score, ref p2Score);
                        }
                        // Else if player 2 sent the word, processes the word as such
                        else if (player2 == true)
                        {

                            processScore(splitCommand[1], p2Words, p1Words, p2Illegal, p1Illegal, ref p2Score, ref p1Score);
                        }

                        if (p1TempScore != p1Score || p2TempScore != p2Score)
                        {
                            // After the word has been processed, sends the updated score to both of the clients
                            p1.BeginSend("SCORE " + p1Score + " " + p2Score + "\n", (a, b) => { }, false);
                            p2.BeginSend("SCORE " + p2Score + " " + p1Score + "\n", (a, b) => { }, false);
                        }
                    }
                }

                // Else, the client is sent the "Ignoring" command to tell the client that they were not following protocol
                else
                {
                    sendingSocket.BeginSend("IGNORING" + splitCommand[0] + "\n", (a, b) => { }, false);
                }
            }
        }

        /// <summary>
        /// Method that processes the word to see if either of the user gains points or loses points, and handles adding the words to each hashset as neccesary
        /// </summary>
        /// <param name="word"> The word that was passed to the server from the client</param>
        /// <param name="playerPlayed"> List of words that the player who sent the word has played</param>
        /// <param name="otherPlayer"> List of words that the other player has played</param>
        /// <param name="playerIllegal"> List of illegal words that the player who sent the word has played</param>
        /// <param name="otherIllegal"> List of illegal words that the other player has played</param>
        /// <param name="playerScore"> Score of the player that played the word</param>
        /// <param name="otherScore"> Score of the other player </param>
        private void processScore(string word, HashSet<string> playerPlayed, HashSet<string> otherPlayer, HashSet<string> playerIllegal, HashSet<string> otherIllegal, ref int playerScore, ref int otherScore)
        {
            // If the word cannot be formed, the player's score who played the word is reduced by 1, and the word is added to the illegal hashset
            if (!board.CanBeFormed(word) && !playerIllegal.Contains(word))
            {
                playerScore = playerScore - 1;
                playerIllegal.Add(word);
            }
            // Else if the other player has already played this word, it is removed from there list and added
            // To the common list. The other player's score is reduced by the point value of the word
            else if (otherPlayer.Contains(word))
            {
                playedWords.Add(word);

                otherPlayer.Remove(word);

                otherScore = otherScore - scoreWord(word);

            }
            // If the player's played list, or common played words list or player's illegal list contains the word, ignore the word
            else if (playerPlayed.Contains(word) || playedWords.Contains(word) || playerIllegal.Contains(word))
            {
                // do Nothing
            }
            // If the word is in the legal dictionary, add the word to the player played hash, and add the value to the player's score
            else if (legalDictionary.Contains(word))
            {
                playerPlayed.Add(word);

                playerScore = playerScore + scoreWord(word);
            }
            // If none of the above conditions are true, the word is illegal. Score is reduced by 1, and the word is added to the illegal list
            else
            {
                playerScore = playerScore - 1;
                playerIllegal.Add(word);
            }
        }

        /// <summary>
        /// Determines point value of the word passed
        /// </summary>
        /// <param name="word"> Word being processed </param>
        /// <returns></returns>
        private int scoreWord(string word)
        {
            // If the word has a length under 3 letters long, returns 0
            if (word.Length < 3)
                return 0;

            // Else if the word length is 3 or 4, returns 1
            else if (word.Length == 3 || word.Length == 4)
                return 1;

            // Else if the word length is 5, returns 2
            else if (word.Length == 5)
                return 2;

            // Else if the word length is 6, returns 3
            else if (word.Length == 6)
                return 3;

            // Else if the word length is 7, returns 5
            else if (word.Length == 7)
                return 5;

            // Else returns 11. This should only be used if the word length is greater than 7
            else
                return 11;
        }


    }
}
