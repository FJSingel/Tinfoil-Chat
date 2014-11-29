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
        Client client3 = new Client(423);

        Thread chatReader1 = new Thread(() => readStream(client1.getChatStream())); // Start new thread reading the MemoryStream chat1
        Thread chatReader2 = new Thread(() => readStream(client2.getChatStream())); // Start new thread reading the MemoryStream chat2

        chatReader1.Start();
        chatReader2.Start();

        if (client1.findUser("127.0.0.1", 421))
        {
            Thread.Sleep(500);
            client1.message(0, "dang");
            Thread.Sleep(500);
            client2.message(0, "Foo");
            Thread.Sleep(500);
            client1.message(0, "Hello");
            Thread.Sleep(500);
            client3.findUser("127.0.0.1", 420);
            Thread.Sleep(500);
            client3.message(0, "Hi");
        }

        Console.ReadKey();

        client1.close();
        client2.close();
    }
}