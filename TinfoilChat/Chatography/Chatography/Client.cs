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
    class Client
    {
        List<TcpClient> onlineClients;
        NetworkStream stream;
        int portnum;
        OTRSessionManager _alice_otr_session_manager;
        static bool isOnline;

        public Client(int port)
        {
            onlineClients = new List<TcpClient>();
            isOnline = true;
            portnum = port;
        }

        public void initialize()
        {
            waitForConnection();
        }

        private void findUser(string ip, int port)
        {
            try
            {
                IPAddress.Parse(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> " + ex.ToString());
            }
            try
            {
                TcpClient nextClient = new TcpClient(ip, port);
                addClient(nextClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> " + ex.ToString());
            }
        }

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

        private void addClient(TcpClient client)
        {
            string clientNo = Convert.ToString(onlineClients.Count);
            onlineClients.Add(client);

            Thread cThread = new Thread(() => clientListener(client,clientNo));
            cThread.Start();
        }

        private void clientListener(TcpClient clSocket, string clNo)
        {
            string dataFromClient = null;
            byte[] bytesFrom = new byte[clSocket.ReceiveBufferSize];

            while ((true))
            {
                try
                {
                    NetworkStream networkStream = clSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clSocket.ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0,dataFromClient.IndexOf('$'));
                    Console.WriteLine(" Client-" + clNo + ":" + dataFromClient);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void broadcast(string msg)
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

        public static void Main(string[] args)
        {
            int port = Int32.Parse(Console.ReadLine());

            Client client = new Client(port);
            client.initialize();
        }

        #region OTRAdditions

        public void openOTRSession(String native_user_ID, String buddy_ID)
        {
            /* Declare OTR variables*/
            OTRSessionManager _alice_otr_session_manager = null;

            string _my_unique_id = native_user_ID; //Something like "Alice"
            string _my_buddy_unique_id = buddy_ID; //Something like "Bob"


            /* Create OTR session and Request OTR session */
            _alice_otr_session_manager = new OTRSessionManager(_my_unique_id);

            _alice_otr_session_manager.OnOTREvent += new OTREventHandler(OnAliceOTRMangerEventHandler);

            _alice_otr_session_manager.CreateOTRSession(_my_buddy_unique_id);

            _alice_otr_session_manager.RequestOTRSession(_my_buddy_unique_id, OTRSessionManager.GetSupportedOTRVersionList()[0]);


        }

        //TODO: This manager needs to be updated to use the network traffic
        private void OnAliceOTRManagerEventHandler(object source, OTREventArgs e)
        {

            switch (e.GetOTREvent())
            {
                case OTR_EVENT.MESSAGE:
                    //This event happens when a message is decrypted successfully
                    Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
                    break;
                case OTR_EVENT.SEND:
                    //This is where you would send the data on the network. Next line is just a dummy line. e.GetMessage() will contain message to be sent
                    SendDataOnNetwork(AliceID, e.GetMessage());
                    break;
                case OTR_EVENT.ERROR:
                    //Some sort of error occurred. We should use these errors to decide if it is fatal (failure to verify key) or benign (message did not decrypt)
                    Console.WriteLine("Alice: OTR Error: {0} \n", e.GetErrorMessage());
                    Console.WriteLine("Alice: OTR Error Verbose: {0} \n", e.GetErrorVerbose());
                    break;
                case OTR_EVENT.READY:
                    //Fires when each user is ready for communication. Can't communicate prior to this.
                    Console.WriteLine("Alice: Encrypted OTR session with {0} established \n", e.GetSessionID());
                    _alice_otr_session_manager.EncryptMessage(AlicesFriendID, "HI FIRST MESSAGE");
                    break;
                case OTR_EVENT.DEBUG:
                    //Just for debug lines. Flagged using a true flag in the session manager construction
                    Console.WriteLine("Alice: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.EXTRA_KEY_REQUEST:
                    //Allow for symmetric AES key usage. Only for OTR v3+.
                    //I doubt we need this.
                    break;
                case OTR_EVENT.SMP_MESSAGE:
                    //Fires after SMP process finishes
                    Console.WriteLine("Alice: " + e.GetMessage() + "\n");
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
