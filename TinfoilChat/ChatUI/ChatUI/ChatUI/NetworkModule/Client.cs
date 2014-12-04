using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using ChatUI;

namespace ChatUI.NetworkModule
{
    public class Client
    {
        bool isOnline;
        int portnum = 420;

        TcpListener serverSocket;
        HashSet<TcpClient> onlineClients;
        HashSet<IPAddress> ignoreList;

        Thread portListener;
        List<Thread> connections;

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

#pragma warning disable 618     //ignores the deprecated TcpListener constructor warning
            serverSocket = new TcpListener(portnum);
#pragma warning restore 618

            onlineClients = new HashSet<TcpClient>();
            connections = new List<Thread>();

            portListener = new Thread(() => waitForConnection(isOnline, serverSocket));
            portListener.Start();
        }

        /// <summary>
        /// Is the thread listening on the default or otherwise specified socket for connections
        /// </summary>
        private void waitForConnection(bool online, TcpListener waitingSocket)
        {

            waitingSocket.Start();

            while (online)
            {
                TcpClient nextClient = waitingSocket.AcceptTcpClient();
                IPAddress nextClientIP = ((IPEndPoint)nextClient.Client.RemoteEndPoint).Address;
                if (ignoreList.Contains(nextClientIP))
                {
                    nextClient.Close();
                    continue;
                }
                addClient(nextClient);
                Session.currentSession.newUser(nextClient);
            }

            waitingSocket.Stop();
        }

        public TcpClient findUser(string ip)
        {
            return findUser(ip, portnum);
        }

        /// <summary>
        /// Attempts to connect to a user at specified ip address and port number
        /// </summary>
        /// <param name="ip">The IP address of the user</param>
        /// <param name="port">The port number the user's client is using</param>
        /// <returns>TcpClient of User at IP address/port specified if the connection was successful, otherwise null.</returns>
        public TcpClient findUser(string ip, int port)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(ip, port);
                addClient(client);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            return client;
        }

        private void addClient(TcpClient client)
        {
            onlineClients.Add(client);
            Thread cThread = new Thread(() => clientListener(isOnline, client));
            connections.Add(cThread);
            cThread.Start();
        }

        /// <summary>
        /// Listens to the network stream from specified socket and writes data to output stream
        /// </summary>
        /// <param name="online">The isOnline boolean needed to shutdown threads</param>
        /// <param name="clSocket">Socket of Client</param>
        /// <param name="messageQueue">Client number in online list, in order of added.</param>
        private void clientListener(bool online, TcpClient clSocket)
        {
            NetworkStream networkStream;
            byte[] bytesFrom = new byte[clSocket.ReceiveBufferSize];

            while (online && clSocket.Connected)
            {
                try
                {
                    networkStream = clSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clSocket.ReceiveBufferSize);
                    int msgSize = 1;
                    foreach (byte byt in bytesFrom)
                    {
                        if (byt == 0)
                        {
                            break;
                        }
                        msgSize++;
                    }
                    byte[] msgBytes = new byte[msgSize];
                    Array.Copy(bytesFrom, msgBytes, msgSize);
                    Session.currentSession.newMessage(msgBytes);
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
        /// <param name="client">TcpClient in online list.</param>
        /// <param name="msg">Message to be sent</param>
        public void message(TcpClient client, byte[] msg)
        {
            Thread mThread = new Thread(() => messageThread(client, msg));
            mThread.Start();
        }

        /// <summary>
        /// Writes message to every connected clients' socket's network stream
        /// </summary>
        /// <param name="msg">Message to be sent</param>
        public void broadcast(byte[] msg)
        {
            int bytesOfmsg = msg.Length + 1;
            byte[] bytesToSend = new byte[bytesOfmsg];

            msg.CopyTo(bytesToSend, 0);
            Byte terminate = 0;
            bytesToSend.SetValue(terminate, msg.Length);

            foreach (TcpClient client in onlineClients)
            {
                Thread mThread = new Thread(() => messageThread(client, msg));
                mThread.Start();
            }
        }

        private void messageThread(TcpClient client, byte[] msg)
        {
            int bytesOfmsg = msg.Length + 1;
            byte[] bytesToSend = new byte[bytesOfmsg];

            msg.CopyTo(bytesToSend, 0);
            Byte terminate = 0;
            bytesToSend.SetValue(terminate, msg.Length);

            NetworkStream networkStream = client.GetStream();
            networkStream.Write(bytesToSend, 0, bytesOfmsg);
            networkStream.Flush();
        }

        public void close()
        {
            isOnline = false;

            Thread.Sleep(500);

            portListener.Abort();
            serverSocket.Stop();

            foreach (Thread thread in connections)
            {
                thread.Abort();
            }

            foreach (TcpClient client in onlineClients)
            {
                client.Close();
            }
        }

        public void ignore(TcpClient client)
        {
            ignoreList.Add(((IPEndPoint)client.Client.RemoteEndPoint).Address);
            onlineClients.Remove(client);
            client.Close();
        }
    }
}
