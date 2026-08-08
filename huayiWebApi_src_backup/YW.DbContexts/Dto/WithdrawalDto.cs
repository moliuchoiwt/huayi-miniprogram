using System.Collections.Generic;

namespace YW.DbContexts.Dto
{
    public class WithdrawalDto 
    {
        /// <summary>
        /// 年
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 子集集合
        /// </summary>
        public List<Withdrawal> List { get; set; }
    }
}
