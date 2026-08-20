namespace YW.Service
{

    public partial interface IRewardSetService
    {
        /// <summary>
        /// 添加/修改
        /// </summary>
        Task<ResultModel> Operation(RewardSetView model);

    }

    public partial class RewardSetService : BaseRepository<RewardSet>, IRewardSetService
    {

        private readonly RewardSetMapper mapper = new();
        private readonly RewardRelationMapper rewardRelationMapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public RewardSetService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        /// <summary>
        /// 添加/修改
        /// </summary>
        public async Task<ResultModel> Operation(RewardSetView view)
        {

            var res = new ResultModel();

            if (view.RelationList == null || view.RelationList.Count <= 0)
            {
                res.msg = "奖励列表不能为空";
                return res;
            }
            if (RewardSetDb.Count(a => a.RewardType == view.RewardType && a.State <= 0) > 0)
            {
                res.msg = "奖励类型已存在,请勿重复添加";
                return res;
            }
            db.Ado.BeginTran();
            try
            {
                var info = mapper.ToModel(view);
                if (info.Id > 0)
                {
                    info.UpdateTime = DateTime.Now;
                    await RewardSetDb.UpdateAsync(info);
                }
                else
                {
                    info.CreateTime = DateTime.Now;
                    info.UpdateTime = DateTime.Now;
                    info.Id = await RewardSetDb.InsertReturnIdentityAsync(info);
                }
                view.RelationList.ForEach(item =>
                {
                    item.UpdateTime = DateTime.Now;
                    item.RewardId = info.Id;
                });
                //1.清除删除的奖励记录
                if (view.RelationList.Count(a => a.Id > 0) > 0)
                {
                    var rids = view.RelationList.Where(a => a.Id > 0).Select(a => a.Id).ToList();
                    RewardRelationDb.Update(a => new RewardRelation { State = 99, UpdateTime = DateTime.Now }, a => a.State == 0 && !SqlSugar.SqlFunc.ContainsArray(rids, a.Id));
                }
                //2.更新修改的奖励记录
                if (view.RelationList.Count(a => a.Id > 0) > 0)
                {
                    var rlist = rewardRelationMapper.ToModelList(view.RelationList.Where(a => a.Id > 0).ToList());
                    RewardRelationDb.UpdateRange(rlist);
                }
                //3.添加新增记录
                if (view.RelationList.Count(a => a.Id == 0) > 0)
                {
                    var rlist = rewardRelationMapper.ToModelList(view.RelationList.Where(a => a.Id == 0).ToList());
                    RewardRelationDb.InsertRange(rlist);
                }
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
                return res;
            }
            db.Ado.CommitTran();
            res.code = (int)ResultEnum.success;
            res.msg = "操作成功";
            return res;

        }


    }
}