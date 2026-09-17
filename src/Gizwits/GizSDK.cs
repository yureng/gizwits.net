using Com.Gizwits.Gizwifisdk.Api;
using Com.Gizwits.Gizwifisdk.Enumration;

namespace Gizwits;

public class GizSDK : GizSDKListener
{
    internal readonly ProductInfo ProductInfo = null;
    readonly Func<IGizDeviceGenerator> _deviceGenerator = null;
    Action<string, string> _messageNotify = null;
    Action<bool> _startDone = null;
    
    public string UID { get; private set; }
    public string Token { get; private set; }
    public bool IsLoggedIn => (!String.IsNullOrEmpty(UID) && !String.IsNullOrEmpty(Token));
    
    public IEnumerable<object> WifiDevices => ConvertTo(GizWifiSDK.SharedInstance().DeviceList);
    public IEnumerable<object> BleDevices => ConvertTo(GizWifiSDK.SharedInstance().BoundBleDevice);

    internal bool Initialising { get => _startDone != null; }
    public bool Initialized { get; private set; } = false;

    public static string Version
    {
        get 
        {
            string ver = null;
#if ANDROID
            ver = GizWifiSDK.SharedInstance()?.Version;
#elif IOS
            ver = GizWifiSDK.Version;
#endif
            return ver;
        }
    }

    internal GizSDK(ProductInfo productInfo, Func<IGizDeviceGenerator> deviceGenerator)
    {
        ProductInfo = productInfo;
        _deviceGenerator = deviceGenerator;
    }

    // 初始化启动 SDK *(需在主线程调用此方法)
    public void InitializeStart(
        Action<string, string> msgCallback,
        Action<bool> discoveCallback,
        Action<bool> bleFoundCallback,
        Action tokenInvalidCallback,
        Action startedCallback = null)
    {
        _messageNotify = msgCallback;
        this.OnTokenInvalid = tokenInvalidCallback;
        this.OnDiscovered = discoveCallback;
        this.OnDiscoverBleDevice = bleFoundCallback;

        if (!Initialized && _startDone == null)
        {
            _startDone = (succeed) => 
            {
                Initialized = succeed;
                startedCallback?.Invoke();
            };

            this.OnNotifyEvent += OnNotify;
            this.Start(ProductInfo);
        }
    }

    void OnNotify(GizEventType eventType, GizWifiErrorCode eventId)
    {
        if (eventType == GizEventType.GizEventSDK)
        {
            this.OnNotifyEvent -= OnNotify;

            _startDone?.Invoke(eventId == GizWifiErrorCode.GizSdkStartSuccess);
            _startDone = null;
        }
    }

    // 设置日志级别
    public void SetLogLevel(int level = 0)
    {
        var gizLevel = level switch
        {
            3 => GizLogPrintLevel.GizLogPrintAll,
            2 => GizLogPrintLevel.GizLogPrintII,
            1 => GizLogPrintLevel.GizLogPrintI,
            _ => GizLogPrintLevel.GizLogPrintNone
        };

#if ANDROID
        GizWifiSDK.SharedInstance().SetLogLevel(gizLevel);
#elif IOS
        GizWifiSDK.SetLogLevel(gizLevel);
#endif
    }

    // 获取设备实例
    public GizDevice GetGizDevice(string did,
        Action<IGizDeviceData, IGizDeviceData, IGizDeviceData, byte[], int> dataCallback,
        Action<DevStatus> statusCallback)
    {
        var d = GizWifiSDK.SharedInstance()?.DeviceList?.FirstOrDefault(dev => { return (dev.Did == did); });
        return d != null ? new GizDevice(d, this, dataCallback, statusCallback) : null;
    }

    // 请求检索设备
    public void RequestDiscovered()
    {
        if (UID != null && Token != null)
            GizWifiSDK.SharedInstance().GetBoundDevices(UID, Token);
    }

    // 短信验证码
    public Task<bool> RequestSMSCode(string phoneNumber)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnRequestSendPhoneSMSCode = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

        GizWifiSDK.SharedInstance().RequestSendPhoneSMSCode(ProductInfo.App_Secret, phoneNumber);
        return cs.Task;
    }

    // 用户注册
    public Task<Tuple<string, string>> UserRegister(string userName, string password, string verifyCode)
    {
        var cs = new TaskCompletionSource<Tuple<string, string>>();
        this.OnUserRegister = (succeed, uid, token) =>
        {
            if (cs.Task.Status != TaskStatus.RanToCompletion)
            {
                if (succeed)
                    cs.SetResult(new Tuple<string, string>(uid, token));
                else
                    cs.SetResult(new Tuple<string, string>(null, null));
            }
        };

        GizWifiSDK.SharedInstance().RegisterUser(userName, password, verifyCode, GizUserAccountType.GizUserPhone);
        return cs.Task;
    }

    // 用户登录
    public Task<bool> UserLogin(string userName, string password)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnUserLogin = (succeed, uid, token) =>
        {
            if (succeed)
                UserSession(uid, token);
            else
                UserSession(null, null);

            if (cs.Task.Status != TaskStatus.RanToCompletion)
                cs.SetResult(succeed);
        };

        if (String.IsNullOrWhiteSpace(userName))
            GizWifiSDK.SharedInstance().UserLoginAnonymous();
        else
            GizWifiSDK.SharedInstance().UserLogin(userName, password);

        return cs.Task;
    }

    public void UserSession(string uid, string token)
    {
        UID = uid;
        Token = token;
    }

    // 用户登出
    public Task<bool> UserLogout()
    {
        if (UID != null)
        {
            var cs = new TaskCompletionSource<bool>();
            this.OnUserLogout = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

            GizWifiSDK.SharedInstance().UserLogout(UID);
            return cs.Task;
        }
        return Task.FromResult(true);
    }

    // 用户更改密码
    public Task<bool> UserPasswordChange(string oldPassword, string password)
    {
        if (Token != null)
        {
            var cs = new TaskCompletionSource<bool>();
            this.OnUserChangePassword = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

            GizWifiSDK.SharedInstance().ChangeUserPassword(Token, oldPassword, password);
            return cs.Task;
        }
        return Task.FromResult(false);
    }

    // 用户重置密码
    public Task<bool> UserPasswordRest(string userName, string password, string verifyCode)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnUserChangePassword = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

        GizWifiSDK.SharedInstance().ResetPassword(userName, verifyCode, password, GizUserAccountType.GizUserPhone);
        return cs.Task;
    }

    // 用户信息获取
    public Task<IDictionary<string, object>> UserInfo()
    {
        if (Token != null)
        {
            var cs = new TaskCompletionSource<IDictionary<string, object>>();
            this.OnGetUserInfo = (succeed, info) =>
            {
                if (cs.Task.Status != TaskStatus.RanToCompletion)
                {
                    if (succeed)
                    {
                        var dict = new Dictionary<string, object>();
                        dict.Add(nameof(info.Uid), info.Uid);
                        dict.Add(nameof(info.Username), info.Username);
                        dict.Add(nameof(info.IsAnonymous), info.IsAnonymous);
                        dict.Add(nameof(info.Name), info.Name);
                        dict.Add(nameof(info.Address), info.Address);
                        dict.Add(nameof(info.Email), info.Email);
                        dict.Add(nameof(info.Phone), info.Phone);
                        dict.Add(nameof(info.Birthday), info.Birthday);
                        dict.Add(nameof(info.DeviceBindTime), info.DeviceBindTime);
                        dict.Add(nameof(info.Remark), info.Remark);
                        dict.Add("Gender", null);

                        cs.SetResult(dict);
                    }
                    else
                        cs.SetResult(null);
                }
            };

            GizWifiSDK.SharedInstance().GetUserInfo(Token);
            return cs.Task;
        }
        return Task.FromResult(default(IDictionary<string, object>));
    }

    // 绑定远程设备
    public Task<string> BindRemoteDevice(string mac, string productKey, string productSecret)
    {
        var cs = new TaskCompletionSource<string>();
        this.OnDeviceBind = (succeed, did) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed ? did : null); };

        GizWifiSDK.SharedInstance().BindRemoteDevice(UID, Token, mac, productKey, productSecret, false);
        return cs.Task;
    }

    // 解绑设备
    public Task<bool> UnBindDevice(string did)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnDeviceUnBind = (succeed, _) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

        GizWifiSDK.SharedInstance().UnbindDevice(UID, Token, did);
        return cs.Task;
    }

    // SoftAP SSIDs
    public Task<IEnumerable<string>> GetSoftApSSIDs()
    {
        var cs = new TaskCompletionSource<IEnumerable<string>>();
        this.OnGetSSIDList = (succedd, ssids) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succedd ? ssids : Enumerable.Empty<string>()); };

        GizWifiSDK.SharedInstance().GetSSIDList();
        return cs.Task;
    }

    // 设备配网
    public void DeviceOnboarding(Action<int, object> callback, 
        string ssid, string key,
        int configureMode = 0, string bleName = null, int? timeout = null, bool? bind = null)
    {
        this.OnDeviceOnboarding = (state, d) =>
        {
            if (callback != null)
                callback.Invoke(state, ConvertTo(d));
        };

        GizWifiConfigureMode m;
        string p;
        IList<GizWifiGAgentType> t;

        if (configureMode == 1)
        {
            m = GizWifiConfigureMode.GizWifiSoftAP;
            p = bleName?? ProductInfo.SoftAp_Prefix;
            t = null;
        }
        else if (configureMode == 2)
        {
            m = GizWifiConfigureMode.GizWifiBleLink;
            p = bleName;
            t = null;
        }
        else
        {
            m = GizWifiConfigureMode.GizWifiAirLink;
            p = null;
            t = new List<GizWifiGAgentType>();
            t.Add(GizWifiGAgentType.GizGAgentESP);  // 乐鑫 ESP
            //t.Add(GizWifiGAgentType.GizGAgentHF);   // 汉枫 HF
        }

        this.SetDeviceOnboardingDeploy(ssid, key, m, p, timeout, t, bind ?? this.IsLoggedIn);
    }

    public void StopDeviceOnboarding()
    {
        //var cs = new TaskCompletionSource();
        //this.OnDeviceOnboarding = (state, _) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(); };

        GizWifiSDK.SharedInstance().StopDeviceOnboarding();
        //return cs.Task;
    }

    protected override void SendMessage(string msg, string mtype = null)
    {
        if (!String.IsNullOrEmpty(msg))
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"GizSDK Message: {msg}", mtype);
#else

            if (msg.Contains("SUCCES", StringComparison.InvariantCultureIgnoreCase) ||
                msg.Contains("REQUEST_TIMEOUT", StringComparison.InvariantCultureIgnoreCase) ||
                msg.Contains("RequestTimeout", StringComparison.InvariantCultureIgnoreCase) ||
                msg.Contains("DEVICE_NOT_READY", StringComparison.InvariantCultureIgnoreCase) ||
                msg.Contains("DeviceNotReady", StringComparison.InvariantCultureIgnoreCase))
                
                return;
#endif
            _messageNotify?.Invoke(msg, mtype);
        }
    }

    IEnumerable<object> ConvertTo(IEnumerable<GizWifiDevice> devices)
    {
        if (devices != null)
        {
            foreach (var d in devices)
                yield return ConvertTo(d);
        }
    }

    object ConvertTo(GizWifiDevice d)
    {
        if (d == null)
            return null;
        else
        {
            string did = d.Did;
            string name = d.Alias;
            bool? bleWork = null;
           
            if (d is GizWifiBleDevice)
            {
                var ble = ((GizWifiBleDevice)d);
                bleWork = ble.BleWorkStatus == GizBleWorkStatusType.Normal;

                if (did == null)
                    did = ble.MacAddress;

                if (!String.IsNullOrEmpty(ble.BleAlias))
                    name = ble.BleAlias;
            }

            if (String.IsNullOrEmpty(name))
                name = d.MacAddress?.ToUpper(); //did?.ToUpper();

            return _deviceGenerator().Create(
                d.ProductKey,
                did,
                name,
                d.MacAddress,
                d.IPAddress,
                d.Remark,
                d.IsBind,
                d.IsLAN,
                d.IsDisabled,
                bleWork,
                ConvertTo(d.NetStatus),
                (d.ProductType == GizWifiDeviceType.GizDeviceCenterControl ? DevType.Central : DevType.Normal),
                (d.SharingRole == GizDeviceSharingUserRole.GizDeviceSharingOwner ? Roles.Owner :
                (d.SharingRole == GizDeviceSharingUserRole.GizDeviceSharingNormal ? Roles.Normal :
                (d.SharingRole == GizDeviceSharingUserRole.GizDeviceSharingGuest ? Roles.Guest : Roles.Special))));
        }
    }

    internal static DevStatus ConvertTo(GizWifiDeviceNetStatus gizNetStatus)
    {
        if (gizNetStatus == GizWifiDeviceNetStatus.GizDeviceControlled)
            return DevStatus.Available;
        else if (gizNetStatus == GizWifiDeviceNetStatus.GizDeviceOnline || gizNetStatus == GizWifiDeviceNetStatus.GizDeviceUnavailable)
            return DevStatus.Online;
        else
            return DevStatus.Offline;
    }
}