///**********************************************************************
/// Author       : @ Y_Theta
/// Description  : Neusoft Kq service command line ver
///
///
///**********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebAccessorCore.ApiClient;

namespace KQSerCore {

    public class MainC {
        #region Properties
        private const string CMD_HELP = "h";
        private const string CMD_USER = "u";
        private const string CMD_PWD = "p";
        private const string CMD_KQ = "dokq";

        private const string MSG_ERROR = "Error : command format error ! Enter kqsercore -h for help!";
        #endregion

        #region Methods
        public static void Main(string[] args) {
            if (args.Length > 0) {
                string cmd = args[0].ToLower();
                if (cmd.ToArray()[0].Equals('-')) {
                    string cmdstr = cmd.Remove(0, 1);
                    switch (cmdstr) {
                        case CMD_HELP:
                            Console.WriteLine(Properties.Kqres.help);
                            break;
                        case CMD_USER:
                            if (args.Length == 2) {
                                Properties.Kqsersetting.Default.user = args[1];
                                Properties.Kqsersetting.Default.Save();
                            } else if (args.Length == 1) {
                                Console.WriteLine(Properties.Kqsersetting.Default.user);
                            } else {
                                Console.WriteLine(MSG_ERROR);
                            }
                            break;
                        case CMD_PWD:
                            if (args.Length == 2) {
                                Properties.Kqsersetting.Default.password = args[1];
                                Properties.Kqsersetting.Default.Save();
                            } else if (args.Length == 1) {
                                Console.WriteLine(Properties.Kqsersetting.Default.password);
                            } else {
                                Console.WriteLine(MSG_ERROR);
                            }
                            break;
                        case CMD_KQ:
                            try {
                                Console.WriteLine(new KQSer(new HttpClientAccessor()).DoKQ());

                            } catch (Exception e) {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.StackTrace);
                            }
                            break;
                    }
                } else {
                    Console.WriteLine(MSG_ERROR);
                }
            } else {
                Console.WriteLine("This is a console client for neusoft one key kq.");
            }
        }
        #endregion

        #region Constructors
        #endregion
    }
}
