using Com.Gizwits.Gizwifisdk.Api;
using Com.Gizwits.Gizwifisdk.Enumration;
using Foundation;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Gizwits;

public abstract class GizSDKListener : GizWifiSDKDelegate
{
    protected abstract void SendMessage(string msg, string mtype = null);

    // 事件通知        
    public Action<GizEventType, GizWifiErrorCode> OnNotifyEvent = null;
    public override void DidNotifyEvent(GizWifiSDK wifiSDK, GizEventType eventType, NSObject eventSource, GizWifiErrorCode eventID, string eventMessage)
    {
        //base.DidNotifyEvent(wifiSDK, eventType, eventSource, eventID, eventMessage);
        //不要调用 base 方法：Exception of type 'Foundation.You_Should_Not_Call_base_In_This_Method' was thrown.

        OnNotifyEvent?.Invoke(eventType, eventID);
        this.SendMessage(eventMessage ?? eventID.ToString(), eventType.ToString());
    }

    // 发现设备
    protected Action<bool> OnDiscovered = null;
    public override void DidDiscovered(GizWifiSDK wifiSDK, NSError result, GizWifiDevice[] deviceList)
    {
        var succeed = IsSuccess(result);
        if (OnDiscovered != null)
            OnDiscovered.Invoke(succeed);
    }

    // BLE 设备发现
    protected Action<bool> OnDiscoverBleDevice = null;
    public override void DidDiscoverBleDevice(GizWifiSDK wifiSDK, NSError result, GizWifiBleDevice[] deviceList)
    {
        var succeed = IsSuccess(result);
        if (OnDiscoverBleDevice != null)
            OnDiscoverBleDevice.Invoke(succeed);
    }

    // 发送短信验证码
    protected Action<bool> OnRequestSendPhoneSMSCode = null;
    public override void DidRequestSendPhoneSMSCode(GizWifiSDK wifiSDK, NSError result, string token)
    {
        var succeed = IsSuccess(result);
        if (OnRequestSendPhoneSMSCode != null)
            OnRequestSendPhoneSMSCode.Invoke(succeed);
    }

    // 用户登录
    protected Action<bool, string, string> OnUserLogin = null;
    public override void DidUserLogin(GizWifiSDK wifiSDK, NSError result, string uid, string token)
    {
        var succeed = IsSuccess(result);
        if (OnUserLogin != null)
            OnUserLogin.Invoke(succeed, uid, token);
    }

    // 用户登出
    protected Action<bool> OnUserLogout = null;
    public override void DidUserLogout(GizWifiSDK wifiSDK, NSError result)
    {
        var succeed = IsSuccess(result);
        if (OnUserLogout != null)
            OnUserLogout.Invoke(succeed);
    }

    // 用户注册
    protected Action<bool, string, string> OnUserRegister = null;
    public override void DidRegisterUser(GizWifiSDK wifiSDK, NSError result, string uid, string token)
    {
        var succeed = IsSuccess(result);
        if (OnUserRegister != null)
            OnUserRegister.Invoke(succeed, uid, token);
    }

    // 用户更改密码
    protected Action<bool> OnUserChangePassword = null;
    public override void DidChangeUserPassword(GizWifiSDK wifiSDK, NSError result)
    {
        var succeed = IsSuccess(result);
        if (OnUserChangePassword != null)
            OnUserChangePassword.Invoke(succeed);
    }

    // 获取用户信息
    protected Action<bool, GizUserInfo> OnGetUserInfo = null;
    public override void DidGetUserInfo(GizWifiSDK wifiSDK, NSError result, GizUserInfo userInfo)
    {
        var succeed = IsSuccess(result);
        if (OnGetUserInfo != null)
            OnGetUserInfo.Invoke(succeed, userInfo);
    }

    // 绑定设备
    protected Action<bool, string> OnDeviceBind = null;
    public override void DidBindDevice(GizWifiSDK wifiSDK, NSError result, string did)
    {
        var succeed = IsSuccess(result);
        if (OnDeviceBind != null)
            OnDeviceBind(succeed, did);
    }

    // 解绑设备
    protected Action<bool, string> OnDeviceUnBind = null;
    public override void DidUnbindDevice(GizWifiSDK wifiSDK, NSError result, string did)
    {
        var succeed = IsSuccess(result);
        if (OnDeviceUnBind != null)
            OnDeviceUnBind(succeed, did);
    }

    protected Action<bool, IEnumerable<string>> OnGetSSIDList = null;
    public override void DidGetSSIDList(GizWifiSDK wifiSDK, NSError result, GizWifiSSID[] ssidList)
    {
        var succeed = IsSuccess(result);
        if (OnGetSSIDList != null)
            OnGetSSIDList(succeed, ssidList?.Select(s => s.Ssid));
    }

    // 设备配网
    protected Action<int, GizWifiDevice> OnDeviceOnboarding = null;
    public override void DidSetDeviceOnboarding(GizWifiSDK wifiSDK, NSError result, GizWifiDevice device)
    {
        var succeed = IsSuccess(result);
        if (OnDeviceOnboarding != null)
        {
            int r = succeed ? 1 : (Convert.ToInt64(result.Code) == (long)GizWifiErrorCode.GizSdkDeviceConfigIsRunning ? 0 : -1);
            OnDeviceOnboarding.Invoke(r, device);
        }
    }

    protected void Start(ProductInfo info)
    {
        NSDictionary<NSString, NSString> appInfo;
        List<NSDictionary<NSString, NSString>> productInfo = null;
        NSDictionary<NSString, NSString> cloudServiceInfo = null;

        appInfo = NSDictionary<NSString, NSString>.FromObjectsAndKeys(
            new object[] { new NSString(info.App_ID), new NSString(info.App_Secret) },
            new object[] { new NSString("appId"), new NSString("appSecret") });

        if (info.Product_Infos != null && info.Product_Infos.Count() != 0)
        {
            productInfo = new();
            foreach (var pi in info.Product_Infos)
            {
                var ks = new List<object>();
                ks.Add(new NSString("productKey"));
                ks.Add(new NSString("productSecret"));
                if (!String.IsNullOrEmpty(pi.Item3))
                    ks.Add(new NSString("usingAdapter"));

                var vs = new List<object>();
                vs.Add(new NSString(pi.Item1));
                vs.Add(new NSString(pi.Item2));
                if (!String.IsNullOrEmpty(pi.Item3))
                    vs.Add(new NSString(pi.Item3));

                productInfo.Add(NSDictionary<NSString, NSString>.FromObjectsAndKeys(vs.ToArray(), ks.ToArray()));
            }
        }

        GizWifiSDK.SharedInstance().Delegate = this;
        GizWifiSDK.StartWithAppInfo(appInfo, productInfo?.ToArray(), cloudServiceInfo, false);
    }

    protected void SetDeviceOnboardingDeploy(string ssid, string key, GizWifiConfigureMode mode, string prefix, int? timeout, IList<GizWifiGAgentType> types, bool bind)
    {
        List<NSNumber> ts = null;
        if (types != null && types.Count != 0)
        {
            ts = new();
            foreach (var type in types)
                ts.Add(new NSNumber((long)type));
        }

        GizWifiSDK.SharedInstance().SetDeviceOnboardingDeploy(ssid, key, mode, prefix, timeout ?? 30, ts?.ToArray(), bind);
    }

    protected Action OnTokenInvalid = null;
    internal bool IsSuccess(NSError result)
    {
        if (result != null)
        {
            long code = Convert.ToInt64(result.Code);
            var error = (GizWifiErrorCode)code;

            if (error == GizWifiErrorCode.GizSdkSuccess)
                return true;

            SendMessage(error.ToString());

            if (error == GizWifiErrorCode.GizSdkTokenInvalid || error == GizWifiErrorCode.GizOpenapiTokenInvalid || error == GizWifiErrorCode.GizOpenapiTokenExpired)
                OnTokenInvalid?.Invoke();
        }
        return false;
    }
}