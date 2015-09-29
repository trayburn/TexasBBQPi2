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
        private double _temp;
        public double Temp
        {
            get { return _temp; }
            set
            {
                _temp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Temp)));
            }
        }
        private PinViewModel _slaveInput = new PinViewModel();
        public PinViewModel SlaveInput
        {
            get { return _slaveInput; }
            set
            {
                _slaveInput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SlaveInput)));
            }
        }

        private PinViewModel _slaveOutput = new PinViewModel();
        public PinViewModel SlaveOutput
        {
            get { return _slaveOutput; }
            set
            {
                _slaveOutput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SlaveOutput)));
            }
        }

        private PinViewModel _commandSlave = new PinViewModel();
        public PinViewModel CommandSlave
        {
            get { return _commandSlave; }
            set
            {
                _commandSlave = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandSlave)));
            }
        }


        private PinViewModel _clock = new PinViewModel();
        public PinViewModel Clock
        {
            get { return _clock; }
            set
            {
                _clock = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Clock)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
