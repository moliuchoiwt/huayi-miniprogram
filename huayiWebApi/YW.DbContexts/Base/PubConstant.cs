namespace YW.DbContexts
{
    /// <summary>
    /// 获取固定连接字符串
    /// </summary>
    public class PubConstant
    {
        /// <summary>
        /// 获取固定连接字符串
        /// </summary>
        public static string ConnectionString { get; set; }

        public static ApiConfigDto Config { get; set; }


        public static IHttpContextAccessor Accessor { get; set; }
    }

    /// <summary>
    /// 文件链接处理
    /// </summary>
    public class WebFileHelper
    {


        #region 链接地址处理


        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        public static string GetUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {
            string url = "";
            if (!string.IsNullOrWhiteSpace(imgPath))
            {
                var list = GetListUrl(imgPath, widthPic, heightPic);
                if (list != null && list.Count > 0)
                {
                    url = list.FirstOrDefault();
                }
            }
            return url;
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        public static List<string> GetListUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {
            List<string> urlList = new List<string>();
            if (!string.IsNullOrWhiteSpace(imgPath))
            {
                foreach (var item in imgPath.Split(','))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!item.Contains("http"))
                        {
                            var dirTempPath = Common.CommonHelper.GetMapPath(item);//文件路径
                            if (Common.DirFile.IsExistFile(dirTempPath))
                            {
                                //if (widthPic <= 0)
                                //{
                                urlList.Add(PubConstant.Config.DomianStaticName + item);
                                //}
                                //else
                                //{
                                //    if (item.Contains(".jpg") || item.Contains(".jepg") || item.Contains(".png") || item.Contains(".gif"))
                                //    {

                                //        var imaArr = item.Split('.');
                                //        var thumbnailPath = Common.CommonHelper.GetMapPath($"{imaArr[0]}-{widthPic}.{imaArr[1]}");
                                //        //生成缩略图
                                //        if (!Common.DirFile.IsExistFile(thumbnailPath)) Common.ImageClass.MakeThumbnail(dirTempPath, thumbnailPath, widthPic, heightPic, "HW");
                                //        urlList.Add(PubConstant.Config.DomianStaticName + $"{imaArr[0]}-{widthPic}.{imaArr[1]}");
                                //    }
                                //}
                            }
                            //else urlList.Add("");
                        }
                        else
                            urlList.Add(item);
                    }
                }
            }
            return urlList;
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetListUrl(List<string> imgList)
        {
            List<string> urlList = new List<string>();
            if (imgList != null && imgList.Count > 0)
            {
                foreach (var item in imgList)
                {
                    if (!item.Contains("http"))
                        urlList.Add(PubConstant.Config.DomianStaticName + item);
                    else
                        urlList.Add(item);
                }
            }
            return urlList;
        }
        #endregion

        #region 内容详情中链接处理
        /// <summary>
        /// 内容详情中图片处理
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string getContent(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                if (content.Contains("src=\"/Upload/"))
                {
                    content = content.Replace("src=\"/Upload/", "src=\"" + PubConstant.Config.DomianStaticName + "/Upload/");
                }
                //else if (content.Contains("src=\"/upload/"))
                //{
                //    content = content.Replace("src=\"/upload/", "src=\"" + PubConstant.Config.DomianStaticName + "/upload/");
                //}
                //if (content.Contains("<img"))
                //{
                //    content = content.Replace("<img", "<img style=\"max-width:100%; height: auto\"");
                //}
            }
            return content;
        }
        #endregion

    }

}
