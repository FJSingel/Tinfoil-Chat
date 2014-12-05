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

        public Session.Chat chat {get; set;}

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

        public void DisplayMessage(string message)
        {
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

        public void DisplayMessage(Session.ChatMessage msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                try
                {
                    string strMsg = System.Text.Encoding.Default.GetString(msg.getChatMessage());
                    textBoxChatPane.AppendText(strMsg);
                }
                catch (Exception ex)
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

    public class MessageListener
    {
        private Session session;
        private ChatWindow window;

        public MessageListener(Session sess, ChatWindow cWin)
        {
            window = cWin;
            session = sess;
            session.messageSent += new MessageSentHandler(MessageReceived);
        }

        private void MessageReceived(object sender, EventArgs e)
        {
            ChatSession.Session.ChatMessage msg = (ChatSession.Session.ChatMessage) e;
            if(window.chat.chatID == msg.getChatID()){
                window.DisplayMessage(msg);
            }
        }
    }
}
