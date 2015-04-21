using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using CustomNetworking;
using System.Text;
using BoggleServer;
using System.Threading;

namespace BoggleServerTester
{
    [TestClass]
    public class UnitTest1
    {
        string s1 = "";
        string s2 = "";
        string s3 = "";
        string s4 = "";
        string opponentClosedString = "";

        bool stop1 = false;
        bool stop2 = false;


        Object p1 = new Object();
        Object p2 = new Object();

        ManualResetEvent mre1;
        ManualResetEvent mre2;
        ManualResetEvent mre3;
        ManualResetEvent mre4;
        ManualResetEvent opponentClosedMre;
        int timeout = 2000;


        /// <summary>
        /// This method tests to make sure that basic word scores are sent to and 
        /// from the clients successfully.
        /// </summary>
        [TestMethod]
        public void TestWordScore()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "20", "Dictionary.txt", "qitscatscarsteps" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            mre3 = new ManualResetEvent(false);
            mre4 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, "Client1");

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, "Client2");

            client1.BeginReceive(ReceiveClient1, "Client1");

            client2.BeginReceive(ReceiveClient2, "Client2");

            client1.BeginSend("word quit\n", (e, o) => { }, "Client1");
            client1.BeginSend("word qit\n", (e, o) => { }, "Client1");
            client1.BeginSend("word cat\n", (e, o) => { }, "Client1");



            client1.BeginReceive(ReceiveScore1, "Client1");

            client2.BeginReceive(ReceiveScore2, "Client2");

            // Make sure client 1 gets start message
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("START", s1.Substring(0, 5));
            Assert.AreEqual("Client1", p1);

            // Make sure client 2 gets start message
            Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
            Assert.AreEqual("START", s2.Substring(0, 5));
            Assert.AreEqual("Client2", p2);

            // Make sure Client 1 gets the score message
            Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("SCORE 1 0", s3, "Score passed Incorrectly");

            // Makes sure Client 2 gets the score message
            Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
            Assert.AreEqual("SCORE 0 1", s4, "Score passed Incorrectly");

            // Closes the server
            server.CloseServer();

        }

        /// <summary>
        /// This test case checks all different lengths of word and verifies that the
        /// total is correct at the end.
        /// </summary>
       [TestMethod]
        public void TestAllWordScores()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "20", "Dictionary.txt", "abansnodmodejjsm" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            mre3 = new ManualResetEvent(false);
            mre4 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, "Client1");

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, "Client2");

            client1.BeginReceive(ReceiveClient1, "Client1");

            client2.BeginReceive(ReceiveClient2, "Client2");

            client1.BeginSend("words abandons\n", (e, o) => { }, "Client1");
            client1.BeginSend("word abandon\n", (e, o) => { }, "Client1");
            client1.BeginSend("word abandons\n", (e, o) => { }, "Client1");
            client1.BeginSend("word mode\n", (e, o) => { }, "Client1");
            client1.BeginSend("word modem\n", (e, o) => { }, "Client1");
            client1.BeginSend("word modems\n", (e, o) => { }, "Client1");
            client1.BeginSend("word mod\n", (e, o) => { }, "Client1");
            client1.BeginSend("word abandons\n", (e, o) => { }, "Client1");

            do
            {

                client1.BeginReceive(ReceiveScore1, "Client1");
            }
            while (!s3.Equals("SCORE 23 0"));



            // Make sure client 1 gets start message
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("START", s1.Substring(0, 5));
            Assert.AreEqual("Client1", p1);

            // Make sure client 2 gets start message
            Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
            Assert.AreEqual("START", s2.Substring(0, 5));
            Assert.AreEqual("Client2", p2);

            // Make sure Client 1 gets the score message
            Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("SCORE 23 0", s3, "Score passed Incorrectly");

            // Closes the server
            server.CloseServer();

        }


        /// <summary>
        /// This makes sure that the Time messages are being sent and received properly
        /// </summary>
        [TestMethod]
        public void TestTimeMessage()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "20", "Dictionary.txt", "potscatscarsteps" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            mre3 = new ManualResetEvent(false);
            mre4 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, "Client1");

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, "Client2");

            client1.BeginReceive(ReceiveClient1, "Client1");

            client2.BeginReceive(ReceiveClient2, "Client2");

            client1.BeginReceive(ReceiveTime1, "Client1");

            client2.BeginReceive(ReceiveTime2, "Client2");



            // Make sure client 1 gets start message
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("START", s1.Substring(0, 5));
            Assert.AreEqual("Client1", p1);

            // Make sure client 2 gets start message
            Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
            Assert.AreEqual("START", s2.Substring(0, 5));
            Assert.AreEqual("Client2", p2);

            // Make sure Client 1 gets the score message
            Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("TIME 19", s3, "Time passed Incorrectly");

            // Makes sure Client 2 gets the score message
            Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
            Assert.AreEqual("TIME 19", s4, "Time passed Incorrectly");

            // Closes the server
            server.CloseServer();

        }

        /// <summary>
        /// Tests when opponent closes their connection
        /// </summary>
        [TestMethod]
        public void OpponentClosedGame()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "200", "Dictionary.txt" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);

            //Separate MRE for the opponent closing test case
            opponentClosedMre = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, null);

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, null);

            client1.BeginReceive(ReceiveClient1, "Client1");

            client2.BeginReceive(ReceiveClient2, "Client2");

            //Close the client and make sure the proper TERMINATED message is sent back
            Thread.Sleep(1000);
            client2.BeginReceive(OpponentClosedCallback, null);
            client1.Close();

            //Now make sure the remaining client gets the terminated message sent back
            Assert.AreEqual(true, opponentClosedMre.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("TERMINATED", opponentClosedString);

            server.CloseServer();
        }

        private void OpponentClosedCallback(string s, Exception e, object payload)
        {
            opponentClosedString = s;
            opponentClosedMre.Set();
        }

        /// <summary>
        /// Tests when the player inserts an invalid play command
        /// </summary>
        [TestMethod]
        public void CreateNewGameWithInvalidPlay()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "200", "Dictionary.txt" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            //Wrong Play comment
            client1.BeginSend("PALY ENTEREDWRONG\n", (e, o) => { }, null);

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, null);

            //Now enter in the command correctly so that the game can still be started
            client1.BeginSend("PLAY ENTEREDCORRECTLY\n", (e, o) => { }, null);

            client1.BeginReceive(ReceiveClient1, "Client1");
            client2.BeginReceive(ReceiveClient2, "Client2");

            // Make sure client 1 gets ignore message due to bad command
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("IGNORING", s1.Substring(0, 8));
            Assert.AreEqual("Client1", p1);

            // Make sure client 2 gets start message signifying that client 1's resend worked this time
            Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
            Assert.AreEqual("START", s2.Substring(0, 5));
            Assert.AreEqual("Client2", p2);

            server.CloseServer();
        }

        /// <summary>
        /// Creates a game with only one player then adds the player later
        /// </summary>
        [TestMethod]
        public void CreateNewGameOnePlayer()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "200", "Dictionary.txt" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, null);

            client1.BeginReceive(ReceiveClient1, "Client1");

            // Make sure client 1 times out since no messages will be sent by server
            Assert.AreEqual(false, mre1.WaitOne(timeout), "Timed out waiting 1");

            //Now that second player has connected and added name, client 1 shouldn't time out
            StringSocket client2 = Client.CreateClient(2000);
            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, null);

            //Make sure game has started
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("START", s1.Substring(0, 5));
            Assert.AreEqual("Client1", p1);

            server.CloseServer();
        }


        /// <summary>
        /// This test makes sure that when both clients play the same word, neither receive
        /// credit for the word.
        /// </summary>
        [TestMethod]
        public void TestSameWord()
        {
            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "20", "Dictionary.txt", "potscatscarsteps" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            mre3 = new ManualResetEvent(false);
            mre4 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, "Client1");

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, "Client2");

            client1.BeginReceive(ReceiveClient1, "Client1");

            client2.BeginReceive(ReceiveClient2, "Client2");

            client1.BeginSend("word Pots\n", (e, o) => { }, "Client1");
            client2.BeginSend("word Pots\n", (e, o) => { }, "Client2");

            client1.BeginReceive(ReceiveScore1, "Client1");

            client2.BeginReceive(ReceiveScore2, "Client2");

           
            client1.BeginReceive(ReceiveScore1, "Client1");

            client2.BeginReceive(ReceiveScore2, "Client2");
            
            System.Threading.Thread.Sleep(1000);



            // Make sure client 1 gets start message
            Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
            Assert.AreEqual("START", s1.Substring(0, 5));
            Assert.AreEqual("Client1", p1);

            // Make sure client 2 gets start message
            Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
            Assert.AreEqual("START", s2.Substring(0, 5));
            Assert.AreEqual("Client2", p2);

            // Make sure Client 1 gets the score message
            Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("SCORE 0 0", s3, "Score passed Incorrectly");

            // Makes sure Client 2 gets the score message
            Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
            Assert.AreEqual("SCORE 0 0", s4, "Score passed Incorrectly");

            // Closes the server
            server.CloseServer();

        }

        /// <summary>
        /// This makes sure that the stop message is sent correctly when the time runs out.
        /// </summary>
        [TestMethod]
        public void TestStopMessage()
        {

            BoggleServer.Server server = new BoggleServer.Server(2000, new string[] { "2", "Dictionary.txt", "potscatscarsteps" });

            mre1 = new ManualResetEvent(false);
            mre2 = new ManualResetEvent(false);
            mre3 = new ManualResetEvent(false);
            mre4 = new ManualResetEvent(false);

            StringSocket client1 = Client.CreateClient(2000);

            client1.BeginSend("PLAY Testing1\n", (e, o) => { }, "Client1");

            StringSocket client2 = Client.CreateClient(2000);

            client2.BeginSend("PLAY Testing2\n", (e, o) => { }, "Client2");

            // Receives the start message
            client1.BeginReceive(ReceiveClient1, "Client1");

            // Receives the start message
            client2.BeginReceive(ReceiveClient2, "Client2");

            // Words are added to test all aspects of the stop message
            client1.BeginSend("word pots\n", (e, o) => { }, "Client1");
            client2.BeginSend("word pots\n", (e, o) => { }, "Client2");
            client1.BeginSend("word pot\n", (e, o) => { }, "Client1");
            client2.BeginSend("word cat\n", (e, o) => { }, "Client2");
            client1.BeginSend("word pasdfasdf\n", (e, o) => { }, "Client1");
            client2.BeginSend("word aldsffas\n", (e, o) => { }, "Client2");
            client2.BeginSend("word sto\n", (e, o) => { }, "Client2");
          
            // Waits until stop message for both clients have been received before asserting
            do
            {

                client1.BeginReceive(ReceiveStop1, "Client1");

                client2.BeginReceive(ReceiveStop2, "Client2");

            }
            while (stop1 == false || stop2 == false);

            // Make sure Client 1 gets the score message
            Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
            Assert.AreEqual("STOP", s3.Substring(0, 4));

            // Makes sure Client 2 gets the score message
            Assert.AreEqual("STOP", s4.Substring(0, 4));

            System.Threading.Thread.Sleep(1001);

            // Closes the server
            server.CloseServer();

        }


        //call back for client 1 start
        private void ReceiveClient1(String s, Exception o, object payload)
        {
            s1 = s;
            p1 = payload;

            mre1.Set();
        }

        // call back for client 2 start
        private void ReceiveClient2(String s, Exception o, object payload)
        {
            s2 = s;
            p2 = payload;
            mre2.Set();
        }
        // Callback for client 1 score
        private void ReceiveScore1(String s, Exception e, object payload)
        {

            if (s.Substring(0, 5) == "SCORE")
            {
                s3 = s;

            }

            mre3.Set();
        }

        // callback for client 2 score
        private void ReceiveScore2(String s, Exception e, object payload)
        {

            if (s.Substring(0, 5) == "SCORE")
            {
                s4 = s;

            }

            mre4.Set();
        }

        // callback for client 1 time
        private void ReceiveTime1(String s, Exception e, object payload)
        {

            if (s.Substring(0, 4) == "TIME")
            {
                s3 = s;

            }

            mre3.Set();
        }

        // callback for client 2 time
        private void ReceiveTime2(String s, Exception e, object payload)
        {

            if (s.Substring(0, 4) == "TIME")
            {
                s4 = s;

            }

            mre4.Set();
        }

        // callback for client stop 1
        private void ReceiveStop1(String s, Exception e, object payload)
        {

            if (s.Substring(0, 4) == "STOP")
            {
                s3 = s;

                mre3.Set();
                stop1 = true;

            }
        }

        // callback for client stop 2
        private void ReceiveStop2(String s, Exception e, object payload)
        {

            if (s.Substring(0, 4) == "STOP")
            {
                s4 = s;

                mre4.Set();

                stop2 = true;

            }           
        }
    }

    public class Client
    {
        private int timeout = 2000;
        public static StringSocket CreateClient(int port)
        {       
            TcpClient client = new TcpClient("localhost", port);
            Socket clientSocket = client.Client;
            // Wrap the two ends of the connection into StringSockets
            StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());
            return receiveSocket;
        }
    }
}
