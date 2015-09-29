using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    public class AppViewModel : INotifyPropertyChanged
    {
        private string _BinaryString;
        private int _AdcReading;
        private double _Temp;
        public double Temp
        {
            get { return _Temp; }
            set
            {
                if (_Temp == value) return;
                _Temp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temp)));
            }
        }


        public int AdcReading
        {
            get { return _AdcReading; }
            set
            {
                if (_AdcReading == value) return;
                _AdcReading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AdcReading)));
            }
        }


        public string BinaryString
        {
            get { return _BinaryString; }
            set
            {
                if (_BinaryString == value) return;
                _BinaryString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BinaryString)));
            }
        }
        


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
