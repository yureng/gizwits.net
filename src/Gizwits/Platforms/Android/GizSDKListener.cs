using Android.Content;
using Com.Gizwits.Gizwifisdk.Api;
using Com.Gizwits.Gizwifisdk.Enumration;
using Com.Gizwits.Gizwifisdk.Listener;
using Java.Util.Concurrent;
using System.Threading;

namespace Gizwits;

public abstract class GizSDKListener : GizWifiSDKListener
{
    protected abstract void SendMessage(string msg, string mtype = null);

    // 事件通知        
    internal Action<GizEventType, GizWifiErrorCode> OnNotifyEvent = null;
    public override void DidNotifyEvent(GizEventType eventType, Java.Lang.Object eventSource, GizWifiErrorCode eventId, string eventMessage)
    {
        base.DidNotifyEvent(eventType, eventSource, eventId, eventMessage);

        OnNotifyEvent?.Invoke(eventType, eventId);
        this.SendMessage(eventMessage ?? eventId.ToString(), eventType?.Name().ToString());
    }

    // 发现设备
    //protected Action<bool, IEnumerable<IWifiDevice>> OnDiscovered = null;
    protected Action<bool> OnDiscovered = null;
    public override void DidDiscovered(GizWifiErrorCode result, IList<GizWifiDevice> deviceList)
    {
        base.DidDiscovered(result, deviceList);

        var succeed = IsSuccess(result);
        if (OnDiscovered != null)
        {
            //var devices = succeed ? deviceList : GizWifiSDK.SharedInstance().DeviceList;
            //OnDiscovered.Invoke(succeed, succeed ? GizSDK.ConvertTo(deviceList) : null);
            OnDiscovered.Invoke(succeed);
        }
    }

    // BLE 设备发现
    protected Action<bool> OnDiscoverBleDevice = null;
    public override void DidDiscoverBleDevice(GizWifiErrorCode result, IList<GizWifiBleDevice> deviceList)
    {
        base.DidDiscoverBleDevice(result, deviceList);

        var succeed = IsSuccess(result);
        if (OnDiscoverBleDevice != null)
            OnDiscoverBleDevice.Invoke(succeed);
    }

    // 发送短信验证码
    protected Action<bool> OnRequestSendPhoneSMSCode = null;
    public override void DidRequestSendPhoneSMSCode(GizWifiErrorCode result, string token)
    {
        base.DidRequestSendPhoneSMSCode(result, token);

        var succeed = IsSuccess(result);
        if (OnRequestSendPhoneSMSCode != null)
            OnRequestSendPhoneSMSCode.Invoke(succeed);
    }

    // 用户登录
    protected Action<bool, string, string> OnUserLogin = null;
    public override void DidUserLogin(GizWifiErrorCode result, string uid, string token)
    {
        base.DidUserLogin(result, uid, token);

        var succeed = IsSuccess(result);
        if (OnUserLogin != null)
            OnUserLogin.Invoke(succeed, uid, token);
    }

    // 用户登出
    protected Action<bool> OnUserLogout = null;
    public override void DidUserLogout(GizWifiErrorCode result)
    {
        base.DidUserLogout(result);

        var succeed = IsSuccess(result);
        if (OnUserLogout != null)
            OnUserLogout.Invoke(succeed);
    }

    // 用户注册
    protected Action<bool, string, string> OnUserRegister = null;
    public override void DidRegisterUser(GizWifiErrorCode result, string uid, string token)
    {
        base.DidRegisterUser(result, uid, token);

        var succeed = IsSuccess(result);
        if (OnUserRegister != null)
            OnUserRegister.Invoke(succeed, uid, token);
    }

    // 用户更改密码
    protected Action<bool> OnUserChangePassword = null;
    public override void DidChangeUserPassword(GizWifiErrorCode result)
    {
        base.DidChangeUserPassword(result);

        var succeed = IsSuccess(result);
        if (OnUserChangePassword != null)
            OnUserChangePassword.Invoke(succeed);
    }

    // 获取用户信息
    protected Action<bool, GizUserInfo> OnGetUserInfo = null;
    public override void DidGetUserInfo(GizWifiErrorCode result, GizUserInfo userInfo)
    {
        base.DidGetUserInfo(result, userInfo);

        var succeed = IsSuccess(result);
        if (OnGetUserInfo != null)
            OnGetUserInfo.Invoke(succeed, userInfo);
    }

    // 绑定设备
    protected Action<bool, string> OnDeviceBind = null;
    public override void DidBindDevice(GizWifiErrorCode result, string did)
    {
        base.DidBindDevice(result, did);

        var succeed = IsSuccess(result);
        if (OnDeviceBind != null)
            OnDeviceBind(succeed, did);
    }

    // 解绑设备
    protected Action<bool, string> OnDeviceUnBind = null;
    public override void DidUnbindDevice(GizWifiErrorCode result, string did)
    {
        base.DidUnbindDevice(result, did);

        var succeed = IsSuccess(result);
        if (OnDeviceUnBind != null)
            OnDeviceUnBind(succeed, did);
    }
    
    protected Action<bool, IEnumerable<string>> OnGetSSIDList = null;
    public override void DidGetSSIDList(GizWifiErrorCode result, IList<GizWifiSSID> ssidInfoList)
    {
        base.DidGetSSIDList(result, ssidInfoList);

        var succeed = IsSuccess(result);
        if (OnGetSSIDList != null)
            OnGetSSIDList(succeed, ssidInfoList?.Select(s => s.Ssid));
    }

    // 设备配网
    protected Action<int, GizWifiDevice> OnDeviceOnboarding = null;
    public override void DidSetDeviceOnboarding(GizWifiErrorCode result, GizWifiDevice device)
    {
        base.DidSetDeviceOnboarding(result, device);

        var succeed = IsSuccess(result);
        if (OnDeviceOnboarding != null)
        {
            int r = succeed ? 1 : (result == GizWifiErrorCode.GizSdkDeviceConfigIsRunning ? 0 : -1);
            OnDeviceOnboarding.Invoke(r, device);
        }
    }

    protected void Start(ProductInfo info)
    {
        ConcurrentHashMap appInfo;
        List<ConcurrentHashMap> productInfo = null;
        ConcurrentHashMap cloudServiceInfo = null;

        var dict = new Dictionary<string, object>();
        dict.Add("appId", info.App_ID);
        dict.Add("appSecret", info.App_Secret);
        appInfo = new(dict);            

        if (info.Product_Infos != null && info.Product_Infos.Count() != 0)
        {
            productInfo = new();
            foreach (var pi in info.Product_Infos)
            {
                var p = new Dictionary<string, object>();
                p.Add("productKey", pi.Item1);
                p.Add("productSecret", pi.Item2);
                if (!String.IsNullOrEmpty(pi.Item3))
                    p.Add("usingAdapter", pi.Item3);

                productInfo.Add(new ConcurrentHashMap(p));
            }
        }

        GizWifiSDK.SharedInstance().SetListener(this);
        GizWifiSDK.SharedInstance().StartWithAppInfo(Android.App.Application.Context, appInfo, productInfo, cloudServiceInfo, false);
    }

    protected void SetDeviceOnboardingDeploy(string ssid, string key, GizWifiConfigureMode mode, string prefix, int? timeout, IList<GizWifiGAgentType> types, bool bind)
    {
        GizWifiSDK.SharedInstance().SetDeviceOnboardingDeploy(ssid, key, mode, prefix, timeout ?? 30, types, bind);
    }

    protected Action OnTokenInvalid = null;
    internal bool IsSuccess(GizWifiErrorCode result)
    {
        if (result != null)
        {
            if (result == GizWifiErrorCode.GizSdkSuccess)
                return true;

            SendMessage(result.ToString());

            if (result == GizWifiErrorCode.GizSdkTokenInvalid || result == GizWifiErrorCode.GizOpenapiTokenInvalid || result == GizWifiErrorCode.GizOpenapiTokenExpired)
                OnTokenInvalid?.Invoke();
        }
        return false;
    }
}