using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///提现记录
    // </summary>	

    [SugarTable("Withdrawal")]
    public partial class Withdrawal
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 提现编号
        /// </summary>

        public string withdrawalNo { get; set; }

        /// <summary>
        /// 类型 0-提现到微信零钱 1-提现到银行卡
        /// </summary>

        public int wType { get; set; }

        /// <summary>
        /// 用户类型 0-用户 1-店铺
        /// </summary>

        public int userType { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }



        /// <summary>
        /// 微信OpenID
        /// </summary>

        public string openID { get; set; }

        /// <summary>
        /// 微信银行code
        /// </summary>

        public string bankCode { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>

        public string bankCard { get; set; }

        /// <summary>
        /// 银行开户行
        /// </summary>

        public string bankName { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>

        public decimal amount { get; set; }

        /// <summary>
        /// 提现手续费比例
        /// </summary>

        public decimal serviceRate { get; set; }

        /// <summary>
        /// 提现手续费
        /// </summary>

        public decimal serviceCharge { get; set; }

        /// <summary>
        /// 实际到账金额
        /// </summary>

        public decimal actualTotal { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>

        public string reamrk { get; set; }

        /// <summary>
        /// 审核信息
        /// </summary>

        public string auditIntro { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string package { get; set; } = string.Empty;

        /// <summary>
        /// 状态 0-待审批 1-通过 2-驳回 3-已领取 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string realName { get; set; } = string.Empty;

    }

    public class AuditWithdrawView
    {
        /// <summary>
        /// 审核ID
        /// </summary>
        public List<int> ids { get; set; }
        //审核状态
        public int status { get; set; }
        /// <summary>
        /// 审核信息
        /// </summary>
        public string auditInfo { get; set; }

    }

    /// <summary>
    ///提现记录
    // </summary>	

    public partial class WithdrawalView : Withdrawal
    {
        /// <summary>
        /// 用户昵称
        /// </summary>

        public string userName { get; set; }
    }

    [Mapper]
    public partial class WithdrawalMapper
    {
        public partial WithdrawalView ToView(Withdrawal model);
        public partial List<WithdrawalView> ToViewList(List<Withdrawal> list);
        public partial Withdrawal ToModel(WithdrawalView model);
    }
}

