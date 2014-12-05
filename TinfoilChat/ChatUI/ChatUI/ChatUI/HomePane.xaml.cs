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
using ChatSession;

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for HomePane.xaml
    /// </summary>
    public partial class HomePane : Window
    {
        private Dictionary<int, ChatWindow> windows;
        private Session sess; 

        public HomePane()
        {
            InitializeComponent();

            // Initialize session data
            sess = new Session();
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            launchNewChat();
        }

        public void launchNewChat()
        {
            ChatWindow cWindow = new ChatWindow();
            int newChatId = cWindow.GetHashCode();
            windows.Add(newChatId, cWindow);
            cWindow.Show();
        }

        private void launchNewChat(Session.ChatMessage msg)
        {
            ChatWindow cWindow = new ChatWindow(Session.currentSession.chats[msg.getChatID()]);
            this.windows.Add(msg.getChatID(), cWindow);
            cWindow.Show();
        }

        public void displayChatMessage(Session.ChatMessage msg)
        {
            if (!this.windows.ContainsKey(msg.getChatID()))
            {
                launchNewChat(msg);
            }
            this.windows[msg.getChatID()].DisplayMessage(msg);
        }
    }
}
