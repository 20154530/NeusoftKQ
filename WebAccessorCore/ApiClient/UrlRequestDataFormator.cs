using System.Collections.Generic;
using System.Text;
using System.Web;

namespace WebAccessorCore.ApiClient {
    /// <summary>
    /// 
    /// </summary>
    public class UrlRequestDataFormator : IRequestDataFormator {
        public string GetData(KeyValuePair<string, string>[] args, Encoding encoding, string url =null) {
            encoding = encoding == null ? Encoding.UTF8 : encoding;
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args) {
                sb.Append(arg.Key);
                sb.Append('=');
                sb.Append(arg.Value);
                sb.Append('&');
            }
            sb.Remove(sb.Length - 1, 1);
            return HttpUtility.UrlEncode(sb.ToString(), encoding);
           // return sb.ToString();
        }
    }
}
