using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HA2016N_UWP.Model
{
   
    [DataContract]
    public class Stop
    {
        public Stop()
        {
            this.StopName = new StopName();
            this.StopPosition = new StopPosition();
        }
        [DataMember]
        public string StopUID { get; set; }
        [DataMember]
        public string StopID { get; set; }
        [DataMember]
        public string AuthorityID { get; set; }
        [DataMember]
        public StopName StopName { get; set; }
        [DataMember]
        public StopPosition StopPosition { get; set; }
        [DataMember]
        public string StopAddress { get; set; }
        [DataMember]
        public string UpdateTime { get; set; }
    }

    public class StopName
    {
        public string Zh_tw { get; set; }
        public string En { get; set; }
    }

    public class StopPosition
    {
        public double PositionLat { get; set; }
        public double PositionLon { get; set; }
    }
}
