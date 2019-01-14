using HouseTag_Web.Models;
using System.Collections.Generic;

namespace HouseTag_Web.IService
{
    public interface IMainService
    {

        /// <summary>
        /// 获取城市列表
        /// </summary>
        /// <returns></returns>
        List<string> GetCityList();
        /// <summary>
        /// 获取楼盘信息
        /// </summary>
        /// <param name="index_city"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        ProjectInfo GetProjectInfo(int index_city, string name);

        /// <summary>
        /// 获取评论信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="allComment"></param>
        /// <returns></returns>
        Dictionary<string, List<CommentInfo>> GetCommentInfo(string name, out List<CommentInfo> allComment);

        /// <summary>
        /// 楼盘搜索
        /// </summary>
        /// <param name="index_city"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        SearchResult ProjectSearch(int index_city, string text);

    }
}
