using HouseTag_Web.Models;
using System.Collections.Generic;

namespace HouseTag_Web.IService
{
    public interface IHouseService
    {
        /// <summary>
        /// 获取楼盘信息
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        List<ProjectInfo> GetProjectInfo(string city_url, string name);

        /// <summary>
        /// 获取评论信息
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="pId"></param>
        /// <returns></returns>
        List<CommentInfo> GetProjectCommenInfo(string city_url, string pId, int c_count = 0);

        /// <summary>
        /// 楼盘搜索
        /// </summary>
        /// <param name="city_url"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        SearchResult SearchHouse(string city_url, string text);
    }

}
