using System;
using System.IO;
using System.Reflection;
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

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            Helpers._tabPage = pluginScreenSpace;
            lblStatus = pluginStatusText;
            pluginScreenSpace.Text = "插件标题";

            Helpers._plugin = this;

            Helpers._configPage = configPage = new PluginPage();
            configPage.Dock = DockStyle.Fill;
            pluginScreenSpace.Controls.Add(configPage);

            lblStatus.Text = "初始化完成";
        }

        public void DeInitPlugin()
        {
            //settings.save();
            lblStatus.Text = "插件已卸载";
        }

    }
}
