using ARCL;
using ARCLTypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
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

            if (Connection == null)
                Connection = new ARCLConnection();

            Connection.ConnectionString = txtConnectionString.Text;

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
            }
            else
            {
                Connection.Close();
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
            btnGetConfig.Background = Brushes.Yellow;

            if(Config == null)
                Config = new ConfigManager(Connection);

            Config.Start();
            Config.ReadConfigSection($"{txtConfigSection.Text}");

            Stopwatch sw = new Stopwatch();
            sw.Restart();

            while (!Config.IsSynced & sw.ElapsedMilliseconds < 60000) { Thread.Sleep(1); }
            
            if (Config.IsSynced)
            {
                Console.Out.Write(Config.SectionAsText(txtConfigSection.Text));
                btnGetConfig.Background = Brushes.Green;
            }
            else btnGetConfig.Background = Brushes.Red;

            Config.Stop();

            UpdateSectionList();
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            if (Config.Sections.ContainsKey(lstLoadedSections.SelectedItem.ToString()))
            {
                SaveFileDialog sf = new SaveFileDialog
                {
                    DefaultExt = "*.txt|.txt"
                };

                if ((bool)sf.ShowDialog())
                {
                    File.WriteAllText(sf.FileName, Config.SectionAsText(lstLoadedSections.SelectedItem.ToString()));
                }
            }

        }

        private void btnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            if (Config == null)
                Config = new ConfigManager(Connection);

            OpenFileDialog of = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = "*.txt|.txt"
            };

            if ((bool)of.ShowDialog())
            {
                Config.TextAsSection(File.ReadAllText(of.FileName));
            }
            UpdateSectionList();
        }

        private void btnWriteConfig_Click(object sender, RoutedEventArgs e)
        {
            Config.Start();
            Config.WriteConfigSection(lstLoadedSections.SelectedItem.ToString());
            Config.Stop();
        }

        private void UpdateSectionList()
        {
            lstLoadedSections.Items.Clear();
            foreach (var key in Config.Sections)
            {
                lstLoadedSections.Items.Add(key.Key);
            }
        }
    }
}
