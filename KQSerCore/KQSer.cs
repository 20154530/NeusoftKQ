using AforgeNumVerify;
using AforgeNumVerify.AForge.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebAccessorCore.ApiClient;

namespace KQSerCore {
    /// <summary>
    /// 考勤服务
    /// </summary>
    public class KQSer {
        #region Properties
        private IApiClientContract _client;
        #endregion

        #region Methods
        /// <summary>
        /// 初始化连接代理
        /// </summary>
        private void init() {
            _client.Init();
            _client.AllowCookies = true;
            _client.CharSet = "gbk";
        }

        /// <summary>
        /// 获取form所需的Key字段
        /// </summary>
        /// <returns></returns>
        private string[] pregetaccesskey() {
            try {
                string[] keylist = new string[5];
                var result = _client.Get("http://kq.neusoft.com/", null);
                var col = Regex.Matches(result, @"<input(.+)>");
                if (col.Count == 9) {
                    keylist[0] = Regex.Match(col[2].Value, @"name=""(.+?)""").Groups[1].Value;
                    keylist[1] = Regex.Match(col[3].Value, @"value=""(.+?)""").Groups[1].Value;
                    keylist[2] = Regex.Match(col[4].Value, @"name=""(.+?)""").Groups[1].Value;
                    keylist[3] = Regex.Match(col[5].Value, @"name=""(.+?)""").Groups[1].Value;
                    keylist[4] = Regex.Match(col[6].Value, @"name=""(.+?)""").Groups[1].Value;
                }
                return keylist;
            } catch (Exception e) {
                return new string[] { e.Message + "\n" + e.StackTrace };
            }
        }

        /// <summary>
        /// 识别验证码
        /// </summary>
        /// <returns></returns>
        private string pregetverifycode() {
            try {
                string verurl = "http://kq.neusoft.com/imageRandeCode";
                var verimg = _client.Get(verurl);
                Bitmap tbp = Main.PreProcess(new Bitmap(verimg, false));
                var templateList = new List<Bitmap> {
                Properties.Kqres._0,
                Properties.Kqres._1,
                Properties.Kqres._2,
                Properties.Kqres._3,
                Properties.Kqres._4,
                Properties.Kqres._5,
                Properties.Kqres._6,
                Properties.Kqres._7,
                Properties.Kqres._8,
                Properties.Kqres._9,
            };
                List<Bitmap> tlbs = Main.ToResizeAndCenterIt(Main.Crop_X(Main.Crop_Y(tbp)));
                ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.9f);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < tlbs.Count; i++) {
                    float max = 0;
                    int index = 0;
                    for (int j = 0; j < templateList.Count; j++) {
                        var compare = templateMatching.ProcessImage(tlbs[i], templateList[j]);
                        if (compare.Length > 0 && compare[0].Similarity > max) {
                            max = compare[0].Similarity;
                            index = j;
                        }
                    }
                    sb.Append(index);
                }
                return sb.ToString();
            } catch (Exception e) {
                return e.Message + "\n" + e.StackTrace;
            }

        }

        #region TempleteGenerate

        //for (int k = 1; k < 7; k++) {
        //    Bitmap tbp = Main.LoadTestImg(@"C:\Users\Y_T\Desktop\testimg\imageRandeCode" + k + ".jpg");
        //    IMG.Source = Imaging.CreateBitmapSourceFromHBitmap(tbp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //    List<Bitmap> lbps = Main.ToResizeAndCenterIt(Main.Crop_X(Main.Crop_Y(tbp)));

        //    lbps.ForEach(b => {
        //        SpiltImg.Children.Add(new Image() {
        //            Source = Imaging.CreateBitmapSourceFromHBitmap(b.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
        //            Stretch = Stretch.None,
        //            Margin = new Thickness(8, 0, 8, 0),
        //        });
        //    });

        //    Console.WriteLine("transform complete");

        //    int i = k * 4;
        //    lbps.ForEach(b => {
        //        using (FileStream f = File.Create(@"C:\Users\Y_T\Desktop\testimgresult\" + i + ".bmp")) {
        //            b.Save(f, ImageFormat.Bmp);
        //            f.Flush();
        //        }
        //        i++;
        //    });
        //}
        #endregion

        #region MatchingTest

        //Bitmap tbp = Main.LoadTestImg(@"C:\Users\Y_T\Desktop\testimg\imageRandeCode1.jpg");
        //var templateList = new List<Bitmap> {
        //    Properties.Resources._0,
        //    Properties.Resources._1,
        //    Properties.Resources._2,
        //    Properties.Resources._3,
        //    Properties.Resources._4,
        //    Properties.Resources._5,
        //    Properties.Resources._6,
        //    Properties.Resources._7,
        //    Properties.Resources._8,
        //    Properties.Resources._9,
        //};
        //IMG.Source = Imaging.CreateBitmapSourceFromHBitmap(tbp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //List<Bitmap> tlbs = Main.ToResizeAndCenterIt(Main.Crop_X(Main.Crop_Y(tbp)));
        //ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.9f);
        //StringBuilder sb = new StringBuilder();
        //for (int i = 0; i < tlbs.Count; i++) {
        //    float max = 0;
        //    int index = 0;
        //    for (int j = 0; j < templateList.Count; j++) {
        //        var compare = templateMatching.ProcessImage(tlbs[i], templateList[j]);
        //        if (compare.Length > 0 && compare[0].Similarity > max) {
        //            max = compare[0].Similarity;
        //            index = j;
        //        }
        //    }
        //    sb.Append(index);
        //}

        #endregion

        /// <summary>
        /// 执行考勤
        /// </summary>
        /// <returns></returns>
        public string DoKQ() {
            return DoKQ(Properties.Kqsersetting.Default.user, Properties.Kqsersetting.Default.password);
        }

        /// <summary>
        /// 执行考勤
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public string DoKQ(string user ,string pwd) {
            string status = null;
            string[] keylist = pregetaccesskey();
            string verify = pregetverifycode();
            var record = _client.Post("http://kq.neusoft.com/login.jsp", new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("login","true"),
                new KeyValuePair<string, string>("neusoft_attendance_online",""),
                new KeyValuePair<string, string>(keylist[0],""),
                new KeyValuePair<string, string>("neusoft_key",keylist[1]),
                new KeyValuePair<string, string>(keylist[2],user),
                new KeyValuePair<string, string>(keylist[3],pwd),
                new KeyValuePair<string, string>(keylist[4],verify),
            });
            var verkey = Regex.Match(record, @"<.+""currentempoid"".+value=""(.+)"">");
            _client.Post("http://kq.neusoft.com/record.jsp", new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("currentempoid",verkey.Groups[1].Value),
                new KeyValuePair<string, string>("browser","Chrome"),
            });
            return status;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 初始化考勤服务，从一个API访问接口
        /// </summary>
        /// <param name="dependence"></param>
        public KQSer(IApiClientContract dependence) {
            _client = dependence;
            init();
        }
        #endregion
    }
}
