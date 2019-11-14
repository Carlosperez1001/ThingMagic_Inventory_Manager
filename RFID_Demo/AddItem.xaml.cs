using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static RFID_Demo.UnknownRFID;

namespace RFID_Demo
{
    /// <summary>
    /// Interaction logic for AddItem.xaml
    /// </summary>
    /// 
 
    public partial class AddItem : Window
    {
        UnknownRFID selectedItem;
  
        public AddItem(UnknownRFID selectedItem)
        {
            this.selectedItem = selectedItem;
            InitializeComponent();

            lbl_SelectedEPC.Content = "EPC: " + selectedItem.EPC;
            lbl_SelectedTimeStamp.Content = "Timestamp: " + selectedItem.timeStamp;
        }

        private void btn_SubmitNewItem_Click(object sender, RoutedEventArgs e)
        {
            UnknownRFIDList.RemoveUnknownRFIDItem(selectedItem);
            //Close this window when done
        


        }
    }
}
