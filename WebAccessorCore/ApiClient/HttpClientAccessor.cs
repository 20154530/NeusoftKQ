using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebAccessorCore.ApiClient {
    /// <summary>
    /// 使用HttpClient进行网络请求
    /// </summary>
    public class HttpClientAccessor : IApiClientContract {

        #region Properties

        private HttpClient _client;

        private HttpClientHandler _handler;

        public bool AllowCookies {
            get => _handler.UseCookies;
            set { if (_handler != null) _handler.UseCookies = value; }
        }

        public string CharSet { get; set; }

        public Type ClientType { get; set; }

        #endregion
        /// <summary>
        /// 获取内容流
        /// </summary>
        /// <returns></returns>
        public Stream Get(string url) {
            var respone = _client.GetAsync(url).Result;
            return respone.Content.ReadAsStreamAsync().Result;
        }

        /// <summary>
        /// 普通Get请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="requestecnoding">编码格式</param>
        /// <returns></returns>
        public string Get(string url, Encoding requestecnoding = null) {
            if (requestecnoding is null) requestecnoding = Encoding.UTF8;
            return catchError(() => getResult(_client.GetAsync(requestecnoding.GetString(Encoding.Default.GetBytes(url))).Result));
        }

        /// <summary>
        /// 带参数Get请求
        /// </summary>
        /// <param name="baseurl">地址</param>
        /// <param name="args">参数</param>
        /// <param name="formator">参数格式转换器</param>
        /// <param name="requestecnoding">编码格式</param>
        /// <returns></returns>
        public string Get(string baseurl, KeyValuePair<string, string>[] args, IRequestDataFormator formator = null, Encoding requestecnoding = null) {
            if (requestecnoding is null) requestecnoding = Encoding.UTF8;
            return catchError(() => formator == null ? getResult(_client.GetAsync(
                new UrlRequestDataFormator().GetData(args, requestecnoding, baseurl)).Result) :
                getResult(_client.GetAsync(formator.GetData(args, requestecnoding, baseurl)).Result)
            );
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="set">参数:自定义ToString()的类型，用于自定义格式的请求参数</param>
        /// <param name="requestecnoding">编码格式</param>
        /// <returns></returns>
        public string Post(string url, IRequestContent set, Encoding requestecnoding = null) {
            if (requestecnoding is null) requestecnoding = Encoding.UTF8;
            return catchError(() => getResult(_client.PostAsync(url, new StringContent(set.ToString(requestecnoding))).Result));
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="args">参数</param>
        /// <param name="formator">格式器</param>
        /// <param name="requestecnoding">编码格式</param>
        /// <returns></returns>
        public string Post(string url, KeyValuePair<string, string>[] args, IRequestDataFormator formator = null, Encoding requestecnoding = null) {
            if (requestecnoding is null) requestecnoding = Encoding.UTF8;
            return catchError(() => formator == null ?
            getResult(_client.PostAsync(url, new FormUrlEncodedContent(args)).Result) :
            getResult(_client.PostAsync(url, new StringContent(formator.GetData(args, requestecnoding))).Result)
            );
        }

        /// <summary>
        /// 捕获异常
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private string catchError(RequestAction action) {
            try {
                return action.Invoke();
            } catch (Exception e) {
                return e.Message + '\n' + e.StackTrace;
            }
        }

        private string getResult(HttpResponseMessage response) {
            byte[] content = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.GetEncoding(CharSet).GetString(content);
        }

        public void Init() {
            CharSet = "utf-8";
            _handler = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip };
            _client = new HttpClient(_handler);
            AllowCookies = true;
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
            _client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
        }


    }
}