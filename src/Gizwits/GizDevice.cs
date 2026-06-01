using Com.Gizwits.Gizwifisdk.Api;
using System.Collections;
using System.Threading.Tasks;

namespace Gizwits;

public class GizDevice : GizDeviceListener, IDisposable
{
    private readonly GizWifiDevice _gizDevice = null;        

    internal GizDevice(GizWifiDevice gizDevice, GizSDK sdk,
        Action<IWifiDeviceData, IWifiDeviceData, IWifiDeviceData, byte[], int> dataCallback,
        Action<DevStatus> statusCallback) : base(sdk) 
    {
        this.OnReceiveData = dataCallback;
        this.OnUpdateNetStatus = statusCallback;

        _gizDevice = gizDevice;

#if ANDROID
        _gizDevice.Listener = this;
#elif IOS
        _gizDevice.Delegate = this;
#endif
    }

    public bool IsSubscribed => (_gizDevice != null && _gizDevice.IsSubscribed);

    // 订阅设备状态
    public Task<bool> SetSubscribe(bool subsribed, bool autoGetDeviceStatus = false)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnSetSubscribe = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

        _gizDevice.SetSubscribe(subsribed, autoGetDeviceStatus);
        return cs.Task;
    }

    // 设置定制属性
    public Task<bool> SetCustomInfo(string name, string remark)
    {
        var cs = new TaskCompletionSource<bool>();
        this.OnSetCustomInfo = (succeed) => { if (cs.Task.Status != TaskStatus.RanToCompletion) cs.SetResult(succeed); };

        _gizDevice.SetCustomInfo(remark, name);
        return cs.Task;
    }

    // 发送控制命令
    //int controlSn = -1;
    TaskCompletionSource controlTaskSource = null;        
    public Task Control(IDictionary<string, object> command, int sn)
    {
        if (controlTaskSource != null && controlTaskSource.Task.Status != TaskStatus.RanToCompletion)
        {
            controlTaskSource.SetResult();
        }

        //controlSn = sn;
        controlTaskSource = new TaskCompletionSource();            

        this.OnReceiveData += OnControl;
        Write(_gizDevice, command, sn);

        return controlTaskSource.Task;
    }

    void OnControl(IWifiDeviceData d, IWifiDeviceData f, IWifiDeviceData a, byte[] bs, int sn)
    {
        if (controlTaskSource == null)
        {
            this.OnReceiveData -= OnControl;
            return;
        }

        //if (sn == controlSn && controlTaskSource != null)
        if (controlTaskSource != null)
        {
            this.OnReceiveData -= OnControl;
            if (controlTaskSource.Task.Status != TaskStatus.RanToCompletion)
                controlTaskSource.SetResult();
        }
    }

    // 发送控制命令
    public Task Control(string command, object state, int sn)
    {
        var cmd = new Dictionary<string, object>();
        cmd.Add(command, state);

        return Control(cmd, sn);
    }

    // 请求检索数据
    public void RequestGetData(params string[] attrs)
    {
        _gizDevice.GetDeviceStatus(attrs);
    }

    // 获取硬件信息
    WifiDeviceInfo _infoCache = new ();
    public Task<WifiDeviceInfo> GetDeviceInfo()
    {
        if (_infoCache.FirmwareId != null && _infoCache.FirmwareVersion != null)
            return Task.FromResult(_infoCache);

        var cs = new TaskCompletionSource<WifiDeviceInfo>();
        this.OnGetHardwareInfo = (succeed, info) =>
        {
            if (cs.Task.Status != TaskStatus.RanToCompletion)
            {
                if (succeed)
                    _infoCache = info;

                cs.SetResult(_infoCache);
            }
        };

        _gizDevice.GetHardwareInfo();
        return cs.Task;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (_gizDevice != null)
            {
#if ANDROID
                _gizDevice.Listener = null;
#elif IOS
                _gizDevice.Delegate = null;
#endif
                _gizDevice.SetSubscribe(false, false);                    
                _gizDevice.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}