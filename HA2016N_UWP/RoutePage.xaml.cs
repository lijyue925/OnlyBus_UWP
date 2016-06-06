using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HA2016N_UWP.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace HA2016N_UWP
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class RoutePage : Page
    {
        private DispatcherTimer dispatcherTimer;

        public string StartStopName;
        public string EndStopName;
        public int Counter;

        private ObservableCollection<TransferBus> transferBuses;

        public ObservableCollection<TransferBus> TransferBuses
        {
            get { return transferBuses; }
            set
            {
                if (value != transferBuses)
                {
                    transferBuses = value;
                    //NotifyPropertyChanged();
                }
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}
        public RoutePage()
        {
            TransferBuses = new ObservableCollection<TransferBus>();
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressRing.IsActive = true;
            BusListView.Visibility = Visibility.Collapsed;
            await PlanRoute();
            ProgressRing.IsActive = false;
            BusListView.Visibility = Visibility.Visible;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private async void dispatcherTimer_Tick(object sender, object e)
        {
            if (Counter == 10)
            {
                //var tempList = new List<TransferBus>();
                Counter = 0;
                foreach (var transferBus in TransferBuses)
                {
                    transferBus.ArrivalInfo = await RefreshEstimatedTime(transferBus);
                    //tempList.Add(transferBus);
                }
                //TransferBuses.Clear();// = new ObservableCollection<TransferBus>(tempList);
                //tempList.ForEach(t => transferBuses.Add(t));
                
            }
            
            RefreshTextBlock.Text = $"{10 - Counter++}秒後更新";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string)
            {

                var name = e.Parameter.ToString().Split(',');
                if (name.Length != 2 || App.Stops.All(s => s.StopName.Zh_tw != name[0]) || App.Stops.All(s => s.StopName.Zh_tw != name[1]))
                    InputError();

                StartStopName = name[0];
                EndStopName = name[1];
            }



            TransferInfoTextBlock.Text = $"{StartStopName} → {EndStopName}";


            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            dispatcherTimer.Stop();
            base.OnNavigatedFrom(e);
        }
        private async Task PlanRoute()
        {
            var flag = false;

            var routeContainStartStop = App.Routes.FindAll(r => r.Stops.Any(s => s.StopName.Zh_tw == StartStopName));
            var routeContainEndStop = App.Routes.FindAll(r => r.Stops.Any(s => s.StopName.Zh_tw == EndStopName));

            var solutionA =
                routeContainStartStop.Intersect(routeContainEndStop)
                    .Where(
                        r =>
                            r.Stops.FindIndex(s => s.StopName.Zh_tw == StartStopName) <
                            r.Stops.FindIndex(s => s.StopName.Zh_tw == EndStopName)).ToList();


            if (solutionA.Any())
            {
                AddRouteToXaml(solutionA[0].RouteID, true);
                await AddEstimatedTimeAndBus(1, solutionA[0], StartStopName, EndStopName);
                flag = true;
            }
            else
            {
                var transferStops = App.Stops.FindAll(stop =>
                {
                    var sth =
                        routeContainStartStop.FindAll(
                            routeS => routeS.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    var sth2 =
                        routeContainEndStop.FindAll(
                            routeE => routeE.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    return sth > 0 && sth2 > 0;
                }).GroupBy(x => x.StopName.Zh_tw).Select(y => y.First()).ToList();
                foreach (var transferStop in transferStops)
                {
                    var head =
                        routeContainStartStop.FindAll(
                            routeS =>
                                routeS.Stops.Any(s => s.StopName.Zh_tw == transferStop.StopName.Zh_tw) &&
                                (routeS.Stops.FindIndex(st => st.StopName.Zh_tw == StartStopName) <
                                 routeS.Stops.FindIndex(sg => sg.StopName.Zh_tw == transferStop.StopName.Zh_tw)));
                    var end =
                        routeContainEndStop.FindAll(
                            routeE =>
                                routeE.Stops.Any(s => s.StopName.Zh_tw == transferStop.StopName.Zh_tw) &&
                                (routeE.Stops.FindIndex(st => st.StopName.Zh_tw == EndStopName) >
                                 routeE.Stops.FindIndex(sg => sg.StopName.Zh_tw == transferStop.StopName.Zh_tw)));
                    if (head.Count > 0 && end.Count > 0)
                    {
                        AddRouteToXaml(head[0].RouteID, false);
                        await AddEstimatedTimeAndBus(1, head[0], StartStopName, transferStop.StopName.Zh_tw);

                        AddRouteToXaml(end[0].RouteID, true);
                        await AddEstimatedTimeAndBus(2, end[0], transferStop.StopName.Zh_tw, EndStopName);

                        flag = true;
                        break;
                    }
                }
            }

            if (flag == false)
            {
                //所有含有起點的路線
                var SrouteContainStartStop = App.Routes.FindAll(r => r.Stops.Any(s => s.StopName.Zh_tw == StartStopName));
                //所有含有終點的路線
                var SrouteContainEndStop = App.Routes.FindAll(r => r.Stops.Any(s => s.StopName.Zh_tw == EndStopName));


                var a = new List<Route>();
                var b = new List<Route>();
                SrouteContainStartStop.ForEach(startRoute =>
                {
                    a = App.Routes.FindAll(allRoute => allRoute.Stops.Any(s => startRoute.Stops.Contains(s, new StopCompare())));
                    foreach (var endRoute in SrouteContainEndStop)
                    {
                        b = a.FindAll(allRoute => allRoute.Stops.Any(s => endRoute.Stops.Contains(s, new StopCompare())));
                        if (b.Count > 0)
                            break;
                    }
                });

                var transferStop1 = App.Stops.FindAll(stop =>
                {
                    var sth =
                        SrouteContainStartStop.FindAll(
                            routeS => routeS.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    var sth2 =
                        b.FindAll(
                            routeE => routeE.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    return sth > 0 && sth2 > 0;
                }).GroupBy(x => x.StopName.Zh_tw).Select(y => y.First()).ToList();


                var transferStop2 = App.Stops.FindAll(stop =>
                {
                    var sth =
                        b.FindAll(
                            routeS => routeS.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    var sth2 =
                        SrouteContainEndStop.FindAll(
                            routeE => routeE.Stops.Any(s => s.StopName.Zh_tw == stop.StopName.Zh_tw)).Count;
                    return sth > 0 && sth2 > 0;
                }).GroupBy(x => x.StopName.Zh_tw).Select(y => y.First()).ToList();

                foreach (var transferStop in transferStop1)
                {
                    var head =
                        SrouteContainStartStop.FindAll(
                            routeS =>
                                routeS.Stops.Any(s => s.StopName.Zh_tw == transferStop.StopName.Zh_tw) &&
                                (routeS.Stops.FindIndex(st => st.StopName.Zh_tw == StartStopName) <
                                 routeS.Stops.FindIndex(sg => sg.StopName.Zh_tw == transferStop.StopName.Zh_tw)));
                    var mid =
                        b.FindAll(
                            routeE =>
                                routeE.Stops.Any(s => s.StopName.Zh_tw == transferStop2[0].StopName.Zh_tw) &&
                                (routeE.Stops.FindIndex(st => st.StopName.Zh_tw == transferStop2[0].StopName.Zh_tw) >
                                 routeE.Stops.FindIndex(sg => sg.StopName.Zh_tw == transferStop.StopName.Zh_tw)));

                    var end =
                        SrouteContainEndStop.FindAll(
                            routeE =>
                                routeE.Stops.Any(s => s.StopName.Zh_tw == transferStop2[0].StopName.Zh_tw) &&
                                (routeE.Stops.FindIndex(st => st.StopName.Zh_tw == EndStopName) >
                                 routeE.Stops.FindIndex(sg => sg.StopName.Zh_tw == transferStop.StopName.Zh_tw)));

                    if (head.Count > 0 && mid.Count > 0 && end.Count > 0)
                    {
                        AddRouteToXaml(head[0].RouteID, false);
                        await AddEstimatedTimeAndBus(1, head[0], StartStopName, transferStop.StopName.Zh_tw);

                        AddRouteToXaml(mid[0].RouteID, false);
                        await AddEstimatedTimeAndBus(2, mid[0], transferStop.StopName.Zh_tw, transferStop2[0].StopName.Zh_tw);

                        AddRouteToXaml(end[0].RouteID, true);
                        await AddEstimatedTimeAndBus(3, end[0], transferStop2[0].StopName.Zh_tw, EndStopName);

                        flag = true;
                        break;
                    }
                }


            }
            //TransferBuses = new ObservableCollection<TransferBus>(TransferBuses.OrderBy(b => b.Id));

            //TransferBuses.OrderByDescending(b => b.Id);

            //if (!flag)
            //    ResultError();
        }

        private async Task<string> RefreshEstimatedTime(TransferBus bus)
        {
            var tempTimes = await PtxAPI.GetEstimatedTime(bus.Route.RouteName.Zh_tw);

            var tempArrivalTime = new List<string>();
            var allTime = tempTimes.FindAll(t => t.RouteID == bus.Route.RouteID && t.EstimateTime.HasValue);
            allTime.FindAll(x => x.StopName.Zh_tw == bus.EndStop.Substring(3)).OrderBy(y => y.EstimateTime.Value).ToList().ForEach(
                x =>
                {
                    var sec = TimeSpan.FromSeconds(Convert.ToDouble(x.EstimateTime.Value));
                    if (sec.TotalSeconds < 10)
                        tempArrivalTime.Add($"即將進站");
                    else
                        tempArrivalTime.Add($"{sec.Minutes:D2}分{sec.Seconds:D2}秒");
                });

            while (tempArrivalTime.Count < 2)
            {
                tempArrivalTime.Add("暫無資訊");
            }
            return $"第一班車：{tempArrivalTime[0]}\r\n第二班車：{tempArrivalTime[1]}";

        }
        private async Task AddEstimatedTimeAndBus(int id, Route route, string startName, string stopName)
        {
            var tempTimes = await PtxAPI.GetEstimatedTime(@route.RouteName.Zh_tw);

            lock (new object())
            {
                var tempArrivalTime = new List<string>();
                var allTime = tempTimes.FindAll(t => t.RouteID == route.RouteID && t.EstimateTime.HasValue);
                allTime.FindAll(x => x.StopName.Zh_tw == stopName).OrderBy(y => y.EstimateTime.Value).ToList().ForEach(
                    x =>
                    {
                        var sec = TimeSpan.FromSeconds(Convert.ToDouble(x.EstimateTime.Value));
                        if (sec.TotalSeconds < 10)
                            tempArrivalTime.Add($"即將進站");
                        else
                            tempArrivalTime.Add($"{sec.Minutes:D2}分{sec.Seconds:D2}秒");
                    });

                while (tempArrivalTime.Count < 2)
                {
                    tempArrivalTime.Add("暫無資訊");
                }

                AddBusToXaml(id, route, startName, stopName, tempArrivalTime);
            }

        }

        private void AddRouteToXaml(string routeNo, bool last = false)
        {
            var templateBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };
            var templateGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 32, 93, 160)),
                Padding = new Thickness(5, 5, 5, 5),
                VerticalAlignment = VerticalAlignment.Center
            };


            templateBlock.Text = routeNo;
            templateGrid.Children.Add(templateBlock);
            RouteStackPanel.Children.Add(templateGrid);
            if (!last)
            {
                var templateArrow = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 0),
                    Padding = new Thickness(5),
                    Text = "→"
                };
                RouteStackPanel.Children.Add(templateArrow);
            }
        }

        private void AddBusToXaml(int id, Route route, string startStop, string endStop, List<string> tempArrT)
        {
            var tempStartStop = $"起站：{startStop}";
            var tempEndStop = $"到站：{endStop}";
            var tempArrivalInfo = $"第一班車：{tempArrT[0]}\r\n第二班車：{tempArrT[1]}";

            TransferBuses.Add(new TransferBus
            {
                Id = id,
                Route = route,
                BusNo = route.RouteName.Zh_tw,
                ArrivalInfo = tempArrivalInfo,
                StartStop = tempStartStop,
                EndStop = tempEndStop
            });
        }
        private async void InputError()
        {
            var dialog = new Windows.UI.Popups.MessageDialog("無法找到該站名！", "錯誤");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("重新輸入") { Id = 0 });

            dialog.DefaultCommandIndex = 0;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
                this.Frame.Navigate(typeof(MainPage));

        }

        private async void ResultError()
        {
            var dialog = new Windows.UI.Popups.MessageDialog("沒有辦法組合出這樣的路線！", "傷心QQ");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("重新查詢") { Id = 0 });

            dialog.DefaultCommandIndex = 0;

            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
                this.Frame.Navigate(typeof(MainPage));

        }
    }

    class StopCompare : IEqualityComparer<Stop>
    {
        public bool Equals(Stop x, Stop y)
        {
            return x.StopName.Zh_tw == y.StopName.Zh_tw;
        }
        public int GetHashCode(Stop codeh)
        {
            return codeh.StopUID.GetHashCode();
        }
    }

}
