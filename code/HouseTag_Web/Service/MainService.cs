using HouseTag_Web.IService;
using HouseTag_Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tag_Web;

namespace HouseTag_Web.Service
{
    public class MainService : IMainService
    {
        //读取的数据文件
        private List<string> city_url = new List<string>();
        private List<string> tag_dic = new List<string>();
        private List<string> tag_dic_ignore = new List<string>();
        private List<string> author_ignore = new List<string>();
        //获取到的楼盘信息
        private ProjectInfo ank_info = null;
        private ProjectInfo ftx_info = null;
        //选择的城市url
        private string url_ajk;
        private string url_ftx;
        //安居客和房天下service
        private readonly IHouseAjkService _houseAjkService;
        private readonly IHouseFtxService _houseFtxService;
        public MainService(IHouseAjkService houseAjkService, IHouseFtxService houseFtxService)
        {
            _houseAjkService = houseAjkService;
            _houseFtxService = houseFtxService;

            #region 初始化数据文件
            var content = OpenFile("city_url.txt");
            city_url = new List<string>(content);

            content = OpenFile("tag_dict.txt");
            tag_dic = new List<string>(content);

            content = OpenFile("dic_ignore.txt");
            tag_dic_ignore = new List<string>(content);

            content = OpenFile("author_ignore.txt");
            author_ignore = new List<string>(content);
            #endregion
        }

        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetCityList()
        {
            return city_url;
        }
        /// <summary>
        /// 获取楼盘信息
        /// </summary>
        /// <param name="index_city"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ProjectInfo GetProjectInfo(int index_city, string name)
        {
            ProjectInfo p_info = null;
            var url_city = city_url[index_city].Split(',');
            url_ajk = url_city[1];
            url_ftx = url_city[2];
            Task[] tk = new Task[2];
            tk[0] = Task.Factory.StartNew(() =>
            {
                var ank_list = _houseAjkService.GetProjectInfo(url_ajk, name);
                if (ank_list != null && ank_list.Count > 0)
                {
                    //寻找最匹配项
                    foreach (var item in ank_list)
                    {
                        if (item.name == name)
                        {
                            ank_info = item;
                            break;
                        }
                    }
                    //如果没有完全匹配取第一个
                    if (ank_info == null)
                    {
                        ank_info = ank_list[0];
                    }
                }
            });
            tk[1] = Task.Factory.StartNew(() =>
            {
                var ftx_list = _houseFtxService.GetProjectInfo(url_ftx, name);
                if (ftx_list != null && ftx_list.Count > 0)
                {
                    //寻找最匹配项
                    foreach (var item in ftx_list)
                    {
                        //房天下数据某些楼盘名称可能出现住宅两个字
                        item.name = item.name.Replace("住宅", "");
                        if (item.name == name)
                        {
                            ftx_info = item;
                            break;
                        }
                    }
                    //如果没有完全匹配取第一个
                    if (ank_info == null)
                    {
                        ftx_info = ftx_list[0];
                    }

                }
            });
            int timeout = 1000 * 7;
            Task.WaitAll(tk, timeout);

            if (ank_info == null && ftx_info == null)
            {
                return null;
            }
            else
            {
                if (ank_info != null)
                {
                    p_info = ank_info;
                    //如果安居客价格没有则看看房天下是否有价格数据
                    if (string.IsNullOrEmpty(ank_info.price))
                    {
                        if (ftx_info != null)
                        {
                            p_info.price = ftx_info.price;
                        }
                    }
                }
                else
                {
                    p_info = ftx_info;
                }
                p_info.price = p_info.price.Replace("均价", "").Replace("价格待定", "").Replace("广告", "");
                return p_info;
            }
        }
        /// <summary>
        /// 获取评论信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="allComment"></param>
        /// <returns></returns>
        public Dictionary<string, List<CommentInfo>> GetCommentInfo(string name, out List<CommentInfo> allComment)
        {
            var dic_tag = new Dictionary<string, List<CommentInfo>>();
            var list_allComment = new List<CommentInfo>();
            Task[] tk = new Task[2];
            //获取安居客的评论数据
            tk[0] = Task.Factory.StartNew(() =>
            {
                if (ank_info != null && !string.IsNullOrWhiteSpace(ank_info.id))
                {
                    var list_comment = _houseAjkService.GetProjectCommenInfo(url_ajk, ank_info.id);
                    list_allComment.AddRange(list_comment);
                }
            });
            //获取房天下的评论数据
            tk[1] = Task.Factory.StartNew(() =>
            {
                bool flag = true;
                if (ank_info != null && ftx_info != null)
                {
                    //如果两个网站返回的楼盘名称不相同则证明不是同一个楼盘 试着添加住宅两个字
                    if (ank_info.name != ftx_info.name)
                    {
                        //添加住宅关键字 重新尝试获取楼盘信息 （*房天下部分楼盘名称直接带上了住宅）
                        var ftx_list = _houseFtxService.GetProjectInfo(url_ftx, name + "住宅");
                        if (ftx_list != null && ftx_list.Count > 0)
                        {
                            ftx_info = ftx_list[0];
                        }
                        if (ank_info.name != ftx_info.name.Replace("住宅", ""))
                        {
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    if (ftx_info != null && !string.IsNullOrWhiteSpace(ftx_info.id) && !string.IsNullOrWhiteSpace(ftx_info.url))
                    {
                        var list_comment = _houseFtxService.GetProjectCommenInfo(ftx_info.url, ftx_info.id, ftx_info.commentCount);
                        list_allComment.AddRange(list_comment);
                    }
                }
            });
            //6min
            int timeout = (1000 * 60) * 6;
            Task.WaitAll(tk, timeout);
            allComment = list_allComment;
            dic_tag = GetTag(list_allComment, name);
            //显示标签
            return dic_tag;

        }

        /// <summary>
        /// 楼盘搜索
        /// </summary>
        /// <param name="index_city"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public SearchResult ProjectSearch(int index_city, string text)
        {
            SearchResult result = new SearchResult();
            var url = city_url[index_city].Split(',')[1];
            result = _houseAjkService.SearchHouse(url, text);
            return result;
        }

        /// <summary>
        /// 按行读取文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string[] OpenFile(string fileName)
        {
            //运行目录 路径尽量使用/并且注意大小写 linux大小写很敏感
            var runPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var filePath = runPath + "/Data/" + fileName;
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath, Encoding.UTF8);
            }
            else
            {
                throw new FileNotFoundException("找不到文件:" + fileName);
            }
        }

        /// <summary>
        /// 获取标签数据
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        private Dictionary<string, List<CommentInfo>> GetTag(List<CommentInfo> comment, string pName)
        {
            var dic_result = new Dictionary<string, List<CommentInfo>>();
            if (comment != null)
            {
                foreach (var item_c in comment)
                {
                    //评论作者
                    if (FilterAuthor(item_c.author, pName))
                    {
                        continue;
                    }
                    var content = item_c.content;
                    //排除忽略关键字
                    foreach (var item_ig in tag_dic_ignore)
                    {
                        content = content.Replace(item_ig, "");
                    }
                    //开始匹配关键词
                    foreach (var item_dic in tag_dic)
                    {
                        var dic_arr = item_dic.Split(':');
                        var dic_text = dic_arr[0];
                        if (dic_arr.Length > 1)
                        {
                            //拆分近义词
                            var dic_synonym = dic_arr[1].Split(',');
                            for (int i = 0; i < dic_synonym.Length; i++)
                            {
                                content = content.Replace(dic_synonym[i], dic_text);
                            }
                        }
                        if (content.Contains(dic_text))
                        {
                            if (dic_result.ContainsKey(dic_text))
                            {
                                var key_list = dic_result[dic_text];
                                key_list.Add(item_c);
                            }
                            else
                            {
                                dic_result.Add(dic_text, new List<CommentInfo>() { (item_c) });
                            }
                        }
                    }

                }
            }
            return dic_result;
        }

        /// <summary>
        /// 过滤用户 代抢、置业顾问等
        /// </summary>
        /// <param name="author"></param>
        /// <param name="pName"></param>
        /// <returns></returns>
        private bool FilterAuthor(string author, string pName)
        {
            author = author.ToUpper();
            //排除掉当前职业顾问的评论数据
            if (author.Contains(pName))
            {
                return true;
            }
            //按照作者关键字进行过滤
            foreach (var item_ig in author_ignore)
            {
                var keyword = item_ig.Split('-');
                if (keyword.Length > 1)
                {
                    var str = keyword[0];
                    //先判断作者是否包含过滤关键字 比如代抢、微信等 
                    if (author.Contains(str.ToUpper()))
                    {
                        //包含后 作者名称一般会有微信号码 通过正则提取是否包含字母和数字
                        var n = Regex.Matches(author, "[a-zA-Z0-9]").Count;
                        //微信号长度 或者qq号
                        if (n > 5)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (author.Contains(keyword[0].ToUpper()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
