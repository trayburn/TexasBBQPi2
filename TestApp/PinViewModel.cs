using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace TestApp
{
    public class PinViewModel : INotifyPropertyChanged
    {
        private GpioPin pin;

        public PinViewModel()
        {
        }

        public PinViewModel(string name, GpioOpenStatus status, GpioPin pin)
        {
            Name = name;
            Status = status.ToString();
            this.pin = pin;
            Number = pin.PinNumber;
        }

        private string _name = "Name";
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        private int _number = 666;
        public int Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number)));
                }
            }
        }
        private string _status = "Design Mode";
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        public bool _value = false;
        public bool Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
