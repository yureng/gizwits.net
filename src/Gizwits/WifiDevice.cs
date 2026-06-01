
//using Gizwits;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Gizwits
//{
//    public abstract class WifiDevice : IDisposable
//    {
//        protected GizDevice gizDev = null;
//        protected bool SdkLoaded => gizDev != null;

//        public WifiDevice(string productKey, string id)
//        {
//            if (String.IsNullOrEmpty(productKey))
//                throw new ArgumentNullException(nameof(productKey));

//            //if (String.IsNullOrEmpty(id))
//            //    throw new ArgumentNullException(nameof(id));

//            this.ProductKey = productKey;
//            this.Id = id;
//        }

//        public string ProductKey { get; private set; }
//        public string Id { get; private set; }
//        public string Name { get; protected set; }
//        public string MAC { get; protected set; }
//        public string IP { get; protected set; }
//        public string Remark { get; protected set; }
//        public bool IsBind { get; protected set; }
//        public bool IsLAN { get; protected set; }
//        public bool? BleWork { get; protected set; }
//        public bool IsDisabled { get; protected set; } = true;
//        public DevStatus Status { get; protected set; } = DevStatus.Offline;
//        public DevType Type { get; protected set; } = DevType.Normal;
//        public Roles Role { get; protected set; } = Roles.Normal;

//        public async Task SdkLoad()
//        {
//            if (this.gizDev == null)
//            {
//                var sdk = App.Current.Handler.GetService<Gizwits.GizSDK>();
//                this.gizDev = sdk.GetGizDevice(
//                        this.Id,
//                        (data, faults, alerts, binary, sn) =>
//                        {
//                            if (data != null) OnDataReceived(data);
//                            if (faults != null) OnFaultsReceived(faults);
//                            if (alerts != null) OnAlertsReceived(alerts);
//                            if (binary != null) OnBinaryReceived(binary);
//                        },
//                        (status) => OnStatusChanged(status));

//                this.gizDev.AutoGetDeviceStatus = true;
//                var succed = await this.gizDev.SetSubscribe(true);
//            }
//        }

//        public async Task SdkUnload()
//        {
//            if (this.gizDev != null)
//            {
//                try
//                {
//                    var succed = await this.gizDev.SetSubscribe(false);
//                }
//                finally
//                {
//                    this.gizDev.Dispose();
//                    this.gizDev = null;
//                }
//            }
//        }

//        public virtual void RefreshData(params string[] attrs)
//        {
//            this.gizDev?.RequestGetData(attrs);
//        }

//        internal void Update(string name, string mac, string ip, string remark, bool isBind, bool isLan, bool isDisabled, bool? bleWork, DevStatus status, DevType type, Roles role)
//        {
//            this.Name = name;
//            this.MAC = mac;
//            this.IP = ip;
//            this.Remark = remark;
//            this.IsBind = isBind;
//            this.IsLAN = isLan;
//            this.IsDisabled = isDisabled;
//            this.BleWork = bleWork;           
//            this.Status = status;
//            this.Type = type;
//            this.Role = role;
//        }

//        protected virtual void OnStatusChanged(DevStatus status)
//        {
//            if (this.Status != status)
//                this.Status = status;
//        }

//        protected virtual void OnDataReceived(IWifiDeviceData data)
//        { }

//        protected virtual void OnFaultsReceived(IWifiDeviceData faults)
//        { }

//        protected virtual void OnAlertsReceived(IWifiDeviceData alerts)
//        { }

//        protected virtual void OnBinaryReceived(byte[] binary)
//        { }

//        async void IDisposable.Dispose()
//        {
//            await SdkUnload();
//        }
//    }

//    public class WifiDeviceDefaultGenerator<T> : IWifiDeviceGenerator where T: WifiDevice, new()
//    {
//        public object Create(string productKey, string id, string name, string mac, string ip, string remark, bool isBind, bool isLan, bool isDisabled, bool? bleWork, DevStatus status, DevType type, Roles role)
//        {
//            switch(productKey)
//            {
//                default:
//                    var dev = new T(productKey, id);                    
//                    dev.Update(name, mac, ip, remark, isBind, isLan, isDisabled, bleWork, status, type, role);

//                    return dev;
//            }
//        }
//    }
//}
