using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAccessorCore.ApiClient {
    /// <summary>
    /// 
    /// </summary>
    public interface IRequestContent {
        string ToString(Encoding encoding);
    }
}
