using Riok.Mapperly.Abstractions;
using YW.DbContexts.Dto;

namespace YW.DbContexts
{

    /// <summary>
    ///店铺信息
    // </summary>	

    [SugarTable("Shop")]
    public partial class Shop
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 类型 0-店铺
        /// </summary>

        public int stype { get; set; } = 0;

        /// <summary>
        /// 上级ID
        /// </summary>

        public int parentId { get; set; } = 0;

        /// <summary>
        /// 名称
        /// </summary>

        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 店铺评分
        /// </summary>
        public decimal score { get; set; } = 5M;

        /// <summary>
        /// 图标
        /// </summary>

        public string logo { get; set; } = string.Empty;

        /// <summary>
        /// 轮播图集合
        /// </summary>

        public string bannerUrls { get; set; } = string.Empty;

        /// <summary>
        /// 排序-倒序
        /// </summary>

        public int sort { get; set; } = 0;

        #region 所在地址
        /// <summary>
        /// 省
        /// </summary>

        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 市
        /// </summary>

        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区
        /// </summary>

        public string area { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>

        public string address { get; set; } = string.Empty;
        #endregion

        /// <summary>
        /// 经度
        /// </summary>

        public decimal longitude { get; set; } = 0M;

        /// <summary>
        /// 纬度
        /// </summary>

        public decimal latitude { get; set; } = 0M;

        /// <summary>
        /// 标签
        /// </summary>

        public string labels { get; set; } = string.Empty;

        /// <summary>
        /// 联系号码
        /// </summary>

        public string mobile { get; set; } = string.Empty;


        /// <summary>
        /// 营业时间
        /// </summary>

        public string times { get; set; } = string.Empty;

        /// <summary>
        /// Amount
        /// </summary>

        public decimal amount { get; set; } = 0M;

        /// <summary>
        /// 简介
        /// </summary>

        public string intro { get; set; } = string.Empty;

        /// <summary>
        /// 内容
        /// </summary>

        public string contents { get; set; } = string.Empty;

        /// <summary>
        /// 状态 0-营业中 1-已歇业
        /// </summary>

        public int status { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; } = 0;

        /// <summary>
        /// 联系人
        /// </summary>

        public string realName { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>

        public string idCard { get; set; } = string.Empty;

        /// <summary>
        /// 身份证正面
        /// </summary>

        public string idImg1 { get; set; } = string.Empty;

        /// <summary>
        /// 身份证反面
        /// </summary>

        public string idImg2 { get; set; } = string.Empty;

        /// <summary>
        /// 营业执照
        /// </summary>

        public string businessImg { get; set; } = string.Empty;
        /// <summary>
        /// 合同图片
        /// </summary>
        public string contractImg { get; set; } = string.Empty;

        /// <summary>
        /// 状态 0-待审核 1-已通过 2-已拒绝
        /// </summary>

        public int auditState { get; set; } = 0;

        /// <summary>
        /// 审核信息
        /// </summary>

        public string auditIntro { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;


    }

    /// <summary>
    ///店铺信息
    // </summary>	

    public partial class ShopView : Shop
    {
        public List<string> bannerList { get; set; }
    }

    public class ShopQuery : QueryModel
    {
        public int? auditState { get; set; }
        public int? status { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Lng { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Lat { get; set; }

        /// <summary>
        /// 销量 0.降序 1.升序
        /// </summary>
        public int? orderbySale { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Score { get; set; }
    }


    [Mapper]
    public partial class ShopMapper
    {
        public partial ShopView ToView(Shop model);
        public partial ShopDto ToDto(Shop model);
        public partial List<ShopView> ToViewList(List<Shop> list);
        public partial Shop ToModel(ShopView model);
    }

}

