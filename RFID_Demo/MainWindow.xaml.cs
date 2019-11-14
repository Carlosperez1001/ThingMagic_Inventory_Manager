using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ThingMagic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using DataGrid = System.Windows.Controls.DataGrid;
using System.Collections.ObjectModel;

namespace RFID_Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        public string EPC { get; private set; }
        public string timeStamp { get; private set; }
        public string RSSI { get; private set; }

        public int COM_Port = 0;
        public bool toggelReader = false;
        public bool connectiveStatus = false;
        public ThingMagic.Reader reader;
        ObservableCollection<UnknownRFID> UnknownRFIDList = new ObservableCollection<UnknownRFID>();
        

        public MainWindow()
        {


            InitializeComponent();
            DateTime TodayDate = DateTime.Today;

            UnregisteredDataGrid.ItemsSource = UnknownRFIDList;

            UnknownRFIDList.Add(new UnknownRFID(EPC = "0123456789012345", timeStamp = TodayDate.ToString(), RSSI = "-24"));

            updateConnectiveStatus();


        }

        public void updateConnectiveStatus() {
            if (connectiveStatus == true) {
                Icon_ConnectiveStatus.Fill = new SolidColorBrush(Colors.Yellow);
                cbox_COM.IsEnabled = false;
                btn_Connect.IsEnabled = false;
                lbl_ConnectiveStatus.Content = "Idle";
                if (toggelReader == true)
                {
                    Icon_ConnectiveStatus.Fill = new SolidColorBrush(Colors.Green);
                    lbl_ConnectiveStatus.Content = "Scanning";
                }
            }
           
            else
            {
                Icon_ConnectiveStatus.Fill = new SolidColorBrush(Colors.Red);
                lbl_ConnectiveStatus.Content = "Disconnected";
            }
        }

        public void ConnectRFID()
        {
            {
                //Configurations of COMs "USB Port"

                //Attempt Connection
                try
                {
                    Console.WriteLine(COM_Port);
                    string uri = "eapi:///com" + COM_Port;
                    reader = ThingMagic.Reader.Create(uri);

                    reader.Connect();
                    reader.ParamSet("/reader/region/id", ThingMagic.Reader.Region.NA);
                    //Select Antenna 1 port manually 
                    int[] antennaList = null;
                    string str = "1,1";
                    antennaList = Array.ConvertAll(str.Split(','), int.Parse);
                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);
                    reader.ParamSet("/reader/read/plan", plan);
                    connectiveStatus = true;
                    updateConnectiveStatus();


                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    connectiveStatus = false;
                    //  updateConnectiveStatus();
                    MessageBox.Show("COM " + COM_Port + " is not valid. ");


                }
            }
        }


        public void ReadBatchRFID()
        {
            if (connectiveStatus == true && toggelReader == true)
            {
                try
                {

                    reader.StartReading();
                    reader.TagRead += OnTagRead;

                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);

                }
            }
            else
            {
                MessageBox.Show("M6e is not connected properly. Please select the correct COM port that connects the M6e module");
            }
        }

        private void OnTagRead(Object sender, TagReadDataEventArgs e) {
            if (toggelReader == true) {
                return;
            }
            bool existingTag = false;
            Console.WriteLine(e.TagReadData.EpcString + " " + e.TagReadData.Time.ToString() + " " + e.TagReadData.Rssi.ToString());
            
            if (UnknownRFIDList.Any(p => p.EPC == e.TagReadData.EpcString)) {
                var list = UnknownRFIDList.First(f => f.EPC == e.TagReadData.EpcString) ;
                var index = UnknownRFIDList.IndexOf(list);
                UnknownRFIDList[index].timeStamp = e.TagReadData.Time.ToString();
                UnknownRFIDList[index].RSSI = e.TagReadData.Rssi.ToString();
                Console.WriteLine("I have " + e.TagReadData.EpcString);
                this.Dispatcher.Invoke(() =>
                {
                    UnregisteredDataGrid.Items.Refresh();
                });
                }
            else {
                this.Dispatcher.Invoke(() =>
                {
                    UnknownRFIDList.Add(new UnknownRFID(EPC = e.TagReadData.EpcString, timeStamp = e.TagReadData.Time.ToString(), RSSI = e.TagReadData.Rssi.ToString()));
                });
            }
        }
    
        

        private void btn_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (toggelReader == false)
            {
                toggelReader = true;
                ReadBatchRFID();
            }
            else {
                toggelReader = false; 
            }
        }


        private void onClick_btnConnect(object sender, RoutedEventArgs e)
        {

            COM_Port = cbox_COM.SelectedIndex + 1;
      
            Console.WriteLine(COM_Port);

             ConnectRFID();

        }

        private void btn_AddItem_Click(object sender, RoutedEventArgs e)
        {
            AddItem win2 = new AddItem();
            win2.Show();
            Console.WriteLine("Check List");
            for (int i = 0; i < UnknownRFIDList.Count; i++)
            {
                Console.WriteLine(i+ "] " + string.Concat(UnknownRFIDList[i].EPC, "---", UnknownRFIDList[i].timeStamp));
            }
        }
    }
}

    
