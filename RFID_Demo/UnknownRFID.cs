using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.timeStamp = timeStamp; //RawTagData.Substring(44);
            

        }
        public string EPC
        { 
            get { return _EPC; }
            set { _EPC = value; }
           
        }
        public string timeStamp {
            get { return _timeStamp; }
            set { _timeStamp = value; }
           
        }
        public string RSSI
        {
            get { return _RSSI; }
            set { _RSSI = value; }
         
        }
    public void printRFID()
        {
          //  Console.WriteLine(this.EPC + "   " + this.timeStamp);
        }

       
    }


}

