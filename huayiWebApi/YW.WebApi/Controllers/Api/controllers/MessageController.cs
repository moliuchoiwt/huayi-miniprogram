using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 應用內消息/通知系統（需求4）
    /// 用戶↔用戶、用戶↔客服 雙向對話
    /// </summary>
    [Authorize(Roles = "api")]
    public class MessageController : BaseController
    {
        private readonly IMsgService _msgService;
        public MessageController(MsgService msgService, IClaimsAccessor claimsAccessor)
        {
            _msgService = msgService;
            _claimsAccessor = claimsAccessor;
        }

        /// <summary>
        /// 發送消息（用戶→用戶 / 用戶→客服）
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> Send(MsgView model)
        {
            var res = new ResultModel();
            if (user == null) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(model.contents)) { res.msg = "消息内容不能为空"; return res; }

            var info = new Msg
            {
                msgType = model.msgType,        // 1=用戶聊天 2=客服聊天
                fromUserId = user.Id,
                toUserId = model.toUserId,      // 0=發給平台客服
                orderNo = model.orderNo ?? "",
                sessionId = model.sessionId ?? $"{Math.Min(user.Id, model.toUserId)}_{Math.Max(user.Id, model.toUserId)}",
                title = model.title ?? "",
                contents = model.contents,
                status = 0,
                readState = 0,
                createTime = DateTime.Now,
                updateTime = DateTime.Now
            };

            var isok = await _msgService.InsertAsync(info);
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = isok ? "发送成功" : "发送失败";
            return res;
        }

        /// <summary>
        /// 獲取消息列表（會話列表，按 sessionId 分組，取最新一條）
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> SessionList(QueryModel view)
        {
            var res = new ResultModel();
            if (user == null) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            var p = new PageModel { PageIndex = view.pageNum, PageSize = view.pageSize };
            // 取與當前用戶相關的所有消息，按 sessionId 分組取最新
            var list = await db.Queryable<Msg>()
                .Where(a => a.status == 0 && (a.fromUserId == user.Id || a.toUserId == user.Id || a.toUserId == 0))
                .OrderBy(a => a.createTime, OrderByType.Desc)
                .ToListAsync();

            // 分組取每個 session 最新一條 + 未讀數
            var sessions = list
                .GroupBy(a => a.sessionId)
                .Select(g => new
                {
                    sessionId = g.Key,
                    lastMsg = g.OrderByDescending(a => a.createTime).First(),
                    unreadCount = g.Count(a => a.toUserId == user.Id && a.readState == 0)
                })
                .OrderByDescending(a => a.lastMsg.createTime)
                .ToList();

            res.code = (int)ResultEnum.success;
            res.data = new { total = sessions.Count, items = sessions };
            return res;
        }

        /// <summary>
        /// 獲取某會話的歷史消息
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> History(MsgQuery view)
        {
            var res = new ResultModel();
            if (user == null) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(view.sessionId)) { res.msg = "缺少会话ID"; return res; }

            var p = new PageModel { PageIndex = view.pageNum, PageSize = view.pageSize };
            var data = await db.Queryable<Msg>()
                .Where(a => a.status == 0 && a.sessionId == view.sessionId)
                .OrderBy(a => a.createTime, OrderByType.Desc)
                .ToPageListAsync(p.PageIndex, p.PageSize);

            // 標記已讀（當前用戶是接收方）
            await db.Updateable<Msg>()
                .SetColumns(a => new Msg { readState = 1, readTime = DateTime.Now })
                .Where(a => a.sessionId == view.sessionId && a.toUserId == user.Id && a.readState == 0)
                .ExecuteCommandAsync();

            res.code = (int)ResultEnum.success;
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        /// <summary>
        /// 獲取未讀消息總數（用於底部導航紅點）
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> UnreadCount()
        {
            var res = new ResultModel();
            if (user == null) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            var count = await db.Queryable<Msg>()
                .CountAsync(a => a.status == 0 && a.toUserId == user.Id && a.readState == 0);

            res.code = (int)ResultEnum.success;
            res.data = count;
            return res;
        }
    }

    public class MsgQuery : QueryModel
    {
        public string sessionId { get; set; }
    }
}
