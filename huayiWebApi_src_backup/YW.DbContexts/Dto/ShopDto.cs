using System.Collections.Generic;

namespace YW.DbContexts.Dto
{
    public class ShopDto : Shop
    {
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string>  imgList { get; set; }
    }
}
