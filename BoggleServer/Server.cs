using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CustomNetworking;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace BoggleServer
{
    /// <summary>
    /// Author: Logan Gore
    /// </summary>
    public class Server
    {
        //Edited from the Chat Server and Client Example.

        // Listens for incoming connections
        private TcpListener server;

        // the name associated with the socket
        private Queue<string> user_names;

        // Queue to make sure sockets are paired for a game
        private Queue<StringSocket> socketQueue;

        //HashSet used to hold all the words in the dictionary
        private HashSet<string> wordDic;

        //Time sent in to the command line
        private int time;

        //Sets whether the gameBoard was passed in via command line
        private string gameBoard = null;

        //New TcpListener specifically for the Web Server
        private TcpListener webServer;

        //Must set a connection string for the password and location of database
        //NOTE: I had to take it out for security sake of posting class project to github
        private string connectionString;



        /// <summary>
        ///  Launches a server that that runs on port 2500
        /// </summary>
        /// <param name="args">args passed in on command line</param>
        public static void Main(string[] args)
        {
            Server BoggleServer = new Server(2000, args);
            //Now start the web server
            BoggleServer.RunWebServer(2500);
            Console.ReadLine();
        }

        #region Boggle Sever
        /// <summary>
        /// Creates a Boggleserver that listens for connections on the provided port
        /// </summary>
        public Server(int port, string[] args)
        {
            server = new TcpListener(IPAddress.Any, port);
            user_names = new Queue<string>();
            socketQueue = new Queue<StringSocket>();
            server.Start();
            server.BeginAcceptSocket(ConnectionReceived, null);

            //Read in the dictionary file
            string[] wordDictionary = File.ReadAllLines(args[1]);

            wordDic = new HashSet<string>(wordDictionary);

            //Parse out the time that was sent via commandline
            int.TryParse(args[0], out time);

            //Checks if they entered in a custom board, and if that board is 16 characters long
            if (args.Length == 3 && args[2].Length == 16)
            {
                gameBoard = args[2];
            }

        }

        /// <summary>
        /// Deals with connection requests
        /// </summary>
        private void ConnectionReceived(IAsyncResult ar)
        {
            Socket socket = server.EndAcceptSocket(ar);
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);
            ss.BeginReceive(PlayerNameReceived, ss);
            Console.WriteLine("Accepted Connection");
            server.BeginAcceptSocket(ConnectionReceived, null);
        }

        /// <summary>
        /// Checks to see if the user sent in the command PLAY (username)
        /// If so, sets the players name, if not, sends ignoring command back to the client
        /// </summary>
        /// <param name="name">Name the user sent in via Play command</param>
        /// <param name="e">Exception</param>
        /// <param name="p">Payload</param>
        private void PlayerNameReceived(String name, Exception e, object p)
        {
            StringSocket ss = (StringSocket)p;

            if (e != null || name == null)
            {
                //There's been an exception close the socket
                if (socketQueue.Count > 1)
                    socketQueue.Dequeue();
                ss.Close();
                return;
            }


            lock (socketQueue)
            {
                string[] command = name.Trim().Split(' ');

                //Make sure they sent in the PLAY command
                if (command[0].Equals("PLAY", StringComparison.InvariantCultureIgnoreCase))
                {
                    user_names.Enqueue(command[1]);
                    socketQueue.Enqueue(ss);
                }
                else
                {
                    ss.BeginSend("IGNORING " + command[0] + "\n", (a, b) => { }, null);
                    ss.BeginReceive(PlayerNameReceived, ss);
                    return;
                }

                //If there's another existing socket, pair them up in a game
                while (socketQueue.Count > 1)
                {
                    StringSocket player1 = socketQueue.Dequeue();
                    StringSocket player2 = socketQueue.Dequeue();

                    Game newGame;

                    Console.WriteLine("Game started");
                    //Checks to see if a board was passed in on command line, if not don't pass into Game class
                    if (gameBoard == null)
                    {
                        newGame = new Game(user_names.Dequeue(), user_names.Dequeue(), player1, player2, wordDic, time);
                    }
                    else
                    {
                        newGame = new Game(user_names.Dequeue(), user_names.Dequeue(), player1, player2, wordDic, time, gameBoard);
                    }

                    player1.BeginReceive(CommandReceivedCallback, new Tuple<Game, StringSocket>(newGame, player1));
                    player2.BeginReceive(CommandReceivedCallback, new Tuple<Game, StringSocket>(newGame, player2));
                }
            }
        }

        /// <summary>
        /// Receives commands from the players once the game is setup
        /// </summary>
        /// <param name="command">Command sent from player client</param>
        /// <param name="e">Exception</param>
        /// <param name="socketGameTuple">Tuple that contains the socket and the game object</param>
        private void CommandReceivedCallback(String command, Exception e, object socketGameTuple)
        {
            Tuple<Game, StringSocket> player = (Tuple<Game, StringSocket>)socketGameTuple;

            //If both the command and exception are null, one of the clients closed their connection
            if (command == null && e == null)
            {
                StringSocket p2 = (StringSocket)player.Item1.p2;
                StringSocket p1 = (StringSocket)player.Item1.p1;

                //Other player closed socket, send a message out and close the remaining socket
                if ((StringSocket)player.Item2 == (StringSocket)player.Item1.p1)
                {
                    p2.BeginSend("TERMINATED\n", (a, b) => { }, null);
                    p2.Close();
                }
                else
                {
                    p1.BeginSend("TERMINATED\n", (a, b) => { }, null);
                    p1.Close();
                }
            }

            lock (socketQueue)
            {
                if (command != null)
                {
                    //Game object needs to parse the command sent in
                    player.Item1.ParseCommand(command, player.Item2);

                    //Receive another line now that the command has been sent in
                    player.Item2.BeginReceive(CommandReceivedCallback, new Tuple<Game, StringSocket>(player.Item1, player.Item2));
                }
            }

        }

        public void CloseServer()
        {
            server.Stop();
        }

        #endregion Boggle Server

        #region Web Server

        /// <summary>
        /// Begins an instance of a Web Server
        /// </summary>
        /// <param name="port"></param>
        private void RunWebServer(int port)
        {
            webServer = new TcpListener(IPAddress.Any, port);
            webServer.Start();
            webServer.BeginAcceptSocket(WebConnectionReceived, null);
        }

        /// <summary>
        /// Receives web connections and sets up a Begin Receive for the socket
        /// </summary>
        /// <param name="ar">Async result</param>
        private void WebConnectionReceived(IAsyncResult ar)
        {
            Socket acceptSocket = webServer.EndAcceptSocket(ar);
            StringSocket connectionSocket = new StringSocket(acceptSocket, UTF8Encoding.Default);
            connectionSocket.BeginReceive(HttpRequestReceived, connectionSocket);
            webServer.BeginAcceptSocket(WebConnectionReceived, null);
        }

        /// <summary>
        /// Retrieves the HTTP Request and finds the html to send back in response
        /// </summary>
        /// <param name="request">The request sent in</param>
        /// <param name="e">Possible Exception</param>
        /// <param name="payload">Payload</param>
        private void HttpRequestReceived(string request, Exception e, object payload)
        {
            StringSocket connectionSocket = (StringSocket)payload;
            string httpResponse = "HTTP/1.1 200 OK\r\nConnection: close\r\nContent-Type: text/html; charset=UTF-8\r\n";
            if (request != null && Regex.IsMatch(request, @"^GET"))
            {
                string[] splitRequest = request.Split(' ');

                string htmlFiller = HtmlForPage(splitRequest[1]);
                string html = String.Format(@"<html>
                                                <head><title>Boggle</title></head>
                                                <body><h1>BEST BOGGLE SITE EVER</h1><br/><a href='/players'>Home</a><br/>{0}</body>
                                                </html>", htmlFiller);

                connectionSocket.BeginSend(httpResponse, (a, b) => { }, null);
                connectionSocket.BeginSend("\r\n", (a, b) => { }, null);
                connectionSocket.BeginSend(html, (a, b) => { }, null);
            }

            connectionSocket.Close();
        }

        /// <summary>
        /// Retrieves the html for the body depending on the page requested
        /// </summary>
        /// <param name="page">The page requested</param>
        /// <returns></returns>
        private string HtmlForPage(string page)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Players Page
                if (page == "/players")
                {
                    return GeneratePlayersHtml(connection);
                }

                // Individual Player Page
                else if (Regex.IsMatch(page, @"/games\?player="))
                {
                    //Retrieve the index of where the name begins in the string
                    int nameIndex = page.IndexOf('=') + 1;
                    string name = page.Substring(nameIndex);

                    //Make sure that they put an actual name
                    if (name.Length >= 1)
                        return GeneratePlayerHtml(name, connection);
                }

                // Individual Game Page
                else if (Regex.IsMatch(page, @"/games\?id="))
                {
                    //Retrieve the index of where the ID is in the string
                    int nameIndex = page.IndexOf('=') + 1;
                    string gameId = page.Substring(nameIndex);

                    //Make sure that they put an actual id
                    if (gameId.Length >= 1)
                        return GenerateGameHtml(gameId, connection);
                }



                connection.Close();

                // In case the page isn't found
                return "<h1>404 Page not found</h1>";

                
            }

   
        }

        /// <summary>
        /// Generates the Html to display the Game Page
        /// </summary>
        /// <param name="gameId">Id of the game to display</param>
        /// <param name="connection">MySql connection used</param>
        /// <returns></returns>
        private string GenerateGameHtml(string gameId, MySqlConnection connection)
        {
            string html = "<h1>Game Page</h1><br/><br/><table>";

            MySqlCommand game = connection.CreateCommand();
            game.CommandText = "select * from Games where gId=" + Convert.ToInt32(gameId);

            using (MySqlDataReader reader = game.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Create new connection while ExecuteReader is running
                    using (MySqlConnection gameInfoConnection = new MySqlConnection(connectionString))
                    {
                        gameInfoConnection.Open();
                        MySqlCommand gameInfo = gameInfoConnection.CreateCommand();

                        //Get player 1 Name
                        gameInfo.CommandText = "select playerName from Players where pId=" + reader["p1Id"];
                        string player1Name = gameInfo.ExecuteScalar().ToString();

                        //Get player 2 Name
                        gameInfo.CommandText = "select playerName from Players where pId=" + reader["p2Id"];
                        string player2Name = gameInfo.ExecuteScalar().ToString();

                        //Construct Word Lists

                        //Select words that either the player 1 or 2 played
                        gameInfo.CommandText = String.Format("select * from Words where gId={0} AND (pId={1} OR pId={2})",
                            gameId, reader["p1Id"], reader["p2Id"]);

                        HashSet<string> p1Words = new HashSet<string>();
                        HashSet<string> p2Words = new HashSet<string>();
                        HashSet<string> wordsInCommon = new HashSet<string>();
                        HashSet<string> p1IllegalWords = new HashSet<string>();
                        HashSet<string> p2IllegalWords = new HashSet<string>();

                        

                        using (MySqlDataReader wordReader = gameInfo.ExecuteReader())
                        {
                            while (wordReader.Read())
                            {
                                //Get the word and player Id's for comparison
                                String word = wordReader["word"].ToString();
                                int playerId = Convert.ToInt32(wordReader["pId"]);
                                int otherPlayerId = Convert.ToInt32(reader["p1Id"]);

                                if (Convert.ToInt32(wordReader["legal"]) == 1)
                                {
                                    if (playerId == otherPlayerId)
                                    {
                                        //Check if they have the word in common
                                        if (p2Words.Contains(word))
                                        {
                                            wordsInCommon.Add(word);
                                            p2Words.Remove(word);
                                        }
                                        else
                                            p1Words.Add(word);
                                    }
                                    else
                                    {
                                        //Check if they have the word in common
                                        if (p1Words.Contains(word))
                                        {
                                            wordsInCommon.Add(word);
                                            p1Words.Remove(word);
                                        }
                                        else
                                            p2Words.Add(word);
                                    }
                                }
                                else
                                {
                                    if (playerId == otherPlayerId)
                                        p1IllegalWords.Add(word);
                                    else
                                        p2IllegalWords.Add(word);
                                }
                            }

                            
                        }

                        //Player names and links
                        html += String.Format("Player 1: <a href='/games?player={0}'>{0}</a><br/>",
                            player1Name);
                        html += String.Format("Player 2: <a href='/games?player={0}'>{0}</a><br/><br/>",
                            player2Name);

                        // Word lists and scores
                        html += player1Name + " Legal Words: " + String.Join(", ", p1Words.ToArray()) + "<br/>";
                        html += player2Name + " Legal Words: " + String.Join(", ", p2Words.ToArray()) + "<br/>";
                        html += "Common Legal Words: " + String.Join(", ", wordsInCommon.ToArray()) + "<br/>";
                        html += player1Name + " Illegal Words: " + String.Join(", ", p1IllegalWords.ToArray()) + "<br/>";
                        html += player2Name + " Illegal Words: " + String.Join(", ", p2IllegalWords.ToArray()) + "<br/><br/>";
                        html += "Player 1 Score: " + reader["p1Score"] + "<br/>";
                        html += "Player 2 Score: " + reader["p2Score"] + "<br/><br/>";

                        //Board Generation
                        html += "Board</br><table>";
                        string board = reader["board"].ToString();

                        int i = 0;

                        // Iterate each char for a 4x4 board
                        foreach (char c in board)
                        {   
                            if ((i % 4) == 0)
                            {
                                // hit the 4th column so end the row and start a new one
                                html += "</tr><tr><td>" + c + "</td>";
                            }
                            else
                            {
                                html += "<td>" + c + "</td>";
                            }

                            i++;
                        }

                     
                        html += "</table><br/><br/>";

                        
                    }

                    html += "Time Limit: " + reader["timeLimit"] + "<br/>";
                    html += "Date/Time Game Ended: " + reader["gameEnded"];
        
                  }    
            }
            return html += "</table>";
        }

        /// <summary>
        /// Generates Html for the Player's Game Page
        /// 
        /// Invariant: When you update the player 1 display code, you must also update player 2
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="connection">Connection used for MySql</param>
        /// <returns></returns>
        private String GeneratePlayerHtml(string name, MySqlConnection connection)
        {
            string html = @"<h1>" + name  + @"'s Game Page</h1><br/><br/>
                            <table>
                            <tr>
                            <td>Id/Link</td><td>Opponent</td><td>Player Score</td><td>Opponent Score</td>
                            </tr>";

            MySqlCommand games = connection.CreateCommand();
            games.CommandText = "select pId from Players where playerName= " + "'" + name + "'";
            int id = Convert.ToInt32(games.ExecuteScalar());


            // For games where they are player 1
            games.CommandText = "select * from Games where p1Id = " + id;

            using (MySqlDataReader reader = games.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Game ID and link
                    html += String.Format("<tr><td><a href='/games?id={0}'>{0}</a></td>",
                        reader["gId"]);

                    //Have to create a new connection to make a query while the executereader is running
                    using (MySqlConnection gameInfoConnection = new MySqlConnection(connectionString))
                    {
                        gameInfoConnection.Open();
                        MySqlCommand gameInfo = gameInfoConnection.CreateCommand();

                        //Select the players name
                        gameInfo.CommandText = "select playerName from Players where pId =" + reader["p2Id"];
                        string opponentName = gameInfo.ExecuteScalar().ToString();

                        //Output all the info
                        html += String.Format("<td><a href='/games?player={0}'>{0}</a></td><td>{1}</td><td>{2}</td></tr>",
                            opponentName, reader["p1Score"], reader["p2Score"]);
                    }

                    
                }

               
            }

            //Games where they are player 2
            games.CommandText = "select * from Games where p2Id = " + id;
            using (MySqlDataReader reader = games.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Game ID and link
                    html += String.Format("<tr><td><a href='/games?id={0}'>{0}</a></td>",
                        reader["gId"]);

                    using (MySqlConnection gameInfoConnection = new MySqlConnection(connectionString))
                    {
                        gameInfoConnection.Open();
                        MySqlCommand gameInfo = gameInfoConnection.CreateCommand();
                        gameInfo.CommandText = "select playerName from Players where pId =" + reader["p1Id"];
                        string opponentName = gameInfo.ExecuteScalar().ToString();

                        html += String.Format("<td><a href='/games?player={0}'>{0}</a></td><td>{1}</td><td>{2}</td></tr>",
                            opponentName, reader["p2Score"], reader["p1Score"]);
                    }

                    
                }
               
            }

            return html += "</table>";
        }

        /// <summary>
        /// Generates the Page displaying all the players
        /// </summary>
        /// <param name="connection">MySql connection used</param>
        /// <returns></returns>
        private string GeneratePlayersHtml(MySqlConnection connection)
        {
            string html = @"<h1>Players Page</h1><br/><br/><table><tr><td>ID</td><td>Name</td>
                            <td>Won</td><td>Lost</td><td>Tied</td></tr>";

            MySqlCommand command = connection.CreateCommand();
            command.CommandText = "select * from Players";

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    //Output the player id and player name
                    html += String.Format("<tr><td>{0}</td><td><a href='/games?player={1}'>{1}</a></td>",
                        reader["pId"], reader["playerName"]);

                    int gamesWonCount = 0;
                    int gamesLostCount = 0;
                    int gamesTiedCount = 0;

                    //Create new connection while ExecuteReader is running
                    using (MySqlConnection gameCountConnection = new MySqlConnection(connectionString))
                    {
                        gameCountConnection.Open();
                        MySqlCommand playerGames = gameCountConnection.CreateCommand();
                        //Games Won
                        playerGames.CommandText = String.Format(@"select COUNT(*) from Games where
                        (p1Id = {0} AND p1Score > p2Score) OR (p2Id = {0} AND p2Score > p1Score)", reader["pId"]);
                        gamesWonCount = Convert.ToInt32(playerGames.ExecuteScalar());
                        //Games Lost
                        playerGames.CommandText = String.Format(@"select COUNT(*) from Games where
                        (p1Id = {0} AND p1Score < p2Score) OR (p2Id = {0} AND p2Score < p1Score)", reader["pId"]);
                        gamesLostCount = Convert.ToInt32(playerGames.ExecuteScalar());
                        //Games Tied
                        playerGames.CommandText = String.Format(@"select COUNT(*) from Games where
                        (p1Id = {0} AND p1Score = p2Score) OR (p2Id = {0} AND p2Score = p1Score)", reader["pId"]);
                        gamesTiedCount = Convert.ToInt32(playerGames.ExecuteScalar());
                    }

                    html += String.Format("<td>{0}</td><td>{1}</td><td>{2}</td></tr>", gamesWonCount, gamesLostCount, gamesTiedCount);
                }

               
            }

            return html += "</table>";
        }

        #endregion Web Server
    }

}