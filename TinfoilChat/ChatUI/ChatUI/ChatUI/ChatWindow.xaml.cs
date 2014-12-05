using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Chatography;
using System.Windows.Threading;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        //private ChatUIBackend.ChatBackend _backend;

        private Chatography.Client _client;
        private int port;
        private string guestIP;
        private int guestPort;

        public ChatWindow() : this("420", "0.0.0.0", "421"){}

        public ChatWindow(string port, string guestIP, string guestPort)
        {
            // TODO: Complete member initialization
            this.port = int.Parse(port);
            this.guestIP = guestIP;
            this.guestPort = int.Parse(guestPort);

            InitializeComponent();
            //_backend = new ChatUIBackend.ChatBackend(this.DisplayMessage);
            _client = new Chatography.Client(this.port);

            // Start a new thread to listen for interaction
            Thread chatReader = new Thread(() => clientThread(_client.getChatStream()));
            chatReader.Start();

            // Wait to connect with the seeking user
            //_client.findUser(this.guestIP, this.guestPort);
            Console.Write(_client.findUser(this.guestIP, this.guestPort));
        }

        public void clientThread(MemoryStream readStream){
            StreamReader messageReader = new StreamReader(readStream);
            int position = 0;
            while (true)
            {
                try
                {
                    readStream.Position = position;
                    string message = messageReader.ReadToEnd();
                    if (message.Length != 0)
                    {
                        position += message.Length; 
                        this.DisplayMessage(message);
                        Console.Write(message);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.Message);
                }
            }
        }

        public void DisplayMessage(string message/*ChatUIBackend.CompositeType composite*/)
        {
            /*
            string username = composite.Username == null ? "" : composite.Username;
            string message = composite.Message == null ? "" : composite.Message;
            textBoxChatPane.Text += (username + ": " + message + Environment.NewLine);
             */
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    textBoxChatPane.Text += (message + Environment.NewLine);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.Message);
                }
            }));
        }

        private void textBoxEntryField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                //_backend.SendMessage(textBoxEntryField.Text);
                this.DisplayMessage(textBoxEntryField.Text);
                _client.encryptFromGUI(textBoxEntryField.Text);
                //_client.message(0, textBoxEntryField.Text);
                textBoxEntryField.Clear();
            }
        }
    }
}
