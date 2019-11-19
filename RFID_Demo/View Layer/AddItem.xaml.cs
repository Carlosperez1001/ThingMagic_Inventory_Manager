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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static RFID_Demo.Itembook;
using static RFID_Demo.UnknownRFID;
using MessageBox = System.Windows.Forms.MessageBox;

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
            
            if (CheckInput() == true) {
          //  UnknownRFIDList.RemoveUnknownRFIDItem(selectedItem);
                //Add new ItemBook object
                //CheckInput
                DBHelper.addBookQuery(selectedItem.EPC, selectedItem.timeStamp, selectedItem.RSSI, tbox_BookTitle.Text, tbox_Autor.Text, cbox_Genre.Text, imgPhoto.Source);
                //BookListing.addBooktem(selectedItem.EPC, selectedItem.timeStamp, selectedItem.RSSI, tbox_BookTitle.Text, tbox_Autor.Text, cbox_Genre.Text, imgPhoto.Source);
                BookListing.printBookList();
                this.Close(); 
            }
            //Close this window when donef



        }
        private bool CheckInput(){
            if (tbox_BookTitle.Text == null)
            {
                MessageBox.Show("Please enter a book title.");
                return false;
            }
         
            if (tbox_Autor.Text == null)
            {
                MessageBox.Show("Please enter an autor.");
                return false;
            }
            if (cbox_Genre.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a genre.");
                return false;
            }
            if (imgPhoto.Source == null)
            {
                MessageBox.Show("Please upload an image.");
                return false;
            }

            return true;
        }

        private void btn_UploadImage_Click(object sender, RoutedEventArgs e)
        {
            {
                OpenFileDialog op = new OpenFileDialog();
                op.Title = "Select a picture";
                op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                  "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                  "Portable Network Graphic (*.png)|*.png";
                if (op.ShowDialog() ==System.Windows.Forms.DialogResult.OK)
                {
                    imgPhoto.Source = new BitmapImage(new Uri(op.FileName));
                }

            }
        }
    }
}
