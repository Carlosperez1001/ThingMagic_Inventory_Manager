using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ThingMagic;
using static RFID_Demo.Itembook;

namespace RFID_Demo
{


    public class UnknownRFID
    {
        public string _EPC;
        public string _timeStamp;
        public string _RSSI;

        public UnknownRFID(String EPC, String timeStamp, String RSSI)
        {
            this.RSSI = RSSI;
            this.EPC = EPC;
            this.timeStamp = timeStamp;

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

        private static ObservableCollection<UnknownRFID> UnknownList = new ObservableCollection<UnknownRFID>();

        public class UnknownRFIDList
        {

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static ObservableCollection<UnknownRFID> getUnknownRFIDList()
            {
                return UnknownList;
            }



            /// <summary>
            /// 
            /// </summary>
            public static void printUnknow()
            {
                for (int i = 0; i < UnknownList.Count; i++)
                {
                    Console.WriteLine(string.Concat(UnknownList[i].EPC, "---", UnknownList[i].timeStamp));
                }
            }



            /// <summary>
            /// 
            /// </summary>
            /// <param name="EPC"></param>
            /// <param name="timeStamp"></param>
            /// <param name="RSSI"></param>
            public static void addUnknownRFIDItem(String EPC, String timeStamp, String RSSI)
            {

                UnknownList.Add(new UnknownRFID(EPC = EPC, timeStamp = timeStamp.ToString(), RSSI = RSSI));
                Console.WriteLine(EPC + "   " + timeStamp);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="selectedItem"></param>
            public static void RemoveUnknownRFIDItem(UnknownRFID selectedItem)
            {
                UnknownList.Remove(UnknownList.Where(i => i.EPC == selectedItem.EPC).Single());

            }


            /// <summary>
            /// 
            /// </summary>
            public static void RemoveALL()
            {
                UnknownList.Clear();

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e"></param>
            /// <returns></returns>
            public static bool CheckList(TagReadDataEventArgs e)
            {
                //Check dt if EPC already exist. 
                DataTable dt = DBHelper.GetDT();
                DataRow[] foundEPC = dt.Select("Book_RFID_EPC = '" + e.TagReadData.EpcString + "'");
                if (foundEPC.Length != 0)
                {
                    return false;
                }
                 

                if (UnknownList.Any(p => p.EPC == e.TagReadData.EpcString))
                {
                    var list = UnknownList.First(f => f.EPC == e.TagReadData.EpcString);
                    var index = UnknownList.IndexOf(list);
                    UnknownList[index].timeStamp = e.TagReadData.Time.ToString();
                    UnknownList[index].RSSI = e.TagReadData.Rssi.ToString();
                    return true;
                }
                else
                {
                    addUnknownRFIDItem(e.TagReadData.EpcString, e.TagReadData.Time.ToString(), e.TagReadData.Rssi.ToString());
                    return false;
                }
            }
        }
    }

}


