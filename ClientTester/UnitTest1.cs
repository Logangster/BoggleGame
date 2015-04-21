using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoggleClient;
using BoggleServer;
using System.Threading;

namespace ClientTester
{
    [TestClass]
    public class UnitTest1
    {
        string word = "";
        ManualResetEvent mre1;

        [TestMethod]
        public void InvalidConnection()
        {
            BoggleClientModel model = new BoggleClientModel();
            Assert.IsFalse(model.Connect("asdfsdaf", 2000, "hey"));
        }

        [TestMethod]
        public void ValidConnection()
        {
            //Start a server and create a client with a valid address this time
            Server server = new Server(2000, new string[] { "200", "dictionary.txt" });
            BoggleClientModel model = new BoggleClientModel();
            Assert.IsTrue(model.Connect("localhost", 2000, "hey"));

            server.CloseServer();
        }

        [TestMethod]
        public void EventSending()
        {
            mre1 = new ManualResetEvent(false);

            Server server = new Server(2000, new string[] { "200", "dictionary.txt" });

            //Create the two clients
            BoggleClientModel model1 = new BoggleClientModel();
            Assert.IsTrue(model1.Connect("localhost", 2000, "hey"));

            //Test that this client connects with an IP address
            BoggleClientModel model2 = new BoggleClientModel();
            Assert.IsTrue(model2.Connect("127.0.0.1", 2000, "hey"));

            //Register the event
            model1.incomingWordEvent += TestWordEvent;

            //Start message should be sent to the event handler since both clients connected
            mre1.WaitOne(2000);
            Assert.AreEqual("START", word.Substring(0, 5));

            server.CloseServer();
        }

        [TestMethod]
        public void CloseSocket()
        {
            //Start a server and client
            Server server = new Server(2000, new string[] { "200", "dictionary.txt" });
            BoggleClientModel model = new BoggleClientModel();
            Assert.IsTrue(model.Connect("localhost", 2000, "hey"));

            model.CloseClient();

            try
            {
                model.SendWord("Test");
                Assert.Fail();
            }
            catch
            { }

            server.CloseServer();
        }

        // Event handler for Client's BeginReceive
        private void TestWordEvent(string line, Exception e)
        {
            word = line;
            mre1.Set();
        }

    }
}
