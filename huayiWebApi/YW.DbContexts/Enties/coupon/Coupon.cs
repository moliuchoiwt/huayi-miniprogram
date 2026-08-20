using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///优惠券表
    // </summary>	

    [SugarTable("Coupon")]
    public partial class Coupon
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 优惠券类型 0-满减 1-打折
        /// </summary>

        public int couponType { get; set; }

        /// <summary>
        /// 满多少
        /// </summary>

        public decimal startAmount { get; set; }

        /// <summary>
        /// 优惠金额/折扣
        /// </summary>

        public decimal discount { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 已派发数量
        /// </summary>

        public int distributeNum { get; set; }
        /// <summary>
        /// 有效天数（领取时间）
        /// </summary>
        public int dayTime { get; set; }

        /// <summary>
        /// 状态 0-正常 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 创建类型 0-平台 1-店铺
        /// </summary>

        public int createType { get; set; }

        /// <summary>
        /// 创建对象ID
        /// </summary>

        public int createId { get; set; }

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
    ///优惠券表
    // </summary>	

    public partial class CouponView : Coupon
    {
        /// <summary>
        /// 分类ID集合
        /// </summary>
        public List<int> goodsClassIdsList { get; set; } = new List<int>();
    }

    public class GiveCouponQuery
    {
        /// <summary>
        /// 优惠券id
        /// </summary>
        public int couponId { get; set; } = 0;

        /// <summary>
        /// 发放类型 0.全部用户 1.一级会员用户 2.二级会员用户
        /// </summary>
        public int giveType { get; set; } = -1;
        /// <summary>
        /// 发放的用户id
        /// </summary>
        public List<int> userIds { get; set; }

    }

    public class couponQuery : QueryModel
    {
        public int? status { get; set; }

    }

    [Mapper]
    public partial class CouponMapper
    {
        public partial CouponView ToView(Coupon model);
        public partial List<CouponView> ToViewList(List<Coupon> list);
        public partial Coupon ToModel(CouponView model);
    }

}

