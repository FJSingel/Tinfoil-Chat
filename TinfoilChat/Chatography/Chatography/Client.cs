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

        static bool isOnline;

        public Client()
        {
            isOnline = true;
        }

        public void initialize()
        {
            Thread comThread = new Thread(waitForCommand);
            comThread.Start();

            waitForConnection();
        }

        public void waitForCommand()
        {
            while (isOnline)
            {
                string commands = ">> Commands are:\n>>  /find\t-Find user at specified IP address and Port Number.\n" +
                                                    ">>  /exit\t-To go offline and exit.\n" + 
                                                    ">>  /help\tdisplay commands.";
                Console.WriteLine(commands);
                string input = Console.ReadLine();
                if (input[0] == '/')
                {
                    string command = input.Substring(1);
                    switch (command)
                    {
                        case "find":
                            findUser();
                            break;
                        case "exit":
                            isOnline = false;
                            break;
                        case "help":
                            Console.WriteLine(commands);
                            break;
                        default:
                            Console.WriteLine(">> Unrecognized Command. Type 'help' for a list of commands.");
                            break;
                    }
                }
                else if (input[0] == '@')
                {

                }
                else
                {
                    broadcast(input);
                }
            }

            Console.WriteLine("Exiting");
        }

        private void findUser()
        {
            string ip = "0.0.0.0";
            int port = 420;
            bool failed = true;
            while (failed)
            {
                try
                {
                    Console.WriteLine("Enter IP Address");
                    IPAddress IPADDRESS;
                    String line = Console.ReadLine();
                    if (IPAddress.TryParse(line, out IPADDRESS))
                        ip = line;
                    failed = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(">> " + ex.ToString());
                }
            }
            failed = true;
            while (failed)
            {
                try
                {
                    Console.WriteLine("Enter port number");
                    port = Int32.Parse(Console.ReadLine());
                    failed = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(">> " + ex.ToString());
                }
            }
            TcpClient nextClient = new TcpClient(ip, port);
            addClient(nextClient);
        }

        private void waitForConnection()
        {
            TcpListener serverSocket = new TcpListener(420);

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
            Console.WriteLine(" >> " + "Client-" + clientNo + " online!");
            onlineClients.Add(client);

            Thread cThread = new Thread(() => clientListener(client,clientNo));
            cThread.Start();
        }

        private void clientListener(TcpClient clSocket, string clNo)
        {
            string dataFromClient = null;
            byte[] bytesFrom = new byte[10025];
            NetworkStream networkStream;

            while ((true))
            {
                try
                {
                    networkStream = clSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, clSocket.ReceiveBufferSize);
                    dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    Console.WriteLine(" Client-" + clNo + ":" + dataFromClient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
        }

        private void broadcast(string msg)
        {
            byte[] bytesToEveryone = new byte[10025];
            NetworkStream networkStream;

            foreach (TcpClient client in onlineClients)
            {
                networkStream = client.GetStream();
                bytesToEveryone = System.Text.Encoding.ASCII.GetBytes(msg);
                int bytesOfmsg = System.Text.Encoding.ASCII.GetByteCount(msg);
                networkStream.Write(bytesToEveryone, 0, bytesOfmsg);
            }
        }

        public static void Main(string[] args)
        {
            Client client = new Client();
            client.initialize();
        }
    }
}
