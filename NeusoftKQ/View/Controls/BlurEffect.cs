///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using static NeusoftKQ.View.Controls.DllImportMethods;

namespace NeusoftKQ.View.Controls {
    /// <summary>
    /// 启用毛玻璃效果
    /// </summary>
    public class BlurEffect {

        /// <summary>
        /// 设置控件效果
        /// </summary>
        public static bool SetBlur(IntPtr hwnd, AccentState state) {
            try {
                EnableBlur(hwnd, state);
                return true;
            } catch {
                return false;
            }

        }

        /// <summary>
        /// 给句柄指定的控件设置毛玻璃效果
        /// </summary>
        internal static void EnableBlur(IntPtr hwnd, AccentState state) {

            var accent = new AccentPolicy {
                AccentState = state
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(hwnd, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }

}
