using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///分类表
    // </summary>	

    [SugarTable("Class")]
    public partial class Class
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 分类类型 0-产品 具体分类看枚举
        /// </summary>

        public int cType { get; set; }

        /// <summary>
        /// ParentId
        /// </summary>

        public int parentId { get; set; }

        /// <summary>
        /// Title
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// Intro
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// ImgUrl
        /// </summary>

        public string imgUrl { get; set; }

        /// <summary>
        /// Link
        /// </summary>

        public string link { get; set; }


        /// <summary>
        /// Sort
        /// </summary>

        public int sort { get; set; }

        /// <summary>
        /// State
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// UpdateTime
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }

    public class ClassView : Class
    {
        /// <summary>
        /// 子集集合
        /// </summary>
        public List<Class> children { get; set; }
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<Goods> goodsList { get; set; }
    }

    [Mapper]
    public partial class ClassMapper
    {
        public partial ClassView ToView(Class model);
        public partial List<ClassView> ToViewList(List<Class> list);
        public partial Class ToModel(ClassView model);

    }

}

