using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///优惠券权限表
    // </summary>	

    [SugarTable("CouponRole")]
    public partial class CouponRole
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>

        public int CouponId { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>

        public int ShopId { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public int GoodsClassId { get; set; } = 0;
        /// <summary>
        /// 状态 0-正常 99-删除
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime UpdateTime { get; set; } = DateTime.Now;

    }
    /// <summary>
    ///优惠券权限表
    // </summary>	

    public partial class CouponRoleView : CouponRole
    {
    }

    [Mapper]
    public partial class CouponRoleMapper
    {
        public partial CouponRoleView ToView(CouponRole model);
        public partial List<CouponRoleView> ToViewList(List<CouponRole> list);
        public partial CouponRole ToModel(CouponRoleView model);
    }
}

