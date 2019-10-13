///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------


namespace NeusoftKQ.Services {
    /// <summary>
    /// Windows通知接口约定
    /// </summary>
    public interface INotifySer {

        #region Methods

        void Init(string appid);

        void Notify(string title,string msg);
        #endregion
    }
}
