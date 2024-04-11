using System;
using System.Windows.Forms;

namespace MyACTPlugin.Utils
{
    internal static class Helpers
    {
        internal static PluginMain _plugin;
        internal static PluginPage _configPage;
        internal static TabPage _tabPage;
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
}
