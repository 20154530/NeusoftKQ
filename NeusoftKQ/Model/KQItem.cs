using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeusoftKQ.Model {

    /// <summary>
    /// 考勤时间
    /// </summary>
    public class KQItem {

        /// <summary>
        /// 考勤的用户
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
