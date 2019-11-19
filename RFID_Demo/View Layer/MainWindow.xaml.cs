﻿using System;
using System.Linq;
using System.Windows;
using ThingMagic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using static RFID_Demo.UnknownRFID;
using static RFID_Demo.Itembook;
using System.Windows.Threading;
using System.Collections;

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

            DBHelper.EstablishConnection();


            DateTime TodayDates = DateTime.Now;

            //Setup book datagrid
            UnregisteredDataGrid.ItemsSource = UnknownRFIDList.getUnknownRFIDList();    //Setup unregistered RFID datagrid
            DataTable booksDT = new DataTable();
            DBHelper.UpdateBookTable();
            booksDT = DBHelper.GetDT();
            dg_BookTable.DataContext = booksDT;




            //Debugging test data
            UnknownRFIDList.addUnknownRFIDItem("2313213123213222", TodayDates.ToString(), "-24");



            //Set up UI
            updateConnectiveStatus();
        }
        public void FixBookDT()
        {
        }

        //------------ UI, Button, Events, General Purpose Functions Section ------------//


        /// <summary>
        /// Update the UI when connective status changes 
        /// Red: Module is disconnected
        /// Yellow: Module is Idle
        /// Green: Module is Scanning
        /// </summary>
        public void updateConnectiveStatus()
        {
            if (connectiveStatus == true)
            {
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_StartScanning_Click(object sender, RoutedEventArgs e)
        {
            if (toggelReader == false)
            {
                btn_ToggleRead.Content = "Stop";
                toggelReader = true;
                ReadBatchRFID();
            }
            else
            {
                btn_ToggleRead.Content = "Start Reading";
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
            else
            {
                MessageBox.Show("Please select an unregistered item above");
            }
        }
        private void btn_ClearUnregistered_click(object sender, RoutedEventArgs e)
        {
            UnknownRFIDList.RemoveALL();
        }


        private void btn_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dg_BookTable.SelectedItems[0] != null)
                {
                    DataRowView rows = (DataRowView)dg_BookTable.SelectedItems[0];

                    if (rows != null)
                    {

                        Console.WriteLine(rows["Book_RFID_EPC"]);

                        DBHelper.RemoveBookQuery(rows["Book_RFID_EPC"].ToString());
                    }

                    else
                    {
                        MessageBox.Show("Please select an item to remove");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please select an item to remove");
                Console.WriteLine(ex);
            }
        }
      
    
        private void dg_BookTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            new Action(() =>
            {
                check_Known = BookListing.CheckList(e);
                check_Unknown = UnknownRFIDList.CheckList(e);

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


    }

}




