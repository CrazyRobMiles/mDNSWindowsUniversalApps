using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using mDNS;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HostMaster
{
    /// <summary>   /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, imDNSClient
    {

        mDNSManager mDNS;

        ObservableCollection<mDNSHostInfo> hosts = new ObservableCollection<mDNSHostInfo>();


        public MainPage()
        {
            this.InitializeComponent();

            mDNS = new mDNSManager(this);
        }

        public async void mDNSStatusChange(DeviceWatcherStatus newStatus)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                statusTextBlock.Text = newStatus.ToString();
            });
        }

        async public void HostFound(mDNSHostInfo result)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                hosts.Add(result);
            });

        }

        private void findHosts()
        {
            hosts.Clear();
            mDNS.StartFind();
        }

        private void abortSearch()
        {
            mDNS.StopFind();
        }

        private void findRobotsClicked(object sender, RoutedEventArgs e)
        {
            findHosts();
        }

        private void abortSearchClicked(object sender, RoutedEventArgs e)
        {
            abortSearch();
        }
    }

}
