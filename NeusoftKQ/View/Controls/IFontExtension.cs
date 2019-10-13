///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;

namespace NeusoftKQ.View.Controls {

    internal interface IFontIconExtension {
        #region Icon
        /// <summary>
        /// 图标的可见性
        /// </summary>
        Visibility IconVisibility { get; set; }


        #region Alignment
        Thickness IconMargin { get; set; }
        HorizontalAlignment IconHorizontalAlignment { get; set; }
        VerticalAlignment IconVerticalAlignment { get; set; }
        TextAlignment IconTextAlignment { get; set; }
        #endregion

        #region Color
        /// <summary>
        /// 图标的颜色和背景
        /// </summary>
        Brush IconFgNormal { get; set; }
        Brush IconFgOver { get; set; }
        Brush IconFgPressed { get; set; }
        Brush IconFgDisabled { get; set; }
        Brush IconBgNormal { get; set; }
        Brush IconBgOver { get; set; }
        Brush IconBgPressed { get; set; }
        Brush IconBgDisabled { get; set; }
        #endregion

        #endregion
    }

    internal interface IFontLabelExtension {
        #region Label
        /// <summary>
        /// 标签的可见性
        /// </summary>
        Visibility LabelVisibility { get; set; }

        /// <summary>
        /// 标签内容
        /// </summary>
        String LabelString { get; set; }

        /// <summary>
        /// 标签的字体大小
        /// </summary>
        Double LabelFontSize { get; set; }

        /// <summary>
        /// 标签字重
        /// </summary>
        FontWeight LabelFontWeight { get; set; }

        /// <summary>
        /// 标签的字体
        /// </summary>
        FontFamily LabelFontFamily { get; set; }


        #region Alignment
        Thickness LabelMargin { get; set; }
        HorizontalAlignment LabelHorizontalAlignment { get; set; }
        VerticalAlignment LabelVerticalAlignment { get; set; }
        TextAlignment LabelTextAlignment { get; set; }
        #endregion

        #region Color
        /// <summary>
        /// 标签的前景和背景
        /// </summary>
        Brush LabelFgNormal { get; set; }
        Brush LabelFgOver { get; set; }
        Brush LabelFgPressed { get; set; }
        Brush LabelFgDisabled { get; set; }
        Brush LabelBgNormal { get; set; }
        Brush LabelBgOver { get; set; }
        Brush LabelBgPressed { get; set; }
        Brush LabelBgDisabled { get; set; }
        #endregion

        #endregion
    }

    internal interface IToggleBrush {
        #region Icon
        string IconSelect { get; set; }

        Brush IconSelectFgNormal { get; set; }
        Brush IconSelectFgOver { get; set; }
        Brush IconSelectFgPressed { get; set; }
        Brush IconSelectFgDisabled { get; set; }
        Brush IconSelectBgNormal { get; set; }
        Brush IconSelectBgOver { get; set; }
        Brush IconSelectBgPressed { get; set; }
        Brush IconSelectBgDisabled { get; set; }

        #endregion

        #region Label
        string LabelSelect { get; set; }

        Brush LabelSelectFgNormal { get; set; }
        Brush LabelSelectFgOver { get; set; }
        Brush LabelSelectFgPressed { get; set; }
        Brush LabelSelectFgDisabled { get; set; }
        Brush LabelSelectBgNormal { get; set; }
        Brush LabelSelectBgOver { get; set; }
        Brush LabelSelectBgPressed { get; set; }
        Brush LabelSelectBgDisabled { get; set; }

        #endregion
    }

}
