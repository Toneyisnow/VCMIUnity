using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilerLogger : MonoBehaviour
{
    private static DateTime lastTime = DateTime.MinValue;

    public static void RecordProfile(string message)
    {
        if (lastTime == DateTime.MinValue)
        {
            lastTime = DateTime.Now;
        }

        TimeSpan span = (DateTime.Now - lastTime);
        double memoryUsage = System.GC.GetTotalMemory(false) / 1024 / 1024;

        print(string.Format(@"[{0}][{1:0.00}]{2}", span.ToString(@"mm\:ss\.fff"), memoryUsage, message));
        lastTime = DateTime.Now;

    }
}
