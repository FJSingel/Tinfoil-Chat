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

namespace ChatUI
{
    /// <summary>
    /// Interaction logic for ChatSetup.xaml
    /// </summary>
    public partial class ChatSetup : Window
    {
        public ChatSetup()
        {
            InitializeComponent();
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            launchChat();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxEntryField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                launchChat();
            }
        }

        private void launchChat()
        {
            if( PortEntryField.Text.Length > 0 &&
                GuestIPEntryField.Text.Length > 0 &&
                GuestPortEntryField.Text.Length > 0)
            {
                ChatWindow chat = new ChatWindow(PortEntryField.Text, GuestIPEntryField.Text, GuestPortEntryField.Text);
                this.Close();
                chat.Show();
            }
            else
            {
                MessageBox.Show("Your chat is not setup yet. Plesae complete all fields", "Chat Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
