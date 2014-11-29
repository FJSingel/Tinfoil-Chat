using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chatography.NetworkModule
{
    public class Client
    {
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
            isOnline = true;
            onlineClients = new List<TcpClient>();

            chatStream = new MemoryStream();
            cout = new StreamWriter(chatStream);

            connections = new List<Thread>();
            connection = new Thread(() => waitForConnection(isOnline));
            connection.Start();
        }

        /// <summary>
        /// Is the thread listening on the default or otherwise specified socket for connections
        /// </summary>
        private void waitForConnection(bool online)
        {
#pragma warning disable 618     //ignores the deprecated TcpListener constructor
            TcpListener serverSocket = new TcpListener(portnum);
#pragma warning restore 618

            serverSocket.Start();

            while (online)
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
            Thread cThread = new Thread(() => clientListener(isOnline, client, onlineClients.Count - 1));
            cout.WriteLine("Client-" + (onlineClients.Count - 1) + " online!");
            cThread.Start();
        }

        /// <summary>
        /// Listens to the network stream from specified socket and writes data to output stream
        /// </summary>
        /// <param name="clSocket">Socket of Client</param>
        /// <param name="clNo">Client number in online list, in order of added.</param>
        private void clientListener(bool online, TcpClient clSocket, int clNo)
        {
            NetworkStream networkStream;
            string dataFromClient = null;
            byte[] bytesFrom = new byte[clSocket.ReceiveBufferSize];

            while (online)
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
                catch (Exception ex){
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
            for (int i = 0; i < onlineClients.Count; i++ )
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
    }
}
