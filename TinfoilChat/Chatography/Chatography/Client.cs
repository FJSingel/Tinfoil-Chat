﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

//using OTR.Interface;

namespace Chatography
{
    class Client
    {
        //OTRSessionManager _alice_otr_session_manager;
        static bool isOnline;
        int portnum = 420;
        Thread connection;
        List<TcpClient> onlineClients;
        List<Thread> connections;
        TextWriter cout;

        public Client(TextWriter output)
        {
            initialize(output);
        }

        public Client(TextWriter output, int port)
        {
            portnum = port;
            initialize(output);
        }

        private void initialize(TextWriter output)
        {
            isOnline = true;
            onlineClients = new List<TcpClient>();
            cout = output;

            connections = new List<Thread>();
            connection = new Thread(() => waitForConnection());
            connection.Start();
        }

        /// <summary>
        /// Is the thread listening on the default or otherwise specified socket for connections
        /// </summary>
        private void waitForConnection()
        {
            TcpListener serverSocket = new TcpListener(portnum);

            serverSocket.Start();

            while (isOnline)
            {
                TcpClient nextClient = serverSocket.AcceptTcpClient();
                addClient(nextClient);
            }
        }

        /// <summary>
        /// Attempts to connect to a user at specified ip address and port number
        /// </summary>
        /// <param name="ip">The IP address of the user</param>
        /// <param name="port">The port number the user's client is using</param>
        public bool findUser(string ip, int port)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch (Exception ex)
            {
                return false;
            }
            try
            {
                TcpClient nextClient = new TcpClient(ip, port);
                addClient(nextClient);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void addClient(TcpClient client)
        {
            onlineClients.Add(client);
            Thread cThread = new Thread(() => clientListener(client, onlineClients.Count - 1));
            cout.WriteLine("Client-" + (onlineClients.Count - 1) + " online!");
            cThread.Start();
        }

        /// <summary>
        /// Listens to the network stream from specified socket and writes data to output stream
        /// </summary>
        /// <param name="clSocket">Socket of Client</param>
        /// <param name="clNo">Client number in online list</param>
        private void clientListener(TcpClient clSocket, int clNo)
        {
            NetworkStream networkStream;
            string dataFromClient = null;
            byte[] bytesFrom = new byte[clSocket.ReceiveBufferSize];

            while (isOnline)
            {
                try
                {
                    networkStream = clSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0,dataFromClient.IndexOf('$'));
                    cout.WriteLine("Client-" + clNo + ":" + dataFromClient);
                    cout.Flush();
                }
                catch (Exception ex){}
            }
        }

        /// <summary>
        /// Writes message to network stream of specified client's socket
        /// </summary>
        /// <param name="clNo">Client number in online list</param>
        /// <param name="msg">Message to be sent</param>
        private void message(int clNo, string msg)
        {
            byte[] bytesToSend = new byte[10025];
            bytesToSend = Encoding.ASCII.GetBytes(msg + '$');
            int bytesOfmsg = Encoding.ASCII.GetByteCount(msg + '$');

            TcpClient client = onlineClients.ElementAt(clNo);
            NetworkStream networkStream = client.GetStream();
            networkStream.Write(bytesToSend, 0, bytesOfmsg);
            networkStream.Flush();
        }

        /// <summary>
        /// Writes message to every connected clients' socket's network stream
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void broadcast(string msg)
        {
            byte[] bytesToSend = new byte[10025];
            bytesToSend = Encoding.ASCII.GetBytes(msg + '$');
            int bytesOfmsg = Encoding.ASCII.GetByteCount(msg + '$');
            
            foreach (TcpClient client in onlineClients)
            {
                NetworkStream networkStream = client.GetStream();
                networkStream.Write(bytesToSend, 0, bytesOfmsg);
                networkStream.Flush();
            }
        }

        public void close()
        {
            isOnline = false;
            connection.Abort();
            foreach (Thread thread in connections)
            {
                thread.Abort();
            }
        }

        public static void readStream(MemoryStream output)
        {
            StreamReader outputReader = new StreamReader(output);
            int position = 0;
            while (true)
            {
                output.Position = position;
                string OUT = outputReader.ReadToEnd();
                Console.Write(OUT);
                position += OUT.Length;
            }
        }

        public static void Main(string[] args)
        {
            MemoryStream chat1 = new MemoryStream();
            StreamWriter cout1 = new StreamWriter(chat1);
            Thread chatReader1 = new Thread(() => readStream(chat1)); // Start new thread reading the MemoryStream chat1
            chatReader1.Start();

            MemoryStream chat2 = new MemoryStream();
            StreamWriter cout2 = new StreamWriter(chat2);
            Thread chatReader2 = new Thread(() => readStream(chat2)); // Start new thread reading the MemoryStream chat2
            chatReader2.Start();

            Client client1 = new Client(cout1);
            Client client2 = new Client(cout2, 421);

            client1.findUser("127.0.0.1", 421);

            client1.message(0, "Hello");
            client2.message(0, "Foo");

            Console.ReadKey();
            client1.close();
            client2.close();
        }

        //#region OTRAdditions

        //public void openOTRSession(String native_user_ID, String buddy_ID)
        //{
        //    /* Declare OTR variables*/
        //    OTRSessionManager _alice_otr_session_manager = null;

        //    string _my_unique_id = native_user_ID; //Something like "Alice"
        //    string _my_buddy_unique_id = buddy_ID; //Something like "Bob"


        //    /* Create OTR session and Request OTR session */
        //    _alice_otr_session_manager = new OTRSessionManager(_my_unique_id);

        //    _alice_otr_session_manager.OnOTREvent += new OTREventHandler(OnAliceOTRMangerEventHandler);

        //    _alice_otr_session_manager.CreateOTRSession(_my_buddy_unique_id);

        //    _alice_otr_session_manager.RequestOTRSession(_my_buddy_unique_id, OTRSessionManager.GetSupportedOTRVersionList()[0]);


        //}

        ////TODO: This manager needs to be updated to use the network traffic
        //private void OnAliceOTRManagerEventHandler(object source, OTREventArgs e)
        //{

        //    switch (e.GetOTREvent())
        //    {
        //        case OTR_EVENT.MESSAGE:
        //            //This event happens when a message is decrypted successfully
        //            Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
        //            break;
        //        case OTR_EVENT.SEND:
        //            //This is where you would send the data on the network. Next line is just a dummy line. e.GetMessage() will contain message to be sent
        //            SendDataOnNetwork(AliceID, e.GetMessage());
        //            break;
        //        case OTR_EVENT.ERROR:
        //            //Some sort of error occurred. We should use these errors to decide if it is fatal (failure to verify key) or benign (message did not decrypt)
        //            Console.WriteLine("Alice: OTR Error: {0} \n", e.GetErrorMessage());
        //            Console.WriteLine("Alice: OTR Error Verbose: {0} \n", e.GetErrorVerbose());
        //            break;
        //        case OTR_EVENT.READY:
        //            //Fires when each user is ready for communication. Can't communicate prior to this.
        //            Console.WriteLine("Alice: Encrypted OTR session with {0} established \n", e.GetSessionID());
        //            _alice_otr_session_manager.EncryptMessage(AlicesFriendID, "HI FIRST MESSAGE");
        //            break;
        //        case OTR_EVENT.DEBUG:
        //            //Just for debug lines. Flagged using a true flag in the session manager construction
        //            Console.WriteLine("Alice: " + e.GetMessage() + "\n");
        //            break;
        //        case OTR_EVENT.EXTRA_KEY_REQUEST:
        //            //Allow for symmetric AES key usage. Only for OTR v3+.
        //            //I doubt we need this.
        //            break;
        //        case OTR_EVENT.SMP_MESSAGE:
        //            //Fires after SMP process finishes
        //            Console.WriteLine("Alice: " + e.GetMessage() + "\n");
        //            break;
        //        case OTR_EVENT.CLOSED:
        //            //Fires when OTR session closes
        //            Console.WriteLine("Alice: Encrypted OTR session with {0} closed \n", e.GetSessionID());
        //            break;
        //    }
        //}
        //#endregion
    }
}
