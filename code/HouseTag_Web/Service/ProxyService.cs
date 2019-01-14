using Common;
using HouseTag_Web.IService;
using HouseTag_Web.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HouseTag_Web.Service
{
    public class ProxyService : IProxyService
    {
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _memoryCache;
        private ConcurrentDictionary<Proxy, bool> _dic_proxy = new ConcurrentDictionary<Proxy, bool>();
        private ConcurrentDictionary<string, bool> _dicInvaildProxy = new ConcurrentDictionary<string, bool>();

        public ProxyService(IOptions<AppSettings> setting, IMemoryCache memoryCache)
        {
            _appSettings = setting.Value;
            _memoryCache = memoryCache;
            if (_appSettings.proxyEnabled)
            {
                InitProxy();
            }
        }
        /// <summary>
        /// 初始化代理数据
        /// </summary>
        private void InitProxy()
        {
            //异步去初始化代理数据 避免对主线程造成阻塞
            new Task(() =>
            {
                GetNewProxy();
                RefreshProxy();

            }).Start();
        }

        /// <summary>
        /// 刷新代理 实时检查ip是否有效 免费代理ip有效性很不稳定
        /// </summary>
        public void RefreshProxy()
        {
            while (true)
            {
                if (_dic_proxy.Count > 0)
                {
                    Task[] tk = new Task[_dic_proxy.Count];
                    int n = 0;
                    foreach (var item in _dic_proxy)
                    {
                        tk[n] = new Task(() =>
                          {
                              //无效则剔除该ip
                              if (!checkIP(item.Key))
                              {
                                  _dic_proxy.TryRemove(item.Key, out bool flag);
                              }
                          });
                        tk[n].Start();
                        n++;
                    }
                    //超时4min
                    Task.WaitAll(tk, (1000 * 60) * 4);
                    if (_dic_proxy.Count <= 8)
                    {
                        GetNewProxy();
                    }
                }

                //1分钟刷新一次
                Thread.Sleep(1000 * 60);

            }

        }
        /// <summary>
        /// 获取最新代理
        /// </summary>
        private void GetNewProxy()
        {
            //从文件中获取代理数据
            var p_list = GetProxyData();
            if (p_list != null && p_list.Count > 0)
            {
                //检查ip的有效性并添加到字典中
                foreach (var item in p_list)
                {

                    Proxy p = new Proxy()
                    {
                        ip = item.ip,
                        port = item.port
                    };
                    //判断是否在无效列表中
                    if (!_dicInvaildProxy.ContainsKey(p.ip + ":" + p.port))
                    {
                        if (checkIP(p))
                        {
                            _dic_proxy.TryAdd(p, true);
                        }
                        else
                        {
                            //记录无效ip 避免下次从文件中获取时碰到相同的ip
                            _dicInvaildProxy.TryAdd(p.ip + ":" + p.port, false);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取代理数据
        /// </summary>
        /// <returns></returns>
        private List<Proxy> GetProxyData()
        {
            //读取代理ip文件 由另个一个程序定时更新此文
            //文件格式如:127.0.0.1,8080
            var cache_key = "proxy_data";
            var p_list = _memoryCache.Get<List<Proxy>>(cache_key);
            if (p_list == null || p_list.Count == 0)
            {
                var filePath = _appSettings.proxyPath;

                if (File.Exists(filePath))
                {
                    p_list = new List<Proxy>();
                    var content = File.ReadAllLines(filePath);
                    foreach (var item in content)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            string ip = item.Split(',')[0];
                            int port = 0;
                            int.TryParse(item.Split(',')[1], out port);
                            p_list.Add(new Proxy()
                            {
                                ip = ip,
                                port = port
                            });
                        }
                    }
                    _memoryCache.Set<List<Proxy>>(cache_key, p_list, TimeSpan.FromMinutes(_appSettings.proxyCacheTime));
                }
            }
            return p_list;
        }
        /// <summary>
        /// 获取一个ip
        /// </summary>
        /// <returns></returns>
        public Proxy GetProxy()
        {
            Proxy p = new Proxy();
            List<Proxy> list_proxy = _dic_proxy.Keys.ToList();
            if (list_proxy.Count > 0)
            {
                //随机获取一个代理IP
                Random rand = new Random();
                var n = rand.Next(0, list_proxy.Count);
                p = list_proxy[n];
            }
            return p;
        }

        /// <summary>
        /// 检查ip是否有效
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool checkIP(Proxy p)
        {
            var flag = false;
            try
            {
                var html = NetHttpHelper.HttpGetRequest("https://www.baidu.com", out int status, 2000, p.ip, p.port);
                if (status == 200)
                {
                    if (!html.Contains("无效"))
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return flag;
        }

        /// <summary>
        /// 添加无效ip 即当前ip已经被服务器进行封锁
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool AddInvalidIp(string ip)
        {
            var flag = _memoryCache.Set<bool>(ip, true, TimeSpan.FromDays(1));
            return flag;
        }


    }
}
