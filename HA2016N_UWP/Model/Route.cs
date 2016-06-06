using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HA2016N_UWP.Model
{
    [DataContract]
    public class Route
    {
        public Route()
        {
            this.RouteName = new RouteName();
            this.Stops = new List<Stop>();
        }
        [DataMember]
        public string RouteUID { get; set; }
        [DataMember]
        public string RouteID { get; set; }
        [DataMember]
        public RouteName RouteName { get; set; }
        [DataMember]
        public int Direction { get; set; }
        [DataMember]
        public List<Stop> Stops { get; set; }
    }

    public class RouteName
    {
        public string Zh_tw { get; set; }
        public string En { get; set; }
    }
}
