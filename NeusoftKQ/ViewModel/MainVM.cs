///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using KQSerCore;
using NeusoftKQ.View.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAccessorCore.ApiClient;

namespace NeusoftKQ.ViewModel {

    /// <summary>
    /// 主控
    /// </summary>
    public class MainVM : ViewModelBase<MainVM> {
        #region Properties
        private KQSer _client;
        private const string CMD_EXIT = "CMD_EXIT";
        private const string CMD_ABOUT = "CMD_ABOUT";
        private const string CMD_KQ = "CMD_KQ";
        private const string CMD_VKQ = "CMD_VKQ";

        private string _result;
        public string Result {
            get => _result;
            set => SetValue(out _result, value, Result);
        }

        public CommandBase Operation { get; set; }

        public event CommandActionEventHandler DoOperation;
        #endregion

        #region Methods
        private void init() {
            _client = new KQSer(new HttpClientAccessor());
            Operation = new CommandBase(para => DoOperation?.Invoke(this, new CommandArgs(para)));
            DoOperation += MainVM_DoOperation;
        }

        private void MainVM_DoOperation(object sender, CommandArgs args) {
            if (sender == this)
                switch (args.Parameter.ToString()) {
                    case CMD_EXIT:
                        App.Current.Shutdown(0);
                        break;
                    case CMD_ABOUT:
                        break;
                    case CMD_KQ:
                        _client.DoKQ();
                        break;
                    case CMD_VKQ:
                        Result = _client.ViewKQ();
                        break;
                }
        }
        #endregion

        #region Constructors
        public MainVM() => init();
        #endregion
    }
}
