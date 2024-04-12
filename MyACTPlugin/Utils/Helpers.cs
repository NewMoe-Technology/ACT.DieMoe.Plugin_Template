using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace MyACTPlugin.Utils
{
    internal static class Helpers
    {
        /// <summary>
        /// 插件自身
        /// </summary>
        internal static PluginMain _plugin;
        internal static PluginPage _configPage;
        internal static TabPage _tabPage;
        /// <summary>
        /// 解析插件实例
        /// </summary>
        internal static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin;

        /// <summary>
        /// 添加ACT日志行。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void AddLogLine(string type, string message)
        {
            var logline = $"00|{DateTime.Now:O}|0|{type}:{message}|";
            ActGlobals.oFormActMain.ParseRawLogLine(isImport: false, DateTime.Now, logline ?? "");
        }

        /// <summary>
        /// 获取游戏客户端编译日期版本号。
        /// </summary>
        public static string GetGameVersion(Process process)
        {
            var gamePath = Path.GetDirectoryName(process.MainModule.FileName);
            gamePath = Path.Combine(gamePath, "ffxivgame.ver");
            //string version = "2000.01.01.0000.0000";
            var version = File.ReadAllText(gamePath);
            return version;
        }

        /// <summary>
        /// 获取FFXIV解析插件的区域信息。
        /// </summary>
        /// <param name="ffxiv_plugin"></param>
        /// <returns></returns>
        public static string GetRegion(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxiv_plugin)
        {
            switch (GetLanguageId(ffxiv_plugin))
            {
                case 1:
                //return "en";
                case 2:
                //return "fr";
                case 3:
                //return "de";
                case 4:
                    //return "ja";
                    return "intl";
                case 5:
                    return "cn";
                case 6:
                    return "ko";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取FFXIV解析插件语言信息。
        /// </summary>
        /// <param name="ffxiv_plugin"></param>
        /// <returns></returns>
        public static string GetLocaleString(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxiv_plugin)
        {
            switch (GetLanguageId(ffxiv_plugin))
            {
                case 1:
                    return "en";
                case 2:
                    return "fr";
                case 3:
                    return "de";
                case 4:
                    return "ja";
                case 5:
                    return "cn";
                case 6:
                    return "ko";
                default:
                    return null;
            }
        }

        /// <summary>
        /// 直接获取原始的FFXIV解析插件语言ID。
        /// </summary>
        /// <param name="ffxiv_plugin"></param>
        /// <returns></returns>
        public static int GetLanguageId(FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxiv_plugin)
        {
            if (ffxiv_plugin == null)
            {
                if (ffxivPlugin == null) return 0;
                ffxiv_plugin = ffxivPlugin;
            }
            try
            {
                return (int)ffxiv_plugin.DataRepository.GetSelectedLanguageID();
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }

    public static class Extension
    {
        /// <summary>Looks for the next occurrence of a sequence in a byte array</summary>
        /// <param name="array">Array that will be scanned</param>
        /// <param name="start">Index in the array at which scanning will begin</param>
        /// <param name="sequence">Sequence the array will be scanned for</param>
        /// <returns>
        ///   The index of the next occurrence of the sequence of -1 if not found
        /// </returns>
        public static int IndexOf(this byte[] array, int start, byte[] sequence)
        {
            int end = array.Length - sequence.Length; // past here no match is possible
            byte firstByte = sequence[0]; // cached to tell compiler there's no aliasing

            while (start <= end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 1; ; ++offset)
                    {
                        if (offset == sequence.Length)
                        { // full sequence matched?
                            return start;
                        }
                        else if (array[start + offset] != sequence[offset])
                        {
                            break;
                        }
                    }
                }
                ++start;
            }

            // end of array reached without match
            return -1;
        }
    }
    public static class SafeThreadInvoker
    {
        public static object SafeInvoke(this Control control, Delegate method)
        {
            var asyncResult = control.BeginInvoke(method);

            return SafeWait(control, asyncResult);
        }

        public static object SafeInvoke(this Control control, Delegate method, object[] args)
        {
            var asyncResult = control.BeginInvoke(method, args);

            return SafeWait(control, asyncResult);
        }

        private static object SafeWait(Control control, IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return control.EndInvoke(result);
            }

            for (var i = 0; i < 50; i++)
            {
                if (control.IsDisposed || control.Disposing || !control.IsHandleCreated)
                {
                    break;
                }

                result.AsyncWaitHandle.WaitOne(100);
                if (result.IsCompleted)
                {
                    return control.EndInvoke(result);
                }
            }

            return null;
        }

        public static void AppendDateTimeLine(this RichTextBox target, string text)
        {
            if (target.InvokeRequired)
            {
                target.SafeInvoke(new Action(delegate
                {
                    target.AppendDateTimeLine(text);
                }));
            }
            else
            {
                target.AppendText($"\n[{DateTime.Now.ToLongTimeString()}] {text}");
                target.ScrollToCaret();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class EventNameAttribute : Attribute
    {
        public string EventName { get; }

        public EventNameAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}
