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
using static RFID_Demo.UnknownRFID;
using static RFID_Demo.Itembook;
using System.Windows.Threading;

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
        

        public MainWindow()
        {
            InitializeComponent();

      

                //Test data when hardware is unavailable 
                DateTime TodayDates =  DateTime.Now;
                dg_BookTable.ItemsSource = BookListing.getBookList();

                UnregisteredDataGrid.ItemsSource = UnknownRFIDList.getUnknownRFIDList();
                
            //Debugging test data

                 UnknownRFIDList.addUnknownRFIDItem("2313213123213222", TodayDates.ToString(), "-24");
            
           

            //Set up UI
            updateConnectiveStatus();
        }

        //------------ UI, Button, Events, General Purpose Functions Section ------------//


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
         
            UnknownRFID selectedItem = (UnknownRFID)UnregisteredDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                AddItem win2 = new AddItem(selectedItem);
                win2.Show();
                win2.Activate();
                win2.Topmost = true;
            }
            else {
                MessageBox.Show("Please select an unregistered item above");
                    }
        }
        private void btn_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Itembook selectedItem = (Itembook)dg_BookTable.SelectedItem;
            if (selectedItem != null)
            {
                BookListing.RemoveBookItem(selectedItem);
            }
            else
            {
                MessageBox.Show("Please select an item to remove");
            }
        }


            //----------RFID Module Functions Section----------//


            /// <summary>
            /// Establishes a proper connection with the system and M6e Module. 
            /// Config module settings such as [antenna port, region,tag reading/writing protocol]
            /// Creates a "Reader" object from the Mecury API
            /// </summary>
            public void ConnectRFID()
        {
            {
                try
                {
                    string uri = "eapi:///com" + COM_Port;                        //Configurations of COMs "USB Port"
                    reader = ThingMagic.Reader.Create(uri);                       //Create Reader object
                    reader.Connect();
                    reader.ParamSet("/reader/region/id", ThingMagic.Reader.Region.NA);
                
                    int[] antennaList = null;
                    string str = "1,1";
                    antennaList = Array.ConvertAll(str.Split(','), int.Parse);     //Select antenna 1

                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);  //Create "Plan" for module configuration 
                    reader.ParamSet("/reader/read/plan", plan);
                   
                    connectiveStatus = true;
                    updateConnectiveStatus();                                       //Update UI
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
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

        private void OnTagRead(Object sender, TagReadDataEventArgs e)
        {
            if (toggelReader == false)
            {
                return;
            }

           
            bool check_Known = false;
            bool check_Unknown = false;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
            new Action(() => {
                check_Known = BookListing.CheckList(e);
                check_Unknown =  UnknownRFIDList.CheckList(e);

             }));

            if (check_Known == true) { }
            this.Dispatcher.Invoke(() =>
            {
                dg_BookTable.Items.Refresh();
            });

            if (check_Unknown == true) { }
                this.Dispatcher.Invoke(() =>
                {
                    UnregisteredDataGrid.Items.Refresh();
                });

            }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

      
    }

    }




    
