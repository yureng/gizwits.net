namespace Gizwits
{
    public interface IWifiDeviceGenerator
    {
        object Create(string productKey, string id, string name, string mac, string ip, string remark, bool isBind, bool isLan, bool isDisabled,
            bool? bleWork, 
            DevStatus status, DevType type, Roles role);
    }

    // 设备状态
    public enum DevStatus { Offline, Online, Available }

    // 设备类型
    public enum DevType
    {
        Normal,
        Central,
        SubDev,
        Sensor,
        Executor,
        Special
    };

    // 操作角色
    public enum Roles { Special, Owner, Guest, Normal };

    public struct WifiDeviceInfo
    {
        internal WifiDeviceInfo(string firmwareId, string firmwareVersion, string wifiSoftVersion, string wifiHardVersion, string mcuSoftVersion, string mcuHardVersion)
        {
            this.FirmwareId = firmwareId;
            this.FirmwareVersion = firmwareVersion;
            this.WifiSoftVersion = wifiSoftVersion;
            this.WifiHardVersion = wifiHardVersion;
            this.McuSoftVersion = mcuSoftVersion;
            this.McuHardVersion = mcuHardVersion;
        }

        // 固件 ID
        public readonly string FirmwareId;

        // 固件版本
        public readonly string FirmwareVersion;

        // Wifi 软件版本
        public readonly string WifiSoftVersion;

        // Wifi 硬件版本
        public readonly string WifiHardVersion;

        // MCU 软件版本
        public readonly string McuSoftVersion;

        // MCU 硬件版本
        public readonly string McuHardVersion;
    }
}
