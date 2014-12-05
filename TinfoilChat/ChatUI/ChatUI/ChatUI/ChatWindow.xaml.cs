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
using System.Windows.Threading;
using ChatSession;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        //private ChatUIBackend.ChatBackend _backend;

        private Session.Chat chat;

        public ChatWindow()
        {
            InitializeComponent();
            chat = new Session.Chat();
        }

        public ChatWindow(Session.Chat c){
            InitializeComponent();
            chat = c;
        }

        public void clientThread(MemoryStream readStream){
            StreamReader messageReader = new StreamReader(readStream);
            int position = 0;
            while (true)
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
                    textBoxChatPane.AppendText(message);
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
                try
                {
                    this.DisplayMessage("You: " + textBoxEntryField.Text + Environment.NewLine);
                    this.chat.message(textBoxEntryField.Text);
                    textBoxEntryField.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to send message: " + ex.Message, "Unable to Send Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
            }
        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {
            // Chat to make a new user a friend
        }

        private void InviteUser_Click(object sender, RoutedEventArgs e)
        {
            // Invite an old friend to the chat
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
