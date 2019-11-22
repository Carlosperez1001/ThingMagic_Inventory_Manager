using System;
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
using System.IO;
using System.Web;
using Application = System.Windows.Application;

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

            //Create WPF componects 
            InitializeComponent();
            //Attempt connection with DB.
            DBHelper.EstablishConnection();

            //Setup both datagrids
            UnregisteredDataGrid.ItemsSource = UnknownRFIDList.getUnknownRFIDList();
            dg_BookTable.ItemsSource = BookListing.getBookList();

            //Load all books with the DB
            loadBook();

            // Test data 
            // DateTime TodayDates = DateTime.Now;
            // UnknownRFIDList.addUnknownRFIDItem("2313213123213222", TodayDates.ToString(), "-24");

            //Setup UI 
            updateConnectiveStatus();
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
        /// Once a connection with the M6e module is established "(connectiveStatus == true)", the application can start scanning.
        /// When "btn_StartScannin" is clicked, update UI and change the bool "toggelReader" 
        /// </summary>
        private void btn_StartScanning_Click(object sender, RoutedEventArgs e)
        {
            if (toggelReader == false && connectiveStatus == true)
            {
                btn_ToggleRead.Content = "Stop";
                toggelReader = true;
                ReadBatchRFID();
                updateConnectiveStatus();
            }
            else if (toggelReader == true && connectiveStatus == true)
            {
                btn_ToggleRead.Content = "Start Reading";
                toggelReader = false;
                updateConnectiveStatus();
            }
            else
            {
                //connectiveStatus == false
                MessageBox.Show("M6e is not connected properly. Please select the correct COM port that connects the M6e module");
            }
        }


        /// <summary>
        /// Configure "Usb Virtual COM ports"
        /// Troubleshot - Check device manager with M6e plugged in & powered
        /// When "btnConnect" is clicked, the application will attempt a connection with the M6e
        /// </summary>
        private void onClick_btnConnect(object sender, RoutedEventArgs e)
        {
            COM_Port = cbox_COM.SelectedIndex + 1;
            Console.WriteLine(COM_Port);
            ConnectRFID();
        }

        /// <summary>
        ///  Adding an Unregistered RFID tag when it is seleted in the datagrid.
        ///  Open up "AddItem" form.
        /// </summary>
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


        /// <summary>
        /// UI function to clear datagrid.
        /// </summary>
        private void btn_ClearUnregistered_click(object sender, RoutedEventArgs e)
        {
            UnknownRFIDList.RemoveALL();
        }


        /// <summary>
        /// 
        /// </summary>
        private void btn_RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(dg_BookTable.GetType());
            try
            {
                Itembook rows = dg_BookTable.SelectedItem as Itembook;
                if (rows != null)
                {
                    Console.WriteLine(rows.EPC);
                    DBHelper.RemoveBookQuery(rows.EPC.ToString());
                    loadBook();
                }
                else
                {
                    MessageBox.Show("Please select an item to remove");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public void UpdateUnkownRFID_DG(bool CheckUknown)
        {
            if (CheckUknown == true)
            {//Marked as "Unknown" update infomation.
                this.Dispatcher.Invoke(() =>
                {
                    UnregisteredDataGrid.Items.Refresh();   //Update UI 
                    });

            }

        }
        public void UpdateBook_DG(bool Check_Book)
        {
            if (Check_Book == true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    dg_BookTable.Items.Refresh();   //Update UI 
                    });
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
                    string uri = "eapi:///com" + COM_Port;                                              //Configurations of COMs "USB Port"
                    reader = ThingMagic.Reader.Create(uri);                                             //Create Reader object
                    reader.Connect();
                    reader.ParamSet("/reader/region/id", ThingMagic.Reader.Region.NA);

                    int[] antennaList = null;
                    string str = "1,1";
                    antennaList = Array.ConvertAll(str.Split(','), int.Parse);                                   //Select antenna 1

                    SimpleReadPlan plan = new SimpleReadPlan(antennaList, TagProtocol.GEN2, null, null, 1000);  //Create "Plan" for module configuration 
                    reader.ParamSet("/reader/read/plan", plan);

                    connectiveStatus = true;
                    updateConnectiveStatus();                                                                    //Update UI
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    MessageBox.Show("COM " + COM_Port + " is not valid. ");
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTagRead(Object sender, TagReadDataEventArgs e)
        {
           
            bool check_Unknown = false;
            bool check_Book = false;

            try
            {
                Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    if (toggelReader == false)
                    {
                        Console.WriteLine("Read Booklisting End");
                        reader.StopReading();
                        return;
                    }
                    check_Unknown = UnknownRFIDList.CheckList(e);
                    UpdateUnkownRFID_DG(check_Unknown);


                }));

                Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() =>
              {
                  if (toggelReader == false)
                  {
                      Console.WriteLine("Read Booklisting End");
                      reader.StopReading();
                      return;
                  }

                  check_Book = BookListing.CheckList(e);
                  UpdateBook_DG(check_Book);
              }));
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }



        }


    }
}





