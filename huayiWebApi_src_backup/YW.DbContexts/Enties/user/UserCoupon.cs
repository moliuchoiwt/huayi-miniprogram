using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///用户优惠券表
    // </summary>	

    [SugarTable("UserCoupon")]
    public partial class UserCoupon
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int userId { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>

        public int couponId { get; set; }

        /// <summary>
        /// 优惠券标题
        /// </summary>

        public string couponTitle { get; set; }

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
        /// 摘要
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 来源类型  0-系统派发 1-用户领取 2-礼包领取 3-商城购买
        /// </summary>

        public int sourceType { get; set; }

        /// <summary>
        /// 来源关联单号
        /// </summary>

        public string sourceNo { get; set; }

        /// <summary>
        /// 状态 0-未核销 1-已核销 2-已过期 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateOnly endTime { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///用户优惠券表
    // </summary>	

    public partial class UserCouponView : UserCoupon
    {

        /// <summary>
        /// 可用分类名称
        /// </summary>
        public List<string> goodsClassNamesList { get; set; } = new List<string>();

        /// <summary>
        /// 优惠券ID集合
        /// </summary>
        public List<int> CouponIds { get; set; }
        /// <summary>
        /// 用户ID集合
        /// </summary>
        public List<int> UserIds { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public string userName { get; set; }


        /// <summary>
        /// 计算后优惠金额
        /// </summary>
        public decimal couponMoney { get; set; }

    }

    [Mapper]
    public partial class UserCouponMapper
    {
        public partial UserCouponView ToView(UserCoupon model);
        public partial List<UserCouponView> ToViewList(List<UserCoupon> list);
        public partial UserCoupon ToModel(UserCouponView model);
    }
}

