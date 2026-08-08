using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///奖励配置
    // </summary>	

    [SugarTable("RewardSet")]
    public partial class RewardSet
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 奖励类型 0-注册奖励 1-推荐 2-首页
        /// </summary>

        public int RewardType { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// 图标
        /// </summary>

        public string ImgUrl { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>

        public string Intro { get; set; }

        /// <summary>
        /// 状态 99-删除
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
    ///奖励配置
    // </summary>	

    public partial class RewardSetView : RewardSet
    {

        /// <summary>
        /// 关联奖励记录
        /// </summary>

        public List<RewardRelationView> RelationList { get; set; }
        /// <summary>
        /// 领取记录
        /// </summary>

        public List<RewardReceive> ReceiveList { get; set; }

    }

    [Mapper]
    public partial class RewardSetMapper
    {
        public partial RewardSetView ToView(RewardSet model);
        public partial List<RewardSetView> ToViewList(List<RewardSet> list);
        public partial RewardSet ToModel(RewardSetView model);
    }



    /// <summary>
    ///关联奖励奖品记录
    // </summary>	

    [SugarTable("RewardRelation")]
    public partial class RewardRelation
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 奖励配置Id
        /// </summary>
        public int RewardId { get; set; }

        /// <summary>
        /// 类型 0-积分 1-优惠券 2-代金券
        /// </summary>

        public int RelationType { get; set; }

        /// <summary>
        /// 关联Id(优惠券/店铺)
        /// </summary>

        public int RelationId { get; set; }

        /// <summary>
        /// 关联标题
        /// </summary>

        public string RelationName { get; set; }

        /// <summary>
        /// 数额
        /// </summary>

        public decimal Total { get; set; }

        /// <summary>
        /// 状态 99-删除
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
    ///关联奖励记录
    // </summary>	

    public partial class RewardRelationView : RewardRelation
    {


    }



    /// <summary>
    ///奖励领取记录
    // </summary>	

    [SugarTable("RewardReceive")]
    public partial class RewardReceive
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 奖励配置Id
        /// </summary>
        public int RewardId { get; set; }
        /// <summary>
        /// 奖励类型
        /// </summary>
        public int RewardType { get; set; }
        /// <summary>
        /// 奖励标题
        /// </summary>
        public string RewardTitle { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    [Mapper]
    public partial class RewardRelationMapper
    {
        public partial RewardRelationView ToView(RewardRelation model);
        public partial List<RewardRelationView> ToViewList(List<RewardRelation> list);
        public partial RewardRelation ToModel(RewardRelationView model);
        public partial List<RewardRelation> ToModelList(List<RewardRelationView> list);

    }

}

