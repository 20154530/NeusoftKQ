using System.Collections.Generic;
using System.Text;

namespace WebAccessorCore.ApiClient {
    public class JsonRequestDataFormator : IRequestDataFormator {
        public string GetData(KeyValuePair<string, string>[] args, Encoding encoding, string url = null) {
            StringBuilder bulider = new StringBuilder();
            bulider.Append('{');
            foreach (var kv in args) {
                bulider.Append('\"');
                bulider.Append(kv.Key);
                bulider.Append("\":");
                bulider.Append(kv.Value);
                bulider.Append(',');
            }
            bulider.Remove(bulider.Length - 1, 1);
            bulider.Append('}');
            return bulider.ToString();
        }
    }
}
