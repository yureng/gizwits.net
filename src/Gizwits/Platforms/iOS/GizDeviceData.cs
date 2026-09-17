using System;
using System.Collections.Generic;
using Foundation;

namespace Gizwits;

internal partial class GizDeviceData
{
    private partial object GetValue(string name, Type type)
    {
        object obj = null;
        var dict = _source as NSDictionary;
        if (dict != null)
        {
            var ns = dict[name] as NSNumber;
            if (ns != null)
            {
                if (type == typeof(short))
                    obj = ns.Int16Value;
                else if (type == typeof(int))
                    obj = ns.Int32Value;
                else if (type == typeof(long))
                    obj = ns.Int64Value;

                else if (type == typeof(bool))
                    obj = ns.BoolValue;
                else if (type == typeof(double))
                    obj = ns.DoubleValue;
                else if (type == typeof(float))
                    obj = ns.FloatValue;
                else if (type == typeof(string))
                    obj = ns.StringValue;
            }
        }
        return obj;
    }
}
