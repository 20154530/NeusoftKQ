using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebAccessorCore.ApiClient {
    /// <summary>
    /// 
    /// </summary>
    public interface IApiClientContract {
        #region Properties
        /// <summary>
        /// 表示编码格式
        /// </summary>
        string CharSet { get; set; }

        /// <summary>
        /// 启用Cookies
        /// </summary>
        bool AllowCookies { get; set; }

        /// <summary>
        /// 访问器类型
        /// </summary>
        Type ClientType { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// 初始化服务
        /// </summary>
        void Init();

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns>返回流</returns>
        Stream Get(string url);

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        string Get(string url, Encoding requestecnoding = null);

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="baseurl"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string Get(string baseurl, KeyValuePair<string, string>[] args, IRequestDataFormator formator = null, Encoding requestecnoding = null);

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="arg">请求参数</param>
        /// <returns></returns>
        string Post(string url, KeyValuePair<string, string>[] args, IRequestDataFormator formator = null, Encoding requestecnoding = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="set">请求串类</param>
        /// <returns></returns>
        string Post(string url, IRequestContent set, Encoding requestecnoding = null);
        #endregion
    }
}
