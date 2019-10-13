///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using NeusoftKQ.View.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NeusoftKQ.ViewModel {

    /// <summary>
    /// 主控
    /// </summary>
    public class MainVM : ViewModelBase<MainVM> {
        #region Properties
        private const string CMD_EXIT = "CMD_EXIT";
        private const string CMD_ABOUT = "CMD_ABOUT";

        public CommandBase Operation { get; set; }

        public event CommandActionEventHandler DoOperation;
        #endregion

        #region Methods
        private void init() {
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
                }
        }
        #endregion

        #region Constructors
        public MainVM() => init();
        #endregion
    }
}
