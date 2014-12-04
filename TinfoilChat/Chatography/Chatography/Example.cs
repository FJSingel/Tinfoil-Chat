using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Chatography.NetworkModule;

public class Example
{
    //Function to copy over for polling the output stream (separate from network stream)
    public static void readQueue(Queue<byte[]> messageQueue, bool end)
    {
        while (!end)
        {
            if (messageQueue.Count != 0)
            {
                byte[] msg = messageQueue.Dequeue();
                string message = Encoding.UTF8.GetString(msg);
                Console.WriteLine(message);
            }
        }
    }

    public static void Main(string[] args)
    {
        Client client1 = new Client();
        Client client2 = new Client(421);

        bool kill = false;

        byte[] msg = Encoding.UTF8.GetBytes("Hello");

        if (client1.findUser("127.0.0.1", 421))
        {
            Dictionary<TcpClient, Queue<byte[]>> thingy1 = client1.getOnlineClients();
            Dictionary<TcpClient, Queue<byte[]>> thingy2 = client2.getOnlineClients();

            TcpClient tcpclient2 = thingy1.Keys.First();
            TcpClient tcpclient1 = thingy2.Keys.First();

            Queue<byte[]> msgQueue1;
            thingy1.TryGetValue(tcpclient2, out msgQueue1);
            Queue<byte[]> msgQueue2;
            thingy2.TryGetValue(tcpclient1, out msgQueue2);

            Thread chatReader1 = new Thread(() => readQueue(msgQueue1, kill));
            Thread chatReader2 = new Thread(() => readQueue(msgQueue2, kill));

            Thread.Sleep(500);

            chatReader1.Start();
            chatReader2.Start();

            Thread.Sleep(500);
            client1.message(tcpclient2, msg);
            Thread.Sleep(500);
            client2.message(tcpclient1, msg);
        }

        Console.ReadKey();

        client1.close();
        client2.close();
        kill = true;
    }
}