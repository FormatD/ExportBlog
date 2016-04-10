using System;
using System.Collections.Generic;

using System.Text;
using System.Text.RegularExpressions;

namespace ExportBlog
{
    internal class CnBlogsService : IFeedService
    {
        string url = null;
        Regex reg_title = new Regex(@"href=""(http://www\.cnblogs\.com/.+?/(p)/.+?)"">([^<]+?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        Regex reg_title2 = new Regex(@"<a id=""ctl01_lnkTitle""[^>]*?>(.+?)</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        Regex reg_con = new Regex(@"<div id=""cnblogs_post_body"">([\s\S]+)</div><div id=""MySignature"">", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        WebUtility web = null;

        public CnBlogsService(string un)
        {
            url = "http://www.cnblogs.com/" + un + "/default.aspx?page={0}&onlytitle=1";

            web = new WebUtility();
        }

        public IList<FeedEntity> GetList()
        {
            var list = new List<FeedEntity>();

            int totalPageCount = 0;
            for (int i = 1; i < 1000; i++)
            {
                if (totalPageCount > 0 && i > totalPageCount) break;
                web.URL = string.Format(url, i);
                string html = web.Get();
                if (totalPageCount == 0)
                {
                    var mp = Regex.Match(html, @"共(\d+)页");
                    if (mp.Success)
                        totalPageCount = App.ToInt(mp.Groups[1].Value);
                    else if (i > 1) // 如果在除首页外的其他页也没有找到分页控件。。。这可能是不存在的情况，。则就认为到此页就停了 
                        totalPageCount = i;
                }
                var mats = reg_title.Matches(html);
                if (mats.Count == 0) break;
                foreach (Match mat in mats)
                {
                    var fd = new FeedEntity();
                    fd.Url = mat.Groups[1].Value.Split('#')[0].Split('?')[0].ToLower();
                    fd.Title = mat.Groups[3].Value;
                    if (fd.Url.Split('/')[3] != url.Split('/')[3])
                    {
                        continue;
                    }
                    if (fd.Title.StartsWith("[转]"))
                    {
                        fd.Title = fd.Title.Substring(3);
                    }
                    bool has = false;
                    foreach (var fe in list)
                    {
                        if (fe.Url == fd.Url || fe.Title == fd.Title)
                        {
                            has = true;
                            break;
                        }
                    }
                    if (has) continue;

                    list.Add(fd);
                }
            }
            return list;
        }

        public bool GetContent(ref FeedEntity entity)
        {
            web.URL = entity.Url;
            string html = web.Get();
            Match mat = reg_con.Match(html);
            if (mat.Success)
            {
                entity.Content = mat.Groups[1].Value.Trim();
            }
            return mat.Success;
        }
        public FeedEntity GetEntity(string url)
        {
            var entity = new FeedEntity();

            web.URL = url;
            string html = web.Get();
            Match mat = reg_title2.Match(html);
            if (mat.Success)
            {
                entity.Title = mat.Groups[1].Value.Trim();
            }
            mat = reg_con.Match(html);
            if (mat.Success)
            {
                entity.Content = mat.Groups[1].Value.Trim();
            }
            return entity;
        }
    }
}
