using Common;
using HouseTag_Web.IService;
using HouseTag_Web.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HouseTag_Web.Service
{
    public class HouseAjkService : IHouseAjkService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IProxyService _proxyService;
        private Proxy _proxy = new Proxy();
        public HouseAjkService(IProxyService proxyService)
        {
            _proxyService = proxyService;
        }
        /// <summary>
        /// 获取楼盘信息
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ProjectInfo> GetProjectInfo(string city_url, string name)
        {
            if (city_url[city_url.Length - 1] != '/')
            {
                city_url += "/";
            }
            List<ProjectInfo> list = new List<ProjectInfo>();
            var url = city_url + "loupan/s?kw=" + name;
            var html = GetHtml(url);
            if (!string.IsNullOrEmpty(html))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var node = doc.DocumentNode.SelectNodes(".//div[@class='item-mod ']");  //原网页item-mod 后有个空格 
                if (node != null)
                {
                    foreach (var item in node)
                    {
                        string pName = "";
                        var name_html = item.SelectSingleNode(".//span[@class='items-name']");
                        if (name_html != null)
                        {
                            pName = name_html.InnerText;
                        }
                        var url_html = item.SelectSingleNode(".//a[@class='address']");
                        var address = "";
                        var purl = "";
                        if (url_html != null)
                        {
                            address = url_html.InnerText.Replace("&nbsp;", " ");
                            address = address.TrimEnd().TrimStart();
                            foreach (var att in url_html.Attributes)
                            {
                                if (att.Name == "href")
                                {
                                    purl = att.Value;
                                    break;
                                }
                            }
                        }
                        var count_html = item.SelectSingleNode(".//span[@class='list-dp']");
                        int count = 0;
                        if (count_html != null)
                        {
                            var countStr = count_html.InnerHtml.Replace("条点评", "");
                            int.TryParse(countStr, out count);
                        }
                        string projectId = System.Text.RegularExpressions.Regex.Replace(purl, @"[^0-9]+", "");
                        var price_html = item.SelectSingleNode(".//p[@class='price']");
                        var price_text = "";
                        if (price_html != null)
                        {
                            price_text = price_html.InnerText;
                        }
                        list.Add(new ProjectInfo()
                        {
                            name = pName,
                            commentCount = count,
                            id = projectId,
                            url = purl,
                            address = address,
                            price = price_text

                        });

                    }
                }

            }

            return list;
        }
        /// <summary>
        /// 获取评论信息
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="pId"></param>
        /// <param name="c_count"></param>
        /// <returns></returns>
        public List<CommentInfo> GetProjectCommenInfo(string city_url, string pId, int c_count = 0)
        {
            if (city_url[city_url.Length - 1] != '/')
            {
                city_url += "/";
            }
            var cinfo = new List<CommentInfo>();
            int page = 1;
            //循环获取评论
            bool flag = true;
            while (flag)
            {
                var url = $"{city_url}loupan/dianping-{pId}.htmls/?from=commview_dp_moretop&p={page}";
                var html = GetHtml(url);
                if (!string.IsNullOrEmpty(html))
                {
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    //查找评论用户
                    var author = doc.DocumentNode.SelectNodes(".//span[@class='author']");
                    var content = doc.DocumentNode.SelectNodes(".//h4[@class='rev-subtit all-text']");
                    var date = doc.DocumentNode.SelectNodes(".//span[@class='date']");
                    if (author != null && author.Count > 0)
                    {
                        for (int i = 0; i < author.Count; i++)
                        {
                            var cstr = content[i].InnerText.Substring(0, content[i].InnerText.Length - 2);
                            cstr = cstr.Replace("&hellip;", "…").Replace("<br/>", " "); //这里不做换行处理只加空格
                            cinfo.Add(new CommentInfo()
                            {
                                author = author[i].InnerText,
                                content = cstr,
                                date = DateTime.Parse(date[i].InnerText)
                            });
                        }
                    }
                    else
                    {
                        break;
                    }
                    //查找是否还有下一页 如果没有则证明是最后一页
                    var next_page = doc.DocumentNode.SelectNodes(".//a[@class='next-page next-link']");
                    if (next_page == null || next_page.Count == 0)
                    {
                        break;
                    }
                }
                page++;
                Thread.Sleep(20);
            }


            return cinfo;
        }
        /// <summary>
        /// 楼盘搜索
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public SearchResult SearchHouse(string city_url, string text)
        {
            var url = "";
            var html = "";
            try
            {
                if (city_url[city_url.Length - 1] != '/')
                {
                    city_url += "/";
                }
                var result = new SearchResult();
                url = $"{city_url}a/brand/?kw={text}";
                html = GetHtml(url);
                if (!string.IsNullOrEmpty(html))
                {
                    return JsonConvert.DeserializeObject<SearchResult>(html);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                //异常信息先记录下请求的url以及返回的html
                _logger.Debug($"异常:{ex.Message}\r\n SearchHouse url:{url}\r\n html:{html}");
                _logger.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// 获取html数据 统一处理超时、自动换ip
        /// </summary>
        /// <param name="url">请求url</param>
        /// <param name="proxy">默认代理</param>
        /// <returns></returns>
        private string GetHtml(string url, bool usingProxy = false)
        {
            string html = "";
            if (usingProxy)
            {
                _proxy = _proxyService.GetProxy();

            }
            //单次请求ip更换次数
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    html = NetHttpHelper.HttpGetRequest(url, out int status, 5000, _proxy.ip, _proxy.port);
                    if (status == 200 && !string.IsNullOrWhiteSpace(html))
                    {
                        //证明被安居客限制了
                        if (html.Contains("<title>验证码</title>"))
                        {
                            _logger.Debug("ank_需要滑动验证");
                            //换ip
                            _proxy = _proxyService.GetProxy();
                            continue;
                        }
                        else
                        {
                            //终止循环 代表请求成功
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    //换ip
                    _proxy = _proxyService.GetProxy();
                }

            }
            return html;
        }




    }
}
