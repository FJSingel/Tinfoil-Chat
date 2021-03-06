﻿using System;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainPane : Window
    {
        public MainPane()
        {
            InitializeComponent();
            InitializeClient();
        }

        public void launch_chat(object sender, RoutedEventArgs args)
        {
            ChatWindow chat = new ChatWindow();
            chat.Show();
        }

        public void launch_login(object sender, RoutedEventArgs args)
        {
            LoginWindow log = new LoginWindow();
            log.Show();
        }

        public void launch_about(object sender, RoutedEventArgs args)
        {
            About about = new About();
            about.Show();
        }

        private void chat_setup(object sender, RoutedEventArgs e)
        {
            ChatSetup chat_setup = new ChatSetup();
            chat_setup.Show();
        }

        private void InitializeClient(){

        }
    }
}
