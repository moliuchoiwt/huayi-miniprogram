using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///点赞记录
    // </summary>	

    [SugarTable("CollectionRecord")]
    public partial class CollectionRecord
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0-商品 
        /// </summary>

        public int cType { get; set; }

        /// <summary>
        /// 收藏对象Id
        /// </summary>

        public int cId { get; set; }



        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }


        /// <summary>
        /// 状态 0-取消收藏  1-已收藏 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 阅读状态 0-未读  1-已读 99-删除
        /// </summary>

        public int readState { get; set; }

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
    ///点赞记录
    // </summary>	

    public partial class CollectionRecordView : CollectionRecord
    {

        /// <summary>
        /// 收藏对象标题
        /// </summary>

        public string name { get; set; }

        /// <summary>
        /// 封面图片
        /// </summary>
        public string coverImage { get; set; } = string.Empty;

        /// <summary>
        /// 价格
        /// </summary>
        public decimal price { get; set; } = 0M;

    }

    [Mapper]
    public partial class CollectionRecordMapper
    {
        public partial CollectionRecordView ToView(CollectionRecord model);
        public partial List<CollectionRecordView> ToViewList(List<CollectionRecord> list);
        public partial CollectionRecord ToModel(CollectionRecordView model);
    }
}

