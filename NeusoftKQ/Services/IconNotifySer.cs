///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NeusoftKQ.Services {

    /// <summary>
    /// 使用NotifyIcon发送windows通知
    /// </summary>
    public class IconNotifySer : INotifySer {
        #region Properties
        private NotifyIcon _icon;
        #endregion

        #region Methods
        void INotifySer.Init(string appid) {
            _icon = new NotifyIcon {
                Visible = true,
                Text = "Neusoft Onkey KQ",
                Icon = SystemIcons.Application,
                ContextMenuStrip = new ContextMenuStrip()
            };
        }

        void INotifySer.Notify(string title, string msg) {
            throw new NotImplementedException();
        }
        #endregion

        #region Constructors
        public IconNotifySer(NotifyIcon icon) {
            _icon = icon;
        }
        #endregion

    }
}
