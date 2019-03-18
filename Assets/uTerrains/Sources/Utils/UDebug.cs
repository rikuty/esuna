using System;
using System.IO;
using UnityEngine;

namespace UltimateTerrains
{
    public static class UDebug
    {
        private static string Prefix {
            get { return "[uTerrains] "; }
        }

        public static void Log(object message)
        {
            Debug.Log(Prefix + message);
        }

        public static void Log(string message)
        {
            Debug.Log(Prefix + message);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(Prefix + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError(Prefix + message);
        }

        public static void LogException(Exception e)
        {
            Debug.LogException(e);
        }

        public static void Fatal(string message)
        {
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            LogException(new UltimateTerrainException(message));
            if (Application.isPlaying)
                Debug.Break();
        }

        public static void WriteToFile(string message)
        {
            Directory.CreateDirectory("ULogs");
            using (var w = File.AppendText("ULogs/logs.txt")) {
                w.WriteLine(message + "\n");
            }
        }
    }
}