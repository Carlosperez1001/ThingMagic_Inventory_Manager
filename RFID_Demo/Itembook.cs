using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ThingMagic;

namespace RFID_Demo
{
    public class Itembook
    {
        public string _Title;
        public string _Autor;
        public string _Genre;
        public ImageSource _Image;

        public string _EPC;
        public string _timeStamp;
        public string _RSSI;

        // imgPhoto.Source = new BitmapImage
        public Itembook(String EPC, String timeStamp, String RSSI, string Title, string Autor,string Genre, ImageSource Image)
        {
            this.EPC = EPC;
            this.timeStamp = timeStamp;
            this.RSSI = RSSI;

            this.Title = Title;
            this.Autor = Autor;
            this.Genre = Genre;
            this.Image = Image;
        }
        public string EPC
        {
            get { return _EPC; }
            set { _EPC = value; }
        }
        public string timeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        public string RSSI
        {
            get { return _RSSI; }
            set { _RSSI = value; }
        }
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        public string Autor
        {
            get { return _Autor; }
            set { _Autor = value; }
        }
        public string Genre
        {
            get { return _Genre; }
            set { _Genre = value; }
        }
        public ImageSource Image
        {
            get { return _Image; }
            set { _Image = value; }
        }
        private static ObservableCollection<Itembook> BookList = new ObservableCollection<Itembook>();
        public class BookListing
        {
            public static ObservableCollection<Itembook> getBookList()
            {
                return BookList;
            }
            public static void printBookList()
            {
                Console.WriteLine("REEE");
                for (int i = 0; i < BookList.Count; i++)
                {
                    Console.WriteLine(string.Concat(BookList[i].EPC, "---", BookList[i].timeStamp));
                }
            }

                public static void addBooktem(String EPC, String timeStamp, String RSSI, string Title, string Autor, string Genre, ImageSource Image)
            {
                BookList.Add(new Itembook(EPC = EPC, timeStamp = timeStamp.ToString(), RSSI = RSSI, Title = Title, Autor= Autor, Genre = Genre, Image = Image));
                Console.WriteLine(EPC + "   " + timeStamp);
            }
            public static void RemoveBookItem(Itembook selectedItem)
            {
                BookList.Remove(BookList.Where(i => i.EPC == selectedItem.EPC).Single());

            }
            public static bool CheckList(TagReadDataEventArgs e)
            {
                if (BookList.Any(p => p.EPC == e.TagReadData.EpcString))
                {
                    var list = BookList.First(f => f.EPC == e.TagReadData.EpcString);
                    var index = BookList.IndexOf(list);
                    BookList[index].timeStamp = e.TagReadData.Time.ToString();
                    BookList[index].RSSI = e.TagReadData.Rssi.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


    }
    }

