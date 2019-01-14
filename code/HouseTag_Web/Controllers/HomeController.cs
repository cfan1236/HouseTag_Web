using HouseTag_Web.IService;
using HouseTag_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace HouseTag_Web.Controllers
{
    public class HomeController : BaseController
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<string, List<CommentInfo>> _dic_tag = new Dictionary<string, List<CommentInfo>>();
        private static List<CommentInfo> _all_comment = new List<CommentInfo>();
        private static List<string> _city_list = new List<string>();
        private readonly AppSettings _appSettings;
        private readonly IMainService _mainService;

        public HomeController(IOptions<AppSettings> setting, IMainService mainService,
            IProxyService proxyService)
        {
            _appSettings = setting.Value;
            _mainService = mainService;
        }
        public IActionResult Index()
        {
            //debug 记录访问量
            _logger.Debug($"{GetIP()} 访问");
            var cityData = _mainService.GetCityList();
            _city_list = cityData;
            ViewData["city"] = cityData;
            return View();
        }
        /// <summary>
        /// 楼盘搜索
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public JsonResult ProjectSearch(int cityId, string text)
        {
            var jdata = new JsonData<SearchResult>();
            var result = _mainService.ProjectSearch(cityId, text = text.Trim());
            jdata.status = 1;
            jdata.data = result;
            return new JsonResult(jdata);
        }
        /// <summary>
        /// 获取楼盘信息
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult GetProjectInfo(int cityId, string name)
        {
            var jdata = new JsonData<ProjectInfo>();
            var pInfo = _mainService.GetProjectInfo(cityId, name);
            if (pInfo != null)
            {
                //记录用户成功访问的楼盘。后期可以做个最热们楼盘展示
                //TODO 后期将数据存储到mongodb 以便后期使用数据
                var cityName = _city_list[cityId].Split(',')[0];
                _logger.Info($"{GetIP()} 城市:{cityName} 楼盘:{pInfo.name} 价格:{pInfo.price}");
                if (pInfo.address.Length > 20)
                {
                    pInfo.address = pInfo.address.Substring(0, 20) + "...";
                }
            }
            jdata.status = 1;
            jdata.data = pInfo;
            return new JsonResult(jdata);
        }
        /// <summary>
        /// 获取标签信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult GetTagInfo(string name)
        {
            var jdata = new JsonData<List<TagInfo>>();
            _dic_tag = _mainService.GetCommentInfo(name, out _all_comment);
            List<TagInfo> tinfo = new List<TagInfo>();
            if (_dic_tag != null)
            {
                _dic_tag = _dic_tag.OrderByDescending(p => p.Value.Count).ToDictionary(p => p.Key, o => o.Value);
                int minCount = _appSettings.minComment;
                foreach (var item in _dic_tag)
                {
                    var count = item.Value.Count;
                    if (count < minCount)
                    {
                        continue;
                    }
                    tinfo.Add(new TagInfo()
                    {
                        name = item.Key,
                        count = count
                    });
                }
            }
            jdata.status = 1;
            jdata.data = tinfo;
            return new JsonResult(jdata);
        }
        /// <summary>
        /// 获取评论信息
        /// </summary>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public JsonResult GetCommentInfo(string TagName)
        {
            var jdata = new JsonData<List<CommentInfo>>();
            var list = new List<CommentInfo>();
            _dic_tag.TryGetValue(TagName, out list);
            jdata.status = 1;
            jdata.data = list;
            return new JsonResult(jdata);
        }
        /// <summary>
        /// 搜索评论
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public JsonResult SearchComment(string text)
        {
            var jdata = new JsonData<List<CommentInfo>>();
            var list = new List<CommentInfo>();
            if (!string.IsNullOrEmpty(text))
            {
                //显示所有评论
                if (text == "all_c")
                {
                    if (_all_comment.Count > 1500)
                    {
                        list = _all_comment.Take(1500).ToList();
                    }
                    else
                    {
                        list = _all_comment;
                    }
                }
                else
                {
                    foreach (var item in _all_comment)
                    {
                        if (item.content.Contains(text))
                        {
                            list.Add(item);
                        }
                    }
                }

            }

            jdata.status = 1;
            jdata.data = list;
            return new JsonResult(jdata);
        }
    }
}
