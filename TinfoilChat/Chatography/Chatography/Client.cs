using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chatography
{
    class Client
    {
        List<TcpClient> onlineClients;
        NetworkStream stream;
        int portnum;

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
    }
}
