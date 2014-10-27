using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatography
{

    class Contact
    {
        List<string> ip;
        int port;
        TcpClient connection;

        public List<string> lastKnownIPAddresses()
        {
            return ip;
        }

        public int getPort()
        {
            return port;
        }

        public void setConnection(TcpClient client)
        {
            connection = client;
        }
    }

    class Client
    {
        List<Contact> contacts;
        List<Contact> online = new List<Contact>();
        NetworkStream serverStream;

        public Client()
        {

        }

        public void initialize()
        {
            TcpClient client = new TcpClient();
            foreach (Contact contact in contacts)
            {
                if (connectTo(client, contact))
                {
                    online.Add(contact);
                }
            }

            TcpListener serverSocket = new TcpListener(420);
            while(true){
                online.Add(serverSocket.AcceptTcpClient());
            }
        }

        private Boolean connectTo(TcpClient client, Contact contact)
        {
            foreach (string ip in contact.lastKnownIPAddresses())
            {
                try
                {
                    client.Connect(ip, contact.getPort());
                    contact.setConnection(client);
                    return true;
                }
                catch (SocketException e)
                {
                    return false;
                }
            }
            return false;
        }

        static void Main(string[] args)
        {

        }
    }
}
