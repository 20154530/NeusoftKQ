using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebAccessorCore.Utils {
    /// <summary>
    /// 格式化Json串
    /// </summary>
    public class JsonForamtor {
        /// <summary>
        /// 单例
        /// </summary>
        private static Lazy<JsonForamtor> _singleton = new Lazy<JsonForamtor>();
        public static JsonForamtor Singleton {
            get { return _singleton.Value; }
        }

        /// <summary>
        /// Tab距离
        /// </summary>
        public int TabSize { get; set; }

        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="json">json原串</param>
        /// <returns></returns>
        public string Format(string json) {
            var jsonarr = json.ToCharArray().Reverse();
            Stack<char> jsonstack = new Stack<char>(jsonarr);
            int level = 0;
            bool next = false;
            bool right = false;
            MatchCollection mac = Regex.Matches(json, @"(""[^,]+?"")\s*?:");

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < jsonarr.Count(); i++) {
                var k = jsonstack.Pop();
                switch (k) {
                    case '{':
                        level++;
                        next = true;
                        break;
                    case '}':
                        level--;
                        right = true;
                        break;
                    case '[':
                        next = true;
                        break;
                    case ']':
                        next = true;
                        break;
                    case '\n':
                        continue;
                    case '\r':
                        continue;
                    case ' ':
                        continue;
                    case ',':
                        next = true;
                        break;
                }
                sb.Append(k);
                if (next) {
                    sb.Append('\n').Append(' ', TabSize * level);
                    next = false;
                } else if (right) {
                    char c = sb[sb.Length - TabSize * (1 + level) - 3];
                    if (c != ']' && c != '}') {
                        sb.Insert(sb.Length - 1, '\n');
                        sb.Insert(sb.Length - 1, " ", TabSize * level);
                    } else
                        sb.Remove(sb.Length - TabSize - 1, TabSize);
                    right = false;
                }
            }
            return sb.ToString();
        }

        public JsonForamtor() { TabSize = 4; }
    }
}
