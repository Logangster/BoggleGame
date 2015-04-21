using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BoggleClient
{
    /// <summary>
    /// Model for the Boggle Client, handles all the socket connections, sending, and receiving logic
    /// </summary>
    public class BoggleClientModel
    {
        //Used for holding the StringSocket
        private StringSocket clientSocket { get; set; }

        /// <summary>
        /// Event for incoming words
        /// </summary>
        public Action<String, Exception> incomingWordEvent { get; set; }

        /// <summary>
        /// Username for the client
        /// </summary>
        public String clientName { get; private set; }

        /// <summary>
        /// Holds the address for the client
        /// </summary>
        public String address { get; private set; }

        /// <summary>
        /// Initializes an empty BoggleClientModel
        /// </summary>
        public BoggleClientModel()
        {
            clientSocket = null;
            clientName = "";
        }

        /// <summary>
        /// Connects the BoggleClient to BoggleServer
        /// </summary>
        /// <param name="address">Address to connect to DNS or IP</param>
        /// <param name="port">port to connect to</param>
        /// <param name="userName">userName for client connecting</param>
        /// <returns>True if connection succeeds, false otherwise</returns>
        public bool Connect(string address, int port, string userName)
        {
            try
            {
                TcpClient client = new TcpClient(address, port);
                
                //Starts a client, sets their username and address, and sends a play message
                clientSocket = new StringSocket(client.Client, Encoding.UTF8);
                clientSocket.BeginSend("PLAY " + userName + "\n", (e, o) => { }, null);
                this.clientName = userName;
                this.address = address;
                //Start receiving
                clientSocket.BeginReceive(CommandReceived, null);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the command WORD [wordhere] to the server
        /// </summary>
        /// <param name="word">Word to send to server</param>
        public void SendWord(string word)
        {
            clientSocket.BeginSend("WORD " + word + "\n", (e, o) => { }, null);
        }

        /// <summary>
        /// Handle command sent by server with the Event
        /// Possible exception if server crashes, make sure to check for this
        /// </summary>
        /// <param name="s">Command Received</param>
        /// <param name="e">Exception</param>
        /// <param name="payload">Which client sent the message</param>
        private void CommandReceived(string s, Exception e, object payload)
        {

            //Checks if there's an event registered
            if (incomingWordEvent != null)
            {
                incomingWordEvent(s, e);
            }

            clientSocket.BeginReceive(CommandReceived, null);
        }

        /// <summary>
        /// Closes the client connection
        /// </summary>
        public void CloseClient()
        {
            clientSocket.Close();
        }
    }
}
