using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HA2016N_UWP.Model;
using Newtonsoft.Json;

//空白頁項目範本收錄在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HA2016N_UWP
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Init();

            StartTextBox.Text = "覺民路口";
            EndTextBox.Text = "高雄港路外停車場站";
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RoutePage), StartTextBox.Text+","+ EndTextBox.Text);
        }

        private async void Init()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StartTextBox.Visibility = Visibility.Collapsed;
                EndTextBox.Visibility = Visibility.Collapsed;
                SendButton.Visibility = Visibility.Collapsed;
                LoadingIndicator.IsActive = true;

                if (await localFolder.TryGetItemAsync("stops.json") != null && await localFolder.TryGetItemAsync("routes.json") != null)
                {
                    var stopsFile = await localFolder.GetFileAsync("stops.json");
                    var content = await FileIO.ReadTextAsync(stopsFile);
                    App.Stops = JsonConvert.DeserializeObject<List<Stop>>(content);

                    var routesFile = await localFolder.GetFileAsync("routes.json");
                    content = await FileIO.ReadTextAsync(routesFile);
                    App.Routes = JsonConvert.DeserializeObject<List<Route>>(content);
                }
                else
                {
                    StorageFile stopsFile =
                        await localFolder.CreateFileAsync("stops.json", CreationCollisionOption.ReplaceExisting);
                    StorageFile routesFile =
                        await localFolder.CreateFileAsync("routes.json", CreationCollisionOption.ReplaceExisting);

                    App.Stops = await PtxAPI.GetStops();
                    App.Routes = await PtxAPI.GetRoutes();
                    var json = JsonConvert.SerializeObject(App.Stops);
                    await FileIO.WriteTextAsync(stopsFile, json);
                    json = JsonConvert.SerializeObject(App.Routes);
                    await FileIO.WriteTextAsync(routesFile, json);
                }
            }
            finally
            {
                LoadingIndicator.IsActive = false;
                StartTextBox.Visibility = Visibility.Visible;
                EndTextBox.Visibility = Visibility.Visible;
                SendButton.Visibility = Visibility.Visible;
            }
        }

        private void TextBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var filtered = new List<string>();
            App.Stops.FindAll(x => x.StopName.Zh_tw.StartsWith(sender.Text)).ForEach(s =>
            {
                if (!filtered.Contains(s.StopName.Zh_tw))
                    filtered.Add(s.StopName.Zh_tw);
            });
            sender.ItemsSource = filtered;
        }

    }
}
