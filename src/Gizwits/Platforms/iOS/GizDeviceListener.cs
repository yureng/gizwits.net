using Com.Gizwits.Gizwifisdk.Api;
using Com.Gizwits.Gizwifisdk.Enumration;
using Foundation;
using System.Collections;

namespace Gizwits;

public abstract class GizDeviceListener : GizWifiDeviceDelegate
{
    readonly protected GizSDKListener sdk;
    private volatile bool _processing = false;

    internal GizDeviceListener(GizSDKListener sdk)
    {
        this.sdk = sdk;
    }

    protected Action<bool> OnSetSubscribe = null;
    public override void DidSetSubscribe(GizWifiDevice device, NSError result, bool isSubscribed)
    {
        var succeed = sdk.IsSuccess(result);
        if (OnSetSubscribe != null)
            OnSetSubscribe(succeed);
    }

    protected Action<bool> OnSetCustomInfo = null;
    public override void DidSetCustomInfo(GizWifiDevice device, NSError result)
    {
        var succeed = sdk.IsSuccess(result);
        if (OnSetCustomInfo != null)
            OnSetCustomInfo(succeed);
    }

    protected Action<bool, GizDeviceInfo> OnGetHardwareInfo = null;
    public override void DidGetHardwareInfo(GizWifiDevice device, NSError result, NSDictionary<NSString, NSString> hardwareInfo)
    {
        var succeed = sdk.IsSuccess(result);
        if (OnGetHardwareInfo != null)
            OnGetHardwareInfo(succeed, new GizDeviceInfo
            (
                hardwareInfo["wifiFirmwareId"]?.ToString(),
                hardwareInfo["wifiFirmwareVer"]?.ToString(),
                hardwareInfo["wifiSoftVersion"]?.ToString(),
                hardwareInfo["wifiHardVersion"]?.ToString(),
                hardwareInfo["mcuSoftVersion"]?.ToString(),
                hardwareInfo["mcuHardVersion"]?.ToString()
            ));
    }

    protected Action<DevStatus> OnUpdateNetStatus = null;
    public override void DidUpdateNetStatus(GizWifiDevice device, GizWifiDeviceNetStatus netStatus)
    {
        if (OnUpdateNetStatus != null)
            OnUpdateNetStatus.Invoke(GizSDK.ConvertTo(netStatus));
    }

    protected Action<IGizDeviceData, IGizDeviceData, IGizDeviceData, byte[], int> OnReceiveData = null;
    public override void DidReceiveAttrStatus(GizWifiDevice device, NSError result, NSDictionary attrStatus, NSDictionary adapterAttrStatus, NSNumber sn)
    {
        var succeed = sdk.IsSuccess(result);
        if (OnReceiveData != null)
        {
            if (succeed && attrStatus != null && !_processing)
            {
                _processing = true;
                try
                {
                    //var did = device.Did;

                    GizDeviceData data = null;
                    var dict = attrStatus["data"] as NSDictionary;
                    if (dict != null && dict.Count != 0)
                    {
                        data = new GizDeviceData(dict);
                    }

                    GizDeviceData faults = null;
                    dict = attrStatus["faults"] as NSDictionary;
                    if (dict != null && dict.Count != 0)
                    {
                        faults = new GizDeviceData(dict);
                    }

                    GizDeviceData alerts = null;
                    dict = attrStatus["alerts"] as NSDictionary;
                    if (dict != null && dict.Count != 0)
                    {
                        alerts = new GizDeviceData(dict); 
                    }

                    byte[] binary = null;
                    var value = attrStatus["binary"];
                    if (value != null)
                    {
                    }

                    OnReceiveData.Invoke(data, faults, alerts, binary, sn.Int32Value);
                }
                finally
                {
                    _processing = false;
                }
            }
        }
    }

    protected static void Write(GizWifiDevice device, IDictionary<string, object> command, int sn)
    {
        var keys = new List<NSString>(command.Count);
        foreach (string key in command.Keys)
            keys.Add(new NSString(key));

        var values = new List<NSObject>(command.Count);
        foreach (var value in command.Values)
            values.Add(NSObject.FromObject(value));

        var data = new NSDictionary<NSString, NSObject>(keys.ToArray(), values.ToArray());
        device.Write(data, sn);
    }
}