using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysMsgController
    /// </summary>
    public class SysMsgController : BaseController
    {

        private readonly IMsgService _msgService;
        private readonly MsgMapper mapper = new();
        public SysMsgController(IClaimsAccessor claimsAccessor, MsgService msgService)
        {
            _claimsAccessor = claimsAccessor;
            _msgService = msgService;
        }

        #region msg操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Msg>();
            exWhere.And(a => a.status < 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(queryModel.queryName) || a.url.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName) || a.contents.Contains(queryModel.queryName) || a.tagUrl.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await _msgService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(MsgView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _msgService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _msgService.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelMsg(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _msgService.UpdateAsync(it => new Msg { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }

        /// <summary>
        /// 客服回覆消息（需求4：後台回覆用戶）
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> Reply(MsgView model)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(model.contents)) { res.msg = "回覆内容不能为空"; return res; }
            if (model.toUserId <= 0) { res.msg = "请选择接收用户"; return res; }

            var info = new Msg
            {
                msgType = 2, // 客服聊天
                fromUserId = 0, // 0 = 平台客服
                toUserId = model.toUserId,
                sessionId = model.sessionId ?? $"sys_{model.toUserId}",
                title = "客服回覆",
                contents = model.contents,
                status = 0,
                readState = 0,
                createTime = DateTime.Now,
                updateTime = DateTime.Now
            };
            var isok = await _msgService.InsertAsync(info);
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = isok ? "回覆成功" : "回覆失败";
            return res;
        }
        #endregion
    }
}