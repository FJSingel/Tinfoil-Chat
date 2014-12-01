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
        bool isOnline;
        int portnum = 420;

        TcpListener serverSocket;
        Dictionary<TcpClient, Queue<byte[]>> onlineClients;

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

#pragma warning disable 618     //ignores the deprecated TcpListener constructor
            serverSocket = new TcpListener(portnum);
#pragma warning restore 618

            onlineClients = new Dictionary<TcpClient, Queue<byte[]>>();
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
                addClient(nextClient);
            }

            waitingSocket.Stop();
        }

        public bool findUser(string ip)
        {
            return findUser(ip, portnum);
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
            Queue<byte[]> messageQueue = new Queue<byte[]>();
            onlineClients.Add(client,messageQueue);
            Thread cThread = new Thread(() => clientListener(isOnline, client, messageQueue));
            connections.Add(cThread);
            cThread.Start();
        }

        /// <summary>
        /// Listens to the network stream from specified socket and writes data to output stream
        /// </summary>
        /// <param name="clSocket">Socket of Client</param>
        /// <param name="clNo">Client number in online list, in order of added.</param>
        private void clientListener(bool online, TcpClient clSocket, Queue<byte[]> messageQueue)
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
                    messageQueue.Enqueue(msgBytes);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }

        private int indexOfSubArray(byte[] arrayToSearchIn, byte[] arrayToSearchFor)
        {
            int index = 0;
            for (int i = 0; i < (arrayToSearchIn.Length - arrayToSearchFor.Length) + 1; i++)
            {
                index = i;
                if (arrayToSearchIn[i] == arrayToSearchFor[0])
                {
                    bool completeMatch = true;
                    for (int j = 1; j < arrayToSearchFor.Length; j++)
                    {
                        if (arrayToSearchIn[i+j] != arrayToSearchFor[j])
                        {
                            completeMatch = false;
                            break;
                        }
                    }
                    if (completeMatch)
                    {
                        break;
                    }
                }
            }
            if (arrayToSearchIn[index] != arrayToSearchFor[0])
            {
                return -1;
            }
            return index;
        }

        /// <summary>
        /// Writes message to network stream of specified client's socket
        /// </summary>
        /// <param name="clNo">Client number in online list, in order of added.</param>
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
            foreach (TcpClient client in onlineClients.Keys)
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

            foreach (TcpClient client in onlineClients.Keys)
            {
                client.Close();
            }
        }

        public Dictionary<TcpClient, Queue<byte[]>> getOnlineClients()
        {
            return onlineClients;
        }
    }
}
