using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace mDNS
{

    public class mDNSHostInfo
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
        public string ServiceName { get; set; }

        public mDNSHostInfo(string name, string ipAddress)
        {
            Id = name;
            Address = ipAddress;
        }

        public mDNSHostInfo(DeviceInformation args)
        {
            // The IpAddress property contains a list of strings representing IP literals
            var ipAddresses = args.Properties["System.Devices.IpAddress"] as string[];
            // Use the first address in the list
            Address = ipAddresses[0];

            // Now extract the name
            var name = args.Properties["System.Devices.Dnssd.HostName"] as string;
            Id = name;

            var port = args.Properties["System.Devices.Dnssd.PortNumber"];
            Port = port.ToString();

            var serviceName = args.Properties["System.Devices.Dnssd.ServiceName"];
            ServiceName = serviceName.ToString();

        }

        public mDNSHostInfo(mDNSHostInfo input)
        {
            Id = input.Id;
            Address = input.Address;
            Port = input.Port;
            ServiceName = input.ServiceName;
        }

        public override string ToString()
        {
            return "Host: " + Id + " Address: " + Address + " Port: " + Port + " Service name: " + ServiceName;
        }
    }

    public interface imDNSClient
    {
        void mDNSStatusChange(DeviceWatcherStatus newStatus);
        void HostFound(mDNSHostInfo result);
    }

    public class mDNSManager
    {

        static Guid DnsSdProtocol = new Guid("{4526e8c1-8aac-4153-9b16-55e86ada0e54}");

        string queryString = "System.Devices.AepService.ProtocolId:={" + DnsSdProtocol + "} AND " +
            "System.Devices.Dnssd.Domain:=\"local\" AND System.Devices.Dnssd.ServiceName:=";

        DeviceWatcher watcher;

        string serviceName;
        imDNSClient client;

        private Dictionary<string, mDNSHostInfo> mDNSHostsFound;

        public mDNSManager(imDNSClient client, string serviceName = "\"_http._tcp\"")
        {
            this.serviceName = serviceName;
            this.client = client;

            string fullQueryString = queryString + serviceName;

            watcher = DeviceInformation.CreateWatcher(fullQueryString,
                new String[]
                {
                "System.Devices.Dnssd.HostName",
                "System.Devices.Dnssd.ServiceName",
                "System.Devices.Dnssd.TextAttributes",
                "System.Devices.Dnssd.PortNumber",
                "System.Devices.IpAddress" },
                DeviceInformationKind.AssociationEndpointService
                );

            watcher.Added += Device_Added;
            watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
            watcher.Removed += Device_Removed;
            watcher.Updated += Watcher_Updated;
            watcher.Stopped += Watcher_Stopped;

            mDNSHostsFound = new Dictionary<string, mDNSHostInfo>();
        }

        private void storemDNSHostInfo(DeviceInformation args)
        {
            mDNSHostInfo host = new mDNSHostInfo(args);

            if (mDNSHostsFound.ContainsKey(host.Id))
                return;

            mDNSHostsFound.Add(host.Id, host);
        }

        private void Watcher_Stopped(DeviceWatcher sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("***Watcher stopped ");
            client.mDNSStatusChange(watcher.Status);
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            System.Diagnostics.Debug.WriteLine("***Watcher updated: " + args.Id);
            client.mDNSStatusChange(watcher.Status);
        }

        private void Device_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            System.Diagnostics.Debug.WriteLine("***Watcher removed: " + args.Id);
            client.mDNSStatusChange(watcher.Status);
        }

        private void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            System.Diagnostics.Debug.WriteLine("***Enumeration completed ");
            client.mDNSStatusChange(watcher.Status);
            watcher.Stop();
        }

        private void Device_Added(DeviceWatcher sender, DeviceInformation args)
        {
            var ipAddresses = args.Properties["System.Devices.IpAddress"] as string[];
            System.Diagnostics.Debug.WriteLine("***Watcher added: " + args.Id + " " + ipAddresses[0]);
            storemDNSHostInfo(args);
            client.mDNSStatusChange(watcher.Status);
            client.HostFound(new mDNSHostInfo(args));
        }


        /// <summary>
        /// Gets a list of the currenntly located hosts
        /// </summary>
        public List<mDNSHostInfo> Hosts
        {
            get
            {
                List<mDNSHostInfo> result = new List<mDNSHostInfo>();
                foreach (mDNSHostInfo host in mDNSHostsFound.Values)
                    result.Add(new mDNSHostInfo(host));
                return result;
            }
        }

        /// <summary>
        /// Start finding hosts
        /// </summary>
        /// <returns>true if search was started OK</returns>
        public bool StartFind()
        {
            if (watcher.Status != DeviceWatcherStatus.Aborted &&
                watcher.Status != DeviceWatcherStatus.Created &&
                watcher.Status != DeviceWatcherStatus.Stopped)
                return false;

            mDNSHostsFound.Clear();

            watcher.Start();

            client.mDNSStatusChange(watcher.Status);

            return true;

        }

        /// <summary>
        /// Stops finding hosts
        /// </summary>
        public void StopFind()
        {
            if (watcher.Status != DeviceWatcherStatus.Started)
                return;
            watcher.Stop();
        }

    }
}
