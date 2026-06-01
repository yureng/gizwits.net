using Com.Gizwits.Gizwifisdk.Api;
using Com.Gizwits.Gizwifisdk.Enumration;
using Com.Gizwits.Gizwifisdk.Listener;
using Java.Util.Concurrent;
using System.Collections;

namespace Gizwits;

public abstract class GizDeviceListener : GizWifiDeviceListener
{
    readonly protected GizSDKListener sdk;
    private volatile bool _processing = false;

    internal GizDeviceListener(GizSDKListener sdk)
    {
        this.sdk = sdk;
    }

    protected Action<bool> OnSetSubscribe = null;
    public override void DidSetSubscribe(GizWifiErrorCode result, GizWifiDevice device, bool isSubscribed)
    {
        base.DidSetSubscribe(result, device, isSubscribed);

        var succeed = sdk.IsSuccess(result);
        if (OnSetSubscribe != null)
            OnSetSubscribe(succeed);
    }

    protected Action<bool> OnSetCustomInfo = null;
    public override void DidSetCustomInfo(GizWifiErrorCode result, GizWifiDevice device)
    {
        base.DidSetCustomInfo(result, device);

        var succeed = sdk.IsSuccess(result);
        if (OnSetCustomInfo != null)
            OnSetCustomInfo(succeed);
    }

    protected Action<bool, WifiDeviceInfo> OnGetHardwareInfo = null;
    public override void DidGetHardwareInfo(GizWifiErrorCode result, GizWifiDevice device, ConcurrentHashMap hardwareInfo)
    {
        base.DidGetHardwareInfo(result, device, hardwareInfo);

        var succeed = sdk.IsSuccess(result);
        if (OnGetHardwareInfo != null)
            OnGetHardwareInfo(succeed, new WifiDeviceInfo
            (
                hardwareInfo.Get("wifiFirmwareId")?.ToString(),
                hardwareInfo.Get("wifiFirmwareVer")?.ToString(),
                hardwareInfo.Get("wifiSoftVersion")?.ToString(),
                hardwareInfo.Get("wifiHardVersion")?.ToString(),
                hardwareInfo.Get("mcuSoftVersion")?.ToString(),
                hardwareInfo.Get("mcuHardVersion")?.ToString()
            ));
    }

    protected Action<DevStatus> OnUpdateNetStatus = null;
    public override void DidUpdateNetStatus(GizWifiDevice device, GizWifiDeviceNetStatus netStatus)
    {
        base.DidUpdateNetStatus(device, netStatus);

        if (OnUpdateNetStatus != null)
            OnUpdateNetStatus.Invoke(GizSDK.ConvertTo(netStatus));
    }

    protected Action<IWifiDeviceData, IWifiDeviceData, IWifiDeviceData, byte[], int> OnReceiveData = null;
    public override void DidReceiveAttrStatus(GizWifiErrorCode result, GizWifiDevice device, ConcurrentHashMap attrStatus, ConcurrentHashMap adapterAttrStatus, int sn)
    {
        base.DidReceiveAttrStatus(result, device, attrStatus, adapterAttrStatus, sn);

        var succeed = sdk.IsSuccess(result);
        if (OnReceiveData != null)
        {
            if (succeed && attrStatus != null && !_processing)
            {
                _processing = true;
                try
                {
                    //var did = device.Did;

                    WifiDeviceData data = null;
                    if (attrStatus.ContainsKey("data"))
                    {
                        data = new WifiDeviceData(attrStatus.Get("data"));
                    }

                    WifiDeviceData faults = null;
                    if (attrStatus.ContainsKey("faults"))
                    {
                        faults = new WifiDeviceData(attrStatus.Get("faults"));
                    }

                    WifiDeviceData alerts = null;
                    if (attrStatus.ContainsKey("alerts"))
                    {
                        alerts = new WifiDeviceData(attrStatus.Get("alerts"));
                    }

                    byte[] binary = null;
                    if (attrStatus.ContainsKey("binary"))
                    {
                        //binary = Convert.ToBytes(dataMap.Get("binary"));
                    }

                    OnReceiveData.Invoke(data, faults, alerts, binary, sn);
                }
                finally
                {
                    _processing = false;
                }
            }
        }
    }

    //protected Action<IWifiDeviceData, IWifiDeviceData, IWifiDeviceData, byte[], int> OnReceiveData = null;
    //public override void DidReceiveData(GizWifiErrorCode result, GizWifiDevice device, ConcurrentHashMap dataMap, int sn)
    //{        
    //}

    protected static void Write(GizWifiDevice device, IDictionary<string, object> command, int sn)
    {
        var data = new Java.Util.Concurrent.ConcurrentHashMap();
        data.PutAll(command as IDictionary);

        device.Write(data, sn);
    }
}