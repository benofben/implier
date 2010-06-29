using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Implier
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class IsCloneInheritable : Attribute
    {
        private bool IsInheritable = false;
        public IsCloneInheritable()
        {
            IsInheritable = true;
        }
    }

    internal class Utils
    {
        internal static void CopyPropertiesAndFields<T>(T source, T clone) 
            where T: class
        {
            foreach (PropertyInfo propertyInfo in from propertyInfo in typeof (T).GetProperties()
                                                  let attributes = propertyInfo.GetCustomAttributes(false)
                                                  from attribute in attributes.OfType<IsCloneInheritable>()
                                                  select propertyInfo)
            {
                propertyInfo.SetValue(clone, propertyInfo.GetValue(source, null), null);
            }

            foreach (FieldInfo fieldInfo in from fieldInfo in typeof (T).GetFields()
                                            let attributes = fieldInfo.GetCustomAttributes(false)
                                            from attribute in attributes.OfType<IsCloneInheritable>()
                                            select fieldInfo)
            {
                fieldInfo.SetValue(clone, fieldInfo.GetValue(source));
            }
        }

        public static class Log
        {
            static string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "ImplierLog.txt";
            public static void Write(string message)
            {
                using (TextWriter tw = File.AppendText(path))
                {
                    tw.WriteLine(message);
                }
            }
        }

        #region Adjust System Timer
        internal struct SystemTime
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        };

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        internal extern static bool Win32SetSystemTime(ref SystemTime sysTime);

        public static void SetSystemTime(DateTime dateTime)
        {
            // Set system date and time
            SystemTime updatedTime = new SystemTime();
            updatedTime.Year = (ushort)dateTime.Year;
            updatedTime.Month = (ushort)dateTime.Month;
            updatedTime.Day = (ushort)dateTime.Day;
            // UTC time; it will be modified according to the regional settings of the target computer so the actual hour might differ
            updatedTime.Hour = (ushort)dateTime.Hour;
            updatedTime.Minute = (ushort)dateTime.Minute;
            updatedTime.Second = (ushort)dateTime.Second;
            // Call the unmanaged function that sets the new date and time instantly
            Win32SetSystemTime(ref updatedTime);
        }

        public static void AdjustTime(DateTime serverUTCDateTime)
        {
            DateTime localDateTime = DateTime.Now.ToUniversalTime();
            TimeSpan timeSpan = serverUTCDateTime - localDateTime;
            TimeSpan allowableSpan = new TimeSpan(0, 0, 0, 0, 300);

            if (timeSpan.CompareTo(allowableSpan) > 0)
            {
                SetSystemTime(serverUTCDateTime);
            }
        }
        #endregion

        static public int CompareStrings(string x, string y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int retval = x.Length.CompareTo(y.Length);

                    return retval != 0 ? retval : x.CompareTo(y);
                }
            }
        } 

        static internal TValue GetSafeValueByKey<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue result;
            if (!dict.TryGetValue(key, out result))
            {
                result = new TValue();
                dict[key] = result;
            }
            return result;
        }

    }

    internal class SafeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TValue : new()
    {
        #region Properties
        internal new TValue this[TKey key]
        {
            get { return Utils.GetSafeValueByKey(this, key); }
        }
        #endregion
    }

    internal class DualKeySafeDictionary<TKey1, TKey2, TValue>: SafeDictionary<TKey1, SafeDictionary<TKey2, TValue>>
        where TValue:new()
    {
        
    }

    internal class DualKeyDictionary<TKey1, TKey2, TValue>
        where TValue : class
        where TKey1 : IComparable
        where TKey2 : IComparable
    {
        #region Fields
        Dictionary<TKey1, Dictionary<TKey2, TValue>> entries = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
        #endregion

        #region Properties
        public IEnumerable<TValue> Values
        {
            get
            {
                List<TValue> result = new List<TValue>();
                foreach (Dictionary<TKey2, TValue> dictionary in entries.Values)
                {
                    result.AddRange(dictionary.Values);
                }
                return result;
            }
        }
        #endregion

        #region Methods
        public void SetValue(TKey1 key1, TKey2 key2, TValue value)
        {
            Dictionary<TKey2, TValue> dict;

            if (!entries.TryGetValue(key1, out dict))
            {
                dict = new Dictionary<TKey2, TValue>();
                entries.Add(key1, dict);
            }

            if (dict.ContainsKey(key2))
                dict[key2] = value;
            else
                dict.Add(key2, value);
        }

        public TValue GetValue(TKey1 key1, TKey2 key2)
        {
            TValue value;
            Dictionary<TKey2, TValue> dict;

            return entries.TryGetValue(key1, out dict) && dict.TryGetValue(key2, out value) ? value : null;
        }

        public void Remove(TKey1 key1, TKey2 key2)
        {
            Dictionary<TKey2, TValue> dict;
            if (entries.TryGetValue(key1, out dict))
            {
                dict.Remove(key2);
            }
        }

        public void Clear()
        {
            entries.Clear();
        }
        #endregion
    }
}
