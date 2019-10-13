using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAccessorCore.ApiClient {

    /// <summary>
    /// 请求方法委托
    /// </summary>
    /// <returns></returns>
    public delegate string RequestAction();

    /// <summary>
    /// 请求参数转换器
    /// </summary>
    public interface IRequestDataFormator {
        string GetData(KeyValuePair<string, string>[] args, Encoding encoding, string url = null);
    }
}
