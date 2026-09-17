# gizwits.net

**Gizwits SDK wrapper for .Net MAUI**

**Read this in other languages:**

[English](README.md) | [中文](README_zh.md)

Gizwits.Net 是机智云([https://dev.gizwits.com](https://dev.gizwits.com)) SDK 的 .Net MAUI 封装。
通过 Gizwits.Net ，能够写单一的 .Net MAUI 代码，实现 iOS 和 Android 平台的机智云客户端功能，与机智云设备通讯，而不必纠结于两个平台的 SDK 调用差异。

**前提条件**

要使用 Gizwits.Net 库，首先必需在机智云开发者平台注册账户，了解并遵循平台 App SDK 开发政策，获得授权您产品使用的 PRODUCT_KEY、PRODUCT_SECRET，以及 APP_ID、APP_SECRET；
Gizwits.Net 库面向开发终端用户 App 的开发者，因此您也可能需要拥有终端设备的机智云用户账号。

更多信息请访问 [Gizwits 机智云开发者中心](https://dev.gizwits.com)

**安装**

可以下载源码构建 Gizwits.Net 库。
但是更简单的方式，是安装 NuGet 包（[https://www.nuget.org/packages/Gizwits.Net](https://www.nuget.org/packages/Gizwits.Net)),
安装 Gizwits.Net 的 NuGet 包，也会自动安装以下可传递包：

* Gizwits.Net.Android
* Gizwits.Net.iOS

这两个包是机智云 SDK 的 .Net MAUI 绑定库，是必需的包。

**使用**

以下步骤，是如何使用该库的典型示例

1. 首先需要设备类，这个类用来代表终端设备，例如一个 Wifi 开关

```
public class WifiSwitch
{
    readonly IErrorHandler errorHandler;

    public WifiSwitch(string id, IErrorHandler errorHandler)
    {
        this.Id = id;
        this.errorHandler = errorHandler;
        ......
    }

    public string Id { get; set; }
    public bool IsOn { get; set; }

    // 实现 Wifi 开关的操作和逻辑
    ......
}
```

2. 需要一个实现 IGizDeviceGenerator 接口的类，这个类用于构造一个设备类

```
class DeviceGenerator : IGizDeviceGenerator
{
    public object Create(string productKey, string id, string name, string mac, string ip, string remark, bool isBind, bool isLan, bool isDisabled, bool? bleWork, DevStatus status, DevType type, Roles role)
    {
        return productKey switch
        {
            _ => new WifiSwitch(id, errorHandler)
        };
    }
}
```

3. 在 MauiProgram.cs 添加以下代码，注册机智云 SDK

```
var gizClient = GizClient.GetBuilder()
            .UsingAppId(APP_ID)
            .UsingAppSecret(APP_SECRET)
            .UsingProduct(PRODUCT_KEY, PRODUCT_SECRET, "GizAdapterWifiBle")
            .UsingDeviceGenerator(() => new DeviceGenerator())
            .Build();

gizClient.SdkMessageReceived += async (sender, e) =>
{
    // 当收到 SDK 消息
    //await MessageShow.ToastAsync(e);
};

gizClient.InitialStartUp += async (sender, e) =>
{
    // 当 SDK 初始化
    // 例如：你可能想在这里恢复用户登录信息
    //var sdk = ((GizClient)sender).Handler;
    //var session = storage.GetGizUserSession();

    //if (session != null)
    //    sdk.UserSession(session.UID, session.Token);
    //else if (await sdk.UserLogin(null, null))   // 或者匿名登录
    //    storage.SetGizUserSession(sdk.UID, sdk.Token);

    //if (sdk.IsLoggedIn)
    //    sdk.RequestDiscovered();
};

gizClient.UserTokenExpired += (sender, e) =>
{
    // 当 Token 无效
    // 你可以在这里让用户重新执行登录以更新到期的 Token
};

// 必需依赖注入为单例服务
builder.Services.AddSingleton(gizClient);
```

4. 后续通过 GizClient.Handler 实例来使用机智云 SDK

```
var giz = gizClient.Handler;

// 请求短信验证码
await giz.RequestSMSCode(phoneNumber);

// 用户注册
var result = await giz.UserRegister(userName, password, verifyCode);
var uid     = result.Item1;
var token   = result.Item2;

// 用户登录
var succeed = await giz.UserLogin(userName, password);

// 用户登出
await giz.UserLogout();

// 用户更改密码
var succeed  = await giz.UserPasswordChange(oldPassword, password);

// 用户重置密码
var succeed = await giz.UserPasswordRest(userName, newPassword, verifyCode);

// 登录用户信息
var dict = await giz.UserInfo();

// 用蓝牙方式为设备配置 WIfi 连接
giz.DeviceOnboarding(
    (r, d) => 
    {
        var succeed = r == 1;
        var device = d as WifiSwitch;
    },          // 配网结束回调
    ssid,       // Wifi SSID
    password,   // Wifi Password
    2,          // 1: GizWifiSoftAP, 2: GizWifiBleLink, 3: GizWifiAirLink
    blename,    // 蓝牙名称 XPG-GAgent-xxx
    timeout     // 超时
);

// 停止配网
giz.StopDeviceOnboarding();

// 发现设备
giz.RequestDiscovered()；
var devices = giz.WifiDevices;

// 绑定设备
giz.BindRemoteDevice(mac, productKey, productSecret);

// 解绑设备
giz.UnBindDevice(id);

// 获取指定的设备
var gizdev = giz.GetGizDevice(id,
    (data, faults, alerts, binary, sn) => {},   // 设备收到数据的回调
    (status) => {}  // 设备状态改变回调（离线，上线...）
    );

// 订阅设备
var succeed = await gizdev.SetSubscribe(subsribed, autoGetDeviceStatus);

// 取消订阅设备
await gizdev.SetSubscribe(false);

// 发送控制命令
await gizdev.Control(command, state, sn);

// 发送多个控制命令
var commands = new IDictionary<string, object>();
commands.Add(cmd1, state1);
commands.Add(cmd2, state2);
await gizdev.Control(commands, sn);

// 请求检索指定的设备属性字段
giz.RequestGetData(name1, name2, ...);

// 命名设备，备注
await giz.SetCustomInfo(name, remark);

// 获取设备属性信息（固件版本，MCU 版本，Wifi 模块版本）
var info = await giz.GetDeviceInfo();
```

**备注和提醒**

Gizwits.Net 只是包装机智云 SDK，具体函数方法和其中的参数含义，仍然需要参考机智云官方 SDK 开发文档获得解释

Gizwits.Net 基于机智云官方发布的以下 SDK 版本：

* GizWifiSDK-Android-2.23.23.11613
* GizWifiSDK-iOS-2.23.23.01613_1170
