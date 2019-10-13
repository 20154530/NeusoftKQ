using NeusoftKQ.Services;
using System;
using System.Runtime.InteropServices;
using ToastCore.Notification;

namespace NeusoftKQNotify {

    /// <summary>
    /// 判断当前Net版本后引用此dll
    /// </summary>
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [Guid("51AFD681-C7AE-4F2E-8D0E-A9617BDC342B"), ComVisible(true)]
    public class NeusoftKQNotifySer : NotificationService, INotifySer {

        void INotifySer.Init(string appid) {
            Init<NeusoftKQNotifySer>(appid);
        }

        void INotifySer.Notify(string title, string msg) {
            Notify(title, msg);
        }
    }
}
