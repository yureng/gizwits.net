using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Gizwits;

public class GizClient
{
    readonly GizSDK _sdk;

    internal GizClient(GizSDK sdk)
    {
        _sdk = sdk;
    }

    public event EventHandler<string> SdkMessageReceived;
    public event EventHandler<IEnumerable<object>> DeviceDiscoved;
    public event EventHandler<IEnumerable<object>> DeviceBleFound;
    public event EventHandler UserTokenExpired;
    public event EventHandler InitialStartUp;

    public string SdkVersion { get => GizSDK.Version; }
    public ProductInfo ProductInfo { get => _sdk?.ProductInfo; }

    public GizSDK Handler
    {
        get
        {
            this.SdkStart();
            return _sdk;
        }
    }

    public async void SdkStart()
    {
        if (_sdk == null || _sdk.Initialized || _sdk.Initialising)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_sdk.Initialized && !_sdk.Initialising)
            {
                _sdk.InitializeStart(
                    // sdk message
                    (msg, mtype) =>
                    {
                        if (!String.IsNullOrWhiteSpace(msg))
                            SdkMessageReceived?.Invoke(this, msg);
                    },
                    // Wi-Fi device discovery
                    (succeed) =>
                    {
                        if (succeed && DeviceDiscoved != null)
                            DeviceDiscoved.Invoke(this, _sdk.WifiDevices);
                    },
                    // BLE device discovery
                    (succeed) =>
                    {
                        if (succeed && DeviceBleFound != null)
                            DeviceBleFound.Invoke(this, _sdk.BleDevices);
                    },
                    // When the user session is invalid
                    () => 
                    { 
                        UserTokenExpired?.Invoke(this, EventArgs.Empty);
                    },
                    // When sdk starts
                    () => 
                    { 
                        InitialStartUp?.Invoke(this, EventArgs.Empty);
                    }
                );
            }
        });
    }

    public static GizClientBuilder GetBuilder()
    {
        return new GizClientBuilder();
    }
}

public class GizClientBuilder
{
    string _appId, _appSecret, _softApPrefix, _bleNamePrefix;
    List<Tuple<string, string, string>> _products;

    Func<IGizDeviceGenerator> _generator;

    internal GizClientBuilder() { }

    public GizClientBuilder UsingAppId(string id)
    {
        _appId = id;
        return this;
    }

    public GizClientBuilder UsingAppSecret(string secret)
    {
        _appSecret = secret;
        return this;
    }

    public GizClientBuilder UsingProduct(string productKey, string productSecret, string usingAdapter = null)
    {
        if (_products == null)
            _products = new List<Tuple<string, string, string>>();

        _products.Add(new Tuple<string, string, string>(productKey, productSecret, usingAdapter));
        return this;
    }

    public GizClientBuilder UsingSoftApSsidPrefix(string prefix)
    {
        _softApPrefix = prefix;
        return this;
    }

    public GizClientBuilder UsingBleNamePrefix(string prefix)
    {
        _bleNamePrefix = prefix;
        return this;
    }

    public GizClientBuilder UsingDeviceGenerator(Func<IGizDeviceGenerator> generator)
    {
        _generator = generator;
        return this;
    }

    public GizClient Build()
    {
        if (_appId == null)
            throw new InvalidOperationException("Please use the AppId first.");

        if (_appSecret == null)
            throw new InvalidOperationException("Please use the AppSecret first.");

        if (_products == null || _products.Count == 0)
            throw new InvalidOperationException("Please use the Product first.");

        if (_generator == null)
            throw new InvalidOperationException("Please use the DeviceGenerator first.");

        return new GizClient(new GizSDK(new ProductInfo(_appId, _appSecret, _products, _softApPrefix, _bleNamePrefix), _generator));
    }
}
