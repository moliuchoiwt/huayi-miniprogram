using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysRewardSetController
    /// </summary>
    public class SysRewardSetController : BaseController
    {

        private readonly IRewardSetService _rewardSetService;
        private readonly IRewardRelationService _rewardRelationService;
        private readonly IRewardReceiveService _rewardReceiveService;

        private readonly RewardSetMapper mapper = new();
        private readonly RewardRelationMapper rewardRelationMapper = new();
        public SysRewardSetController(IClaimsAccessor claimsAccessor,
            RewardSetService rewardSetService, RewardRelationService rewardRelationService,
            RewardReceiveService rewardReceiveService)
        {
            _claimsAccessor = claimsAccessor;
            _rewardSetService = rewardSetService;
            _rewardRelationService = rewardRelationService;
            _rewardReceiveService = rewardReceiveService;
        }

        #region rewardSet操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<RewardSet>();
            exWhere.And(a => a.State < 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.State == queryModel.queryState.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.Title.Contains(queryModel.queryName) || a.ImgUrl.Contains(queryModel.queryName) || a.Intro.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.CreateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.CreateTime <= queryModel.endTime.Value);
            }

            var list = new List<RewardSetView>();
            var data = await _rewardSetService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
            if (data.Count > 0)
            {
                list = mapper.ToViewList(data);
                var rids = data.Select(a => a.Id).ToList();

                var rdata = await _rewardRelationService.GetListAsync(a => a.State < 99 && SqlFunc.ContainsArray(rids, a.RewardId));
                var rlist = rewardRelationMapper.ToViewList(rdata);
                rlist = rlist.OrderByDescending(a => a.UpdateTime).ToList();


                var receivelist = await _rewardReceiveService.GetListAsync(a => SqlFunc.ContainsArray(rids, a.RewardId));
                receivelist = receivelist.OrderByDescending(a => a.CreateTime).ToList();
                foreach (var item in list)
                {
                    item.RelationList = rlist.Where(a => a.RewardId == item.Id).ToList();
                    item.ReceiveList = receivelist.Where(a => a.RewardId == item.Id).ToList();
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(RewardSetView model)
        {
            var res = await _rewardSetService.Operation(model);
            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelRewardSet(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _rewardSetService.UpdateAsync(it => new RewardSet { State = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");

            return res;

        }
        #endregion
    }
}