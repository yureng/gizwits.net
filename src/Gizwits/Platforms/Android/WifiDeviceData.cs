using System;
using System.Collections.Generic;
using Java.Util.Concurrent;

namespace Gizwits;

internal partial class WifiDeviceData
{
    private partial object GetValue(string name, Type type)
    {
        object obj = null;
        var dict = _source as ConcurrentHashMap;
        if (dict != null)
        {
            var value = dict.Get(name);
            if (obj == null)
            {
                if (type == typeof(short))
                    obj = Convert.ToInt16(value);
                else if (type == typeof(int))
                    obj = Convert.ToInt32(value);
                else if (type == typeof(long))
                    obj = Convert.ToInt64(value);

                else if (type == typeof(bool))
                    obj = Convert.ToBoolean(value);
                else if (type == typeof(double))
                    obj = Convert.ToDouble(value);
                else if (type == typeof(float))
                    obj = Convert.ToSingle(value);
                else if (type == typeof(string))
                    obj = Convert.ToString(value);
            }
        }
        return obj;
    }
}
