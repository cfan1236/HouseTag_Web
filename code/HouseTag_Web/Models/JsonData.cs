using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseTag_Web.Models
{
    public class JsonData<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public T data { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get; set; }
    }
}
