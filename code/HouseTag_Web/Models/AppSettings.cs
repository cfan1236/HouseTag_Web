namespace HouseTag_Web.Models
{
    public class AppSettings
    {
        /// <summary>
        /// 最小评论数据
        /// </summary>
        public int minComment { get; set; }

        /// <summary>
        /// 代理是否启用
        /// </summary>
        public bool proxyEnabled { get; set; }
        /// <summary>
        /// 代理ip路径
        /// </summary>
        public string proxyPath { get; set; }

        /// <summary>
        /// 代理缓存时间
        /// </summary>
        public int proxyCacheTime { get; set; }
    }
}
