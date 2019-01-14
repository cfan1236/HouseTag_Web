using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseTag_Web.Models
{
    /// <summary>
    /// 标签信息
    /// </summary>
    public class TagInfo
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 标签个数
        /// </summary>
        public int count { get; set; }
    }
}
