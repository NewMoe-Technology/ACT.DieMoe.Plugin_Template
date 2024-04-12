using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using MyACTPlugin.Utils;

namespace MyACTPlugin
{
    public class PluginMain : IActPluginV1
    {
        // ACT界面(插件列表中)的状态标签
        private Label lblStatus;
        internal PluginPage configPage;

        /// <summary>
        /// 插件初始化
        /// </summary>
        /// <param name="pluginScreenSpace"></param>
        /// <param name="pluginStatusText"></param>
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            Helpers._tabPage = pluginScreenSpace;
            lblStatus = pluginStatusText;
            pluginScreenSpace.Text = "插件标题";

            Helpers._plugin = this;

            Helpers._configPage = configPage = new PluginPage();
            configPage.Dock = DockStyle.Fill;
            pluginScreenSpace.Controls.Add(configPage);

            // 初始化
            Attach();

            lblStatus.Text = "初始化完成";
        }

        /// <summary>
        /// 插件反初始化
        /// </summary>
        public void DeInitPlugin()
        {
            Helpers.ffxivPlugin.DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
            Helpers.ffxivPlugin.DataSubscription.ProcessChanged -= OnFFXIVProcessChanged;
            //settings.save();
            lblStatus.Text = "插件已卸载";
        }

        /// <summary>
        /// 附加到解析插件
        /// </summary>
        public void Attach()
        {
            lock (this)
            {
                if (ActGlobals.oFormActMain == null)
                {
                    Helpers.ffxivPlugin = null;
                    return;
                }

                if (Helpers.ffxivPlugin == null)
                {
                    var ffxivPluginData = ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(x => x.pluginObj?.GetType().ToString() == "FFXIV_ACT_Plugin.FFXIV_ACT_Plugin");
                    Helpers.ffxivPlugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)ffxivPluginData?.pluginObj;
                    if (Helpers.ffxivPlugin != null)
                    {
                        var waitingFFXIVPlugin = new Task(() => {
                            var isFFXIVPluginStarted = false;
                            while (!isFFXIVPluginStarted)
                            {
                                if (ffxivPluginData.lblPluginStatus.Text.ToUpper().Contains("Started".ToUpper()))
                                {
                                    Helpers.ffxivPlugin.DataSubscription.ProcessChanged += OnFFXIVProcessChanged;
                                    OnFFXIVProcessChanged(Helpers.ffxivPlugin.DataRepository.GetCurrentFFXIVProcess());
                                    isFFXIVPluginStarted = true;
                                    return;
                                }
                                Thread.Sleep(3000);
                            }
                        });
                        waitingFFXIVPlugin.Start();
                    }
                }
            }
        }

        /// <summary>
        /// 当客户端进程变化时触发操作
        /// </summary>
        /// <param name="process"></param>
        private void OnFFXIVProcessChanged(System.Diagnostics.Process process)
        {
            var gameProcess = process;
            if (gameProcess == null)
                return;
            var gameVersion = Helpers.GetGameVersion(gameProcess);
            var gameRegion = Helpers.GetRegion(Helpers.ffxivPlugin);

            Helpers.ffxivPlugin.DataSubscription.NetworkReceived -= ffxivPluginNetworkReceivedDelegate;
            Helpers.ffxivPlugin.DataSubscription.NetworkReceived += ffxivPluginNetworkReceivedDelegate;
        }

        // 网络包解析
        private ushort MapEffect = 0xFFFF;
        private unsafe void ffxivPluginNetworkReceivedDelegate(string connection, long epoch, byte[] message)
        {
            if (message.Length < sizeof(ServerMessageHeader))
            {
                return;
            }
            try
            {
                fixed (byte* ptr = message)
                {
                    var header = (ServerMessageHeader*)ptr;
                    var dataPtr = ptr + 0x20;
                    if (header->MessageType == MapEffect)
                    {
                        onMapEffectEvent(header->ActorID, (FFXIVIpcMapEffect*)dataPtr);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        // 示例事件
        [EventName("onMapEffectEvent")]
        public unsafe void onMapEffectEvent(uint actorId, FFXIVIpcMapEffect* message)
        {
            string logline = $"{actorId:X}:{message->parm1:X8}:{message->parm2:X8}:{message->parm3:X8}:{message->parm4:X8}:";
            Helpers.AddLogLine("103", logline);
        }

        /// <summary>
        /// 示例网络包结构
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct ServerMessageHeader
        {
            [FieldOffset(0)]
            public uint MessageLength;

            [FieldOffset(4)]
            public uint ActorID;

            [FieldOffset(8)]
            public uint LoginUserID;

            [FieldOffset(12)]
            public uint Unknown1;

            [FieldOffset(16)]
            public ushort Unknown2;

            [FieldOffset(18)]
            public ushort MessageType;

            [FieldOffset(20)]
            public uint Unknown3;

            [FieldOffset(24)]
            public uint Seconds;

            [FieldOffset(28)]
            public uint Unknown4;
        }

        /// <summary>
        /// 示例MapEffect结构
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct FFXIVIpcMapEffect
        {
            [FieldOffset(0)]
            public uint parm1;

            [FieldOffset(4)]
            public uint parm2;

            [FieldOffset(8)]
            public ushort parm3;

            [FieldOffset(12)]
            public ushort parm4;
        }
    }
}
