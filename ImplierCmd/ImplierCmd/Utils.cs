using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace ImplierCmd
{
    internal class Utils
    {
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
    }
}