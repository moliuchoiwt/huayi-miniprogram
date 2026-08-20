using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    /// 
    // </summary>	

    [SugarTable("UserGrade")]
    public partial class UserGrade
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// 图片路径
        /// </summary>
        public string imgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 权益列表
        /// </summary>
        public string quanyiJson { get; set; } = string.Empty;

        /// <summary>
        /// 权益规则
        /// </summary>
        public string contents { get; set; } = string.Empty;
        /// <summary>
        /// 有效天数（购买时间）
        /// </summary>
        public int effectiveDays { get; set; } = 0;

        /// <summary>
        /// 级别
        /// </summary>
        public int jibie { get; set; } = 0;
        /// <summary>
        /// 状态
        /// </summary>
        public int status { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public DateTime createTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
        public DateTime updateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 赠送优惠券的id
        /// </summary>
        public string giveAwayCouponIds { get; set; } = string.Empty;
        /// <summary>
        /// 折扣比例
        /// </summary>
        public decimal discount { get; set; } = 0M;

        /// <summary>
        /// 直推人数
        /// </summary>
        public int oneRecommendNum { get; set; } = 0;

        /// <summary>
        /// 个人生活区消费
        /// </summary>
        public decimal consume1 { get; set; } = 0M;

        /// <summary>
        /// 团队生活区消费
        /// </summary>
        public decimal teamConsume1 { get; set; } = 0M;
        /// <summary>
        /// 团队佣金比例
        /// </summary>
        public decimal teamCommissionRatio { get; set; } = 0M;

    }

    public partial class UserGradeView : UserGrade
    {
        public List<string> giveAwayCouponIdList { get; set; }

    }

    [Mapper]
    public partial class UserGradeMapper
    {
        public partial UserGradeView ToView(UserGrade model);
        public partial List<UserGradeView> ToViewList(List<UserGrade> list);
        public partial UserGrade ToModel(UserGradeView model);
    }
}