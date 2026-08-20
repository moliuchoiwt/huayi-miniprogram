namespace YW.DbContexts
{
    /// <summary>
    /// 基础查询字段
    /// </summary>
    public class QueryModel
    {  /// <summary>
       /// 页码
       /// </summary>
        public int pageNum { get; set; } = 1;
        /// <summary>
        /// 页面条数
        /// </summary>
        public int pageSize { get; set; } = 20;
        /// <summary>
        /// 搜索Id
        /// </summary>
        public int? queryId { get; set; }
        /// <summary>
        /// 搜索用户Id
        /// </summary>
        public int? userId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? orderBy { get; set; }
        /// <summary>
        /// Id集合
        /// </summary>
        public List<int> Ids { get; set; }
        /// <summary>
        /// 搜索字段
        /// </summary>
        public string queryName { get; set; }
        /// <summary>
        /// 搜索类型
        /// </summary>
        public int? queryType { get; set; }
        /// <summary>
        /// 父级Id
        /// </summary>
        public int? parentId { get; set; }
        /// <summary>
        /// 搜索状态
        /// </summary>
        public int? queryState { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateOnly? startDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateOnly? endDate { get; set; }

        /// <summary>
        /// 年
        /// </summary>
        public int? queryYear { get; set; }
        /// <summary>
        /// 月
        /// </summary>
        public int? queryMonth { get; set; }
        /// <summary>
        /// 指定日期
        /// </summary>
        public DateTime? myDate { get; set; }


        /// <summary>
        /// 搜索多个状态
        /// </summary>
        public List<int> queryStateArr { get; set; }
        /// <summary>
        /// 搜索多个类型
        /// </summary>
        public List<int> queryTypeArr { get; set; }

        /// <summary>
        /// 是否关注 1-是
        /// </summary>
        public int? isFollow { get; set; }
        /// <summary>
        /// 是否点赞 1-是
        /// </summary>
        public int? isLike { get; set; }
        /// <summary>
        /// 是否收藏 1-是
        /// </summary>
        public bool? isCollection { get; set; }

        /// <summary>
        /// 是否推荐 1-是
        /// </summary>
        public int? isTop { get; set; }

        public int? channelId { get; set; }
        public int? categoryId { get; set; }

        /// <summary>
        /// 店铺id
        /// </summary>
        public int? ShopId { get; set; }

        /// <summary>
        /// 订单号集合
        /// </summary>
        public List<string> noArr { get; set; }

    }


    public class GoodsQuery : QueryModel
    {
        /// <summary>
        /// 商品分类
        /// </summary>
        public int? classId { get; set; }
        /// <summary>
        ///是否爆款
        /// </summary>
        public int? IsHot { get; set; }
        /// <summary>
        ///是否新品
        /// </summary>
        public int? IsNew { get; set; }
        /// <summary>
        /// 是否首页推荐
        /// </summary>
        public bool? isIndex { get; set; }

        /// <summary>
        /// 开始金额
        /// </summary>
        public decimal? startPrice { get; set; }
        /// <summary>
        /// 结束金额
        /// </summary>
        public decimal? endPrice { get; set; }
        /// <summary>
        /// 商品类型
        /// </summary>
        public int? gType { get; set; }
        public int? status { get; set; }

    }
    public class DelModel
    {
        public int[] ids { get; set; }
    }


    public class IndexQuery : QueryModel
    {

    }



    public class AuditModel
    {
        /// <summary>
        /// 选中的Id
        /// </summary>
        public List<int> ids { get; set; }
        /// <summary>
        /// 审核状态
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 审核信息
        /// </summary>
        public string auditInfo { get; set; }
    }

    public class OrderQuery : QueryModel
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 是否支付
        /// </summary>
        public bool? isPay { get; set; } = false;

    }
}
