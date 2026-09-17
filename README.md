# gizwits.net

**Gizwits SDK wrapper for .Net MAUI**

**Read this in other languages:**
[English](README.md) | [中文](README_zh.md)

Gizwits.Net is a .Net MAUI wrapper for the Gizwits ([https://dev.gizwits.com](https://dev.gizwits.com)) SDK.
With Gizwits.Net, you can write a single codebase in .Net MAUI to implement Gizwits client functionality—enabling communication with Gizwits devices—across both iOS and Android platforms, without having to worry about the differences in SDK calls between the two.

**Prerequisites**

To use the Gizwits.Net library, you must first register an account on the Gizwits Developer Platform, familiarize yourself with and adhere to the platform's App SDK development policies, and obtain the PRODUCT_KEY, PRODUCT_SECRET, APP_ID, and APP_SECRET required for your product.
As the Gizwits.Net library is designed for developers creating end-user apps, you may also need a Gizwits user account for the end devices.

For more information, please visit the [Gizwits Developer Center](https://dev.gizwits.com).

**Install**

You can download the source code to build the Gizwits.Net library.
However, a simpler method is to install the NuGet package ([https://www.nuget.org/packages/Gizwits.Net](https://www.nuget.org/packages/Gizwits.Net)).
Installing the Gizwits.Net NuGet package will also automatically install the following transitive packages:

* Gizwits.Net.Android
* Gizwits.Net.iOS

These two packages are .Net MAUI binding libraries for the Gizwits SDK and are required packages.

**Use**

The following steps provide a typical example of how to use the library.

1. First, a device class is needed; this class represents a terminal device, such as a Wi-Fi switch.

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

    // Implement the operations and logic for the Wi-Fi Switch.
    ......
}
```

2. A class implementing the IGizDeviceGenerator interface is required; this class is used to construct a device class.

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

3. Add the following code to MauiProgram.cs to register the Gizwits SDK.

```
var gizClient = GizClient.GetBuilder()
            .UsingAppId(APP_ID)
            .UsingAppSecret(APP_SECRET)
            .UsingProduct(PRODUCT_KEY, PRODUCT_SECRET, "GizAdapterWifiBle")
            .UsingDeviceGenerator(() => new DeviceGenerator())
            .Build();

gizClient.SdkMessageReceived += async (sender, e) =>
{
    // When an SDK message is received
    //await MessageShow.ToastAsync(e);
};

gizClient.InitialStartUp += async (sender, e) =>
{
    // When the SDK initializes
    // For example, you might want to restore user login information here.
    //var sdk = ((GizClient)sender).Handler;
    //var session = storage.GetGizUserSession();

    //if (session != null)
    //    sdk.UserSession(session.UID, session.Token);
    //else if (await sdk.UserLogin(null, null))   // Or log in anonymously.
    //    storage.SetGizUserSession(sdk.UID, sdk.Token);

    //if (sdk.IsLoggedIn)
    //    sdk.RequestDiscovered();
};

gizClient.UserTokenExpired += (sender, e) =>
{
    // When the token is invalid
    // You can have users log in again here to refresh an expired token.
};

// Dependency injection for singleton services.
builder.Services.AddSingleton(gizClient);
```

4. Subsequently, use the Gizwits SDK via the GizClient.Handler instance.

```
var giz = gizClient.Handler;

// Request SMS verification code
await giz.RequestSMSCode(phoneNumber);

// User Registration
var result = await giz.UserRegister(userName, password, verifyCode);
var uid     = result.Item1;
var token   = result.Item2;

// User Login
var succeed = await giz.UserLogin(userName, password);

// User Logout
await giz.UserLogout();

// User changes password
var succeed  = await giz.UserPasswordChange(oldPassword, password);

// User Password Reset
var succeed = await giz.UserPasswordRest(userName, newPassword, verifyCode);

// Log in user information
var dict = await giz.UserInfo();

// Configure the device's Wi-Fi connection via Bluetooth.
giz.DeviceOnboarding(
    (r, d) => 
    {
        var succeed = r == 1;
        var device = d as WifiSwitch;
    },          // Network provisioning completion callback
    ssid,       // Wifi SSID
    password,   // Wifi Password
    2,          // 1: GizWifiSoftAP, 2: GizWifiBleLink, 3: GizWifiAirLink
    blename,    // Ble Name XPG-GAgent-xxx
    timeout     // Timeout
);

// Stop network configuration
giz.StopDeviceOnboarding();

// Discover devices
giz.RequestDiscovered()；
var devices = giz.WifiDevices;

// Bind device
giz.BindRemoteDevice(mac, productKey, productSecret);

// Unbind device
giz.UnBindDevice(id);

// Get the device
var gizdev = giz.GetGizDevice(id,
    (data, faults, alerts, binary, sn) => {},   // 设备收到数据的回调
    (status) => {}  // 设备状态改变回调（离线，上线...）
    );

// Subscribe to Device
var succeed = await gizdev.SetSubscribe(subsribed, autoGetDeviceStatus);

// Unsubscribe device
await gizdev.SetSubscribe(false);

// Send control command
await gizdev.Control(command, state, sn);

// Send multiple control commands
var commands = new IDictionary<string, object>();
commands.Add(cmd1, state1);
commands.Add(cmd2, state2);
await gizdev.Control(commands, sn);

// Request to retrieve specified device attribute fields.
giz.RequestGetData(name1, name2, ...);

// Name the device, or add a remark
await giz.SetCustomInfo(name, remark);

// Retrieve device attribute information (firmware version, MCU version, Wi-Fi module version)
var info = await giz.GetDeviceInfo();
```

**Notes and Reminders**

Gizwits.Net This is merely a wrapper for the Gizwits SDK; for explanations regarding specific functions, methods, and parameter meanings, you still need to consult the official Gizwits SDK development documentation.
Gizwits.Net Based on the following SDK versions officially released by Gizwits:

* GizWifiSDK-Android-2.23.23.11613
* GizWifiSDK-iOS-2.23.23.01613_1170
