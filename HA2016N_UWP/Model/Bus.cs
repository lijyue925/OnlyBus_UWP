using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HA2016N_UWP.Model
{
    [DataContract]
    class Bus
    {
        public Bus()
        {
            this.StopName = new StopName();
            this.RouteName = new RouteName();
        }
        [DataMember]
        public string PlateNumb { get; set; }
        [DataMember]
        public string StopUID { get; set; }
        [DataMember]
        public string StopID { get; set; }
        [DataMember]
        public StopName StopName { get; set; }
        [DataMember]
        public string RouteUID { get; set; }
        [DataMember]
        public string RouteID { get; set; }
        [DataMember]
        public RouteName RouteName { get; set; }
        [DataMember]
        public int Direction { get; set; }
        [DataMember]
        public int MessageType { get; set; }
        [DataMember]
        public string SrcUpdateTime { get; set; }
        [DataMember]
        public string UpdateTime { get; set; }
        [DataMember]
        public int? EstimateTime { get; set; }
    }
}
