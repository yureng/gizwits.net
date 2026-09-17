using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gizwits
{
    public interface IGizDeviceData
    {
        bool TryGet<T>(string name, out T value);
    }

    partial class GizDeviceData : IGizDeviceData
    {
        readonly object _source;

        public GizDeviceData(object source)
        {
            _source = source;
        }

        bool IGizDeviceData.TryGet<T>(string name, out T value)
        {
            if (!String.IsNullOrEmpty(name))
            {
                var obj = GetValue(name, typeof(T));
                if (obj != null)
                {
                    value = (T)obj;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        private partial object GetValue(string name, Type type);
    }
}
