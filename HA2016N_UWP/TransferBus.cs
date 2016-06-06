using System.ComponentModel;
using System.Runtime.CompilerServices;
using HA2016N_UWP.Annotations;
using HA2016N_UWP.Model;

namespace HA2016N_UWP
{
    public class TransferBus : INotifyPropertyChanged
    {
        public TransferBus()
        {
            this.Route = new Route();
        }
        public int Id { get; set; }
        public Route Route { get; set; }
        public string BusNo { get; set; }
        private string arrivalInfo;

        public string ArrivalInfo
        {
            get { return arrivalInfo; }
            set
            {
                if (arrivalInfo != value)
                {
                    arrivalInfo = value;
                    OnPropertyChanged();
                }
            }
        }
        public string StartStop { get; set; }
        public string EndStop { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
