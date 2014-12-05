using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using OTR.Interface;

namespace Chatography
{
    public class Client
    {
        static OTRSessionManager AliceSessionManager;
        static String AlicesFriendID;
        static String AlicesID;
        static bool isOnline;
        int portnum = 420;
        Thread connection;
        List<TcpClient> onlineClients;
        List<Thread> connections;
        MemoryStream chatStream;
        TextWriter cout;

        public Client()
        {
            initialize();
        }

        public Client(int port)
        {
            portnum = port;
            initialize();
        }

        private void initialize()
        {
            AlicesID = "Host";
            AlicesFriendID = "Guest";
            isOnline = true;
            onlineClients = new List<TcpClient>();
            AliceSessionManager = new OTRSessionManager(AlicesID);


            chatStream = new MemoryStream();
            cout = new StreamWriter(chatStream);

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
        /// <returns>True if user is found otherwise false.</returns>
        public bool findUser(string ip, int port)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.Message);
                System.Diagnostics.Debug.Write("Can't parse. Get wrecked.");
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
        {/////////////////////////////////////
            openOTRSession(AlicesFriendID);
            onlineClients.Add(client);
            Thread cThread = new Thread(() => clientListener(client, onlineClients.Count - 1));
            cout.WriteLine("Client-" + (onlineClients.Count - 1) + " online!");
            cThread.Start();
        }

        /// <summary>
        /// Listens to the network stream from specified socket and writes data to output stream
        /// </summary>
        /// <param name="clSocket">Socket of Client</param>
        /// <param name="clNo">Client number in online list, in order of added.</param>
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
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf('$'));
                    AliceSessionManager.ProcessOTRMessage(AlicesFriendID, dataFromClient);
                    //cout.WriteLine("Client-" + clNo + ":" + dataFromClient);
                    cout.WriteLine("Recieved:" + dataFromClient);
                    cout.Flush();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Writes message to network stream of specified client's socket
        /// </summary>
        /// <param name="clNo">Client number in online list, in order of added.</param>
        /// <param name="msg">Message to be sent</param>
        public void message(int clNo, string msg)
        {
            Thread mThread = new Thread(() => messageThread(clNo, msg));
            mThread.Start();
        }

        private void messageThread(int clNo, string msg)
        {
            byte[] bytesToSend = new byte[1024];
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
            for (int i = 0; i < onlineClients.Count; i++)
            {
                Thread mThread = new Thread(() => messageThread(i, msg));
                mThread.Start();
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

        public MemoryStream getChatStream()
        {
            return chatStream;
        }


        //Function to copy over for polling the output stream (separate from network stream)
        public static void readStream(MemoryStream output)
        {
            StreamReader outputReader = new StreamReader(output);
            int position = 0;
            while (true)
            {
                output.Position = position;
                string OUT = outputReader.ReadToEnd();
                Console.Write(OUT); //replace this line with function to output to UI
                if (OUT.Length != 0)
                    position += OUT.Length;
            }
        }

        public static void Main(string[] args)
        {
            Client client1 = new Client();
            Client client2 = new Client(421);
            AliceSessionManager = new OTRSessionManager(AlicesID);

            Thread chatReader1 = new Thread(() => readStream(client1.getChatStream())); // Start new thread reading the MemoryStream chat1
            Thread chatReader2 = new Thread(() => readStream(client2.getChatStream())); // Start new thread reading the MemoryStream chat2

            chatReader1.Start();
            chatReader2.Start();

            client1.findUser("127.0.0.1", 421);
            
            client1.message(0, "dang");
            Thread.Sleep(500);
            client2.message(0, "Foo");
            Thread.Sleep(500);
            client1.message(0, "Hello");


            Console.ReadKey();
        }

        #region OTRAdditions

        public void openOTRSession(String buddy_ID)
        {
            string _my_buddy_unique_id = buddy_ID; //Something like "Bob"


            /* Create OTR session and Request OTR session */
            //AliceSessionManager = new OTRSessionManager(_my_unique_id);

            AliceSessionManager.OnOTREvent += new OTREventHandler(OnAliceOTRManagerEventHandler);

            AliceSessionManager.CreateOTRSession(_my_buddy_unique_id, true);

            AliceSessionManager.RequestOTRSession(_my_buddy_unique_id, OTRSessionManager.GetSupportedOTRVersionList()[0]);


        }

        //TODO: This manager needs to be updated to use the network traffic
        private void OnAliceOTRManagerEventHandler(object source, OTREventArgs e)
        {

            switch (e.GetOTREvent())
            {
                case OTR_EVENT.MESSAGE:
                    //This event happens when a message is decrypted successfully
                    //Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
                    cout.WriteLine("Client-" + e.GetSessionID() + ":" + e.GetMessage());
                    cout.Flush();
                    break;
                case OTR_EVENT.SEND:
                    //This is where you would send the data on the network. Next line is just a dummy line. e.GetMessage() will contain message to be sent
                    message(0, e.GetMessage());
                    break;
                case OTR_EVENT.ERROR:
                    //Some sort of error occurred. We should use these errors to decide if it is fatal (failure to verify key) or benign (message did not decrypt)
                    Console.WriteLine("Alice: OTR Error: {0} \n", e.GetErrorMessage());
                    Console.WriteLine("Alice: OTR Error Verbose: {0} \n", e.GetErrorVerbose());
                    break;
                case OTR_EVENT.READY:
                    //Fires when each user is ready for communication. Can't communicate prior to this.
                    cout.WriteLine("Alice: Encrypted OTR session with {0} established \n", e.GetSessionID());
                    cout.Flush();
                    AliceSessionManager.EncryptMessage(AlicesFriendID, "HI FIRST MESSAGE");
                    break;
                case OTR_EVENT.DEBUG:
                    //Just for debug lines. Flagged using a true flag in the session manager construction
                    cout.WriteLine("DEBUG: " + e.GetMessage() + "\n");
                    cout.Flush();
                    break;
                case OTR_EVENT.EXTRA_KEY_REQUEST:
                    //Allow for symmetric AES key usage. Only for OTR v3+.
                    //I doubt we need this.
                    break;
                case OTR_EVENT.SMP_MESSAGE:
                    //Fires after SMP process finishes
                    cout.WriteLine("Authentication Notice: " + e.GetMessage() + "\n");
                    cout.Flush();
                    break;
                case OTR_EVENT.CLOSED:
                    //Fires when OTR session closes
                    Console.WriteLine("Alice: Encrypted OTR session with {0} closed \n", e.GetSessionID());
                    break;
            }
        }
        #endregion
    }
}
