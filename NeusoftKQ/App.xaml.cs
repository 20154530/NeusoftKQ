using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using NeusoftKQ.Services;
using NeusoftKQ.View.Controls;
using NeusoftKQ.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace NeusoftKQ {
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application {
        private const string ReleaseId = "ReleaseId";
        private const string ProductName = "ProductName";
        private NotifyIcon _icon;
        private ContextMenuEx _cmenu;

        /// <summary>
        /// 
        /// </summary>
        public static INotifySer NotifyManager;

        /// <summary>
        /// 统一调用此方法生成托盘菜单的Popup
        /// 可在应用程序内部获得统一风格
        /// </summary>
        public static PopupBase CreatAreaPopup(UIElement content) {
            PopupBase popup = new PopupBase();
            popup.Child = content;
            popup.Style = Current.FindResource("AreaPopup") as Style;
            return popup;
        }

        /// <summary>
        /// 检查电脑版本信息
        /// </summary>
        private bool frameworkversioncheck() {
            bool is64 = Environment.Is64BitOperatingSystem;
            RegistryKey LocalMachine;
            if (is64)
                LocalMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            else
                LocalMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            RegistryKey currentversion = LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", RegistryKeyPermissionCheck.ReadSubTree);
            string release = currentversion.GetValue(ReleaseId)?.ToString();
            string sysname = currentversion.GetValue(ProductName)?.ToString();

            if (!string.IsNullOrEmpty(sysname) && sysname.Contains("Windows 10")) {
                if (int.Parse(release) > 1511) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 重写启动事件,选择性加载程序集
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {
            if (frameworkversioncheck())
                initWinXNotify();
            else
                initGeneralNotify();

            base.OnStartup(e);
        }

        /// <summary>
        /// 创建发出Win10样式通知的组件
        /// </summary>
        private void initWinXNotify() {
            var notifymod = Assembly.LoadFile(Assembly.GetExecutingAssembly().Location.Replace("NeusoftKQ.exe", "") + "Resources\\NeusoftKQNotify.dll");
            AppDomain.CurrentDomain.Load(notifymod.FullName);
            NotifyManager = (INotifySer)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(notifymod.FullName, "NeusoftKQNotify.NeusoftKQNotifySer");
            NotifyManager.Init(nameof(NeusoftKQ));
            _icon = new NotifyIcon {
                Visible = true,
                Text = "Neusoft Onkey KQ",
                Icon = new System.Drawing.Icon(AppDomain.CurrentDomain.BaseDirectory + "Card.ico"),
                ContextMenuStrip = new ContextMenuStrip()
            };
            _icon.MouseClick += Icon_Click;
        }

        /// <summary>
        /// 创建发出一般样式通知的组件
        /// </summary>
        private void initGeneralNotify() {
            NotifyManager = (INotifySer)new IconNotifySer(_icon);
            NotifyManager.Init(null);
            _icon.MouseClick += Icon_Click;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void createContextMenu() {
            Style winxmenu = Current.Resources["WinXTaskBarContextMenuItemStyle"] as Style;
            _cmenu = new ContextMenuEx { Style = Current.Resources["WinXTaskBarContextMenuStyle"] as Style };
            MenuItemEx exit = new MenuItemEx { Style = winxmenu, LabelString = "退出", Command = MainVM.Singleton.Operation, CommandParameter = "CMD_EXIT" };
            MenuItemEx about = new MenuItemEx { Style = winxmenu, LabelString = "关于", Command = MainVM.Singleton.Operation, CommandParameter = "CMD_ABOUT" };

            _cmenu.Items.Add(about);
            _cmenu.Items.Add(exit);
        }

        /// <summary>
        /// 任务栏图标单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Icon_Click(object sender, MouseEventArgs e) {
            switch (e.Button) {
                case MouseButtons.Left:
                    break;
                case MouseButtons.Right:
                    if (_cmenu is null)
                        createContextMenu();
                    if (_cmenu.IsOpen)
                        return;
                    _cmenu.IsOpen = true;
                    break;
            }
        }

        public App() {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            string[] assemblyloc = NeusoftKQ.Properties.Settings.Default.Loacation.Split(';');
            Match mc = Regex.Match(args.Name, "(.+?),");
            string name = mc.Groups[1].Value;
            string file = $"{AppDomain.CurrentDomain.BaseDirectory}{name}.dll";
            if (File.Exists(file))
                return Assembly.LoadFile(file);
            foreach (var path in assemblyloc) {
                file = $"{AppDomain.CurrentDomain.BaseDirectory}{path}\\{name}.dll";
                if (File.Exists(file))
                    return Assembly.LoadFile(file);
            }
            return null;
        }
    }
}
