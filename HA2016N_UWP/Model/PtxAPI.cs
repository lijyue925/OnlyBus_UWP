using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using HA2016N_UWP.Model;
using Newtonsoft.Json;

namespace HA2016N_UWP.Model
{
    class PtxAPI
    {
        public static async Task<List<Stop>> GetStops()
        {
            var http = new HttpClient();
            var url = new Uri($"http://ptx.transportdata.tw/MOTC/Bus/Stop/Kaohsiung?$format=json");
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Stop>>(result);

        }
        public static async Task<List<Route>> GetRoutes()
        {
            var http = new HttpClient();
            var url = new Uri($"http://ptx.transportdata.tw/MOTC/Bus/StopOfRoute/Kaohsiung?$format=json");
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Route>>(result);

        }

        public static async Task<List<Bus>> GetEstimatedTime(string routeName)
        {
            var http = new HttpClient();
            var url = new Uri("http://ptx.transportdata.tw/MOTC/v1/Bus/EstimatedTimeOfArrival/Kaohsiung/"+routeName+"?%24format=json");
            //var url = new Uri(temp.ToString());
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Bus>>(result);
        }
    }

}
