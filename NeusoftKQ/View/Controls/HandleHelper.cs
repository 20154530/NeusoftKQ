///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeusoftKQ.View.Controls {
    /// <summary>
    /// 获得控件句柄以支持互操作
    /// </summary>
    public class HandleHelper {
        /// <summary>
        /// 获得控件句柄
        /// </summary>
        public static IntPtr GetVisualHandle(Visual visual) {
            return ((HwndSource)PresentationSource.FromVisual(visual)).Handle;
        }

    }
}
