///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeusoftKQ.ViewModel {

    /// <summary>
    /// 属性变更回调
    /// </summary>
    public delegate bool PropertyChangedEventHandler(object op, object np);

    /// <summary>
    /// 基本的ViewModel
    /// </summary>
    public class ViewModelBase<T> : INotifyPropertyChanged where T : class, new() {
        #region Properties
        private static T _singleton;
        private static readonly object _singletonlock = new object();
        public static T Singleton {
            get {
                if (_singleton is null) {
                    lock (_singletonlock) {
                        if (_singleton is null) {
                            _singleton = new T();
                        }
                    }
                }
                return _singleton;
            }
            protected set {
                lock (_singletonlock) {
                    _singleton = value;
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        /// <summary>
        /// 带事件通知回调属性设置方法
        /// 只有在用于Property的set方法时才能触发PropertyChanged
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="nProperty">新属性</param>
        /// <param name="value">传入新属性</param>
        /// <param name="oProperty">旧熟悉</param>
        /// <param name="callBack">回调若属性的更改需要得到控制</param>
        protected void SetValue<U>(out U nProperty, U value, U oProperty, PropertyChangedEventHandler callBack = null) {
            if ((value != null && value.Equals(oProperty)) || (value == null && oProperty == null)) {
                nProperty = oProperty;
                return;
            }
            bool accept = true;
            accept = callBack is null ? true : callBack.Invoke(oProperty, value);
            if (accept)
                nProperty = value;
            else
                nProperty = oProperty;
            PropertyChanged?.Invoke(_singleton, new PropertyChangedEventArgs(GetParaString()));
        }

        protected static string GetParaString() {
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(2);
            var methodBase = stackFrame.GetMethod();
            string[] namepara = methodBase.Name.Split('_');
            if (namepara.Length > 2) {
                StringBuilder bulider = new StringBuilder();
                for (int i = 1; i < namepara.Length; i++) {
                    bulider.Append(namepara[i]);
                    bulider.Append('_');
                }
                bulider.Remove(bulider.Length - 1, 1);
                return bulider.ToString();
            }
            return namepara[1];
        }
        #endregion

        #region Constructors
        public ViewModelBase() { }
        #endregion
    }

}
