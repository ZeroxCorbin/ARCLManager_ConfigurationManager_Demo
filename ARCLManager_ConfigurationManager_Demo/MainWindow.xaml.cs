using ARCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ARCLManager_ConfigurationManager_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ARCLConnection Connection;
        public ARCL.ConfigManager Config;

        public MainWindow()
        {
            InitializeComponent();

            btnConnect.Background = Brushes.Red;
            btnSend.IsEnabled = false;
        }
 

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            
            if(Connection == null)
                Connection = new ARCLConnection(txtConnectionString.Text);

            if (!Connection.IsConnected)
            {
                if (Connection.Connect())
                {
                    //Connection.DataReceived += Connection_DataReceived;
                    //Connection.StartReceiveAsync();

                    btnConnect.Background = Brushes.Green;
                    btnSend.IsEnabled = true;
                    return;
                }
                Connection = null;
            }
            else
            {
                Connection.Close();
                Connection = null;
            }
            btnConnect.Background = Brushes.Red;
            btnSend.IsEnabled = false;
        }

        private void Connection_DataReceived(object sender, string data) =>
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action<string>(ARCLDataReceivedViewUpdate), data);

        private void ARCLDataReceivedViewUpdate(string obj) => txtData.Text += obj;

        private void BtnSend_Click(object sender, RoutedEventArgs e) => Connection?.Write(txtSendMessage.Text);

        private void btnGetConfig_Click(object sender, RoutedEventArgs e)
        {
            Config = new ConfigManager(Connection);

            Config.Start();
            Config.GetConfigSection($"\"{txtConfigSection.Text}\"");
            while (!Config.IsSynced) { Thread.Sleep(1); }

            btnGetConfig.Background = Brushes.Green;
        }
    }
}
