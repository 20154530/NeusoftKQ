///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace NeusoftKQ.View.Controls {
    public class ContextMenuEx : ContextMenu {
        #region Properties
        #endregion

        #region Methods
        #endregion

        #region Constructors
        static ContextMenuEx() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContextMenuEx), new FrameworkPropertyMetadata(typeof(ContextMenuEx), FrameworkPropertyMetadataOptions.Inherits));
        }
        #endregion
    }
}
