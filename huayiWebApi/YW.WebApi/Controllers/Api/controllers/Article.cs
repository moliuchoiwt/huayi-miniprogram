using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 文章
    /// </summary>
    [Authorize(Roles = "api")]
    public class ArticleController : BaseController
    {
        private readonly IArticleService _articleService;
        private readonly IBrowsesService _browsesService;
        private readonly ILikesService _likesService;
        private readonly IFollowsService _followsService;
        private readonly IArticleMessageService _articleMessageService;
        private readonly ICollectionRecordService _collectionRecordService;
        private readonly IUserInfoService _userInfoService;

        private readonly BrowsesMapper browsesMapper = new();
        private readonly ArticleMessageMapper articleMessageMapper = new();
        private readonly LikesMapper likesMapper = new();
        private readonly CollectionRecordMapper collectionRecordMapper = new();
        private readonly FollowsMapper followsMapper = new();

        public ArticleController(ArticleService articleService, IClaimsAccessor claimsAccessor,
            BrowsesService browsesService, LikesService likesService, FollowsService followsService,
            ArticleMessageService articleMessageService, CollectionRecordService collectionRecordService,
            UserInfoService userInfoService)
        {
            _articleService = articleService;
            _claimsAccessor = claimsAccessor;
            _browsesService = browsesService;
            _likesService = likesService;
            _followsService = followsService;
            _articleMessageService = articleMessageService;
            _collectionRecordService = collectionRecordService;
            _userInfoService = userInfoService;
        }

        #region 动态文章

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultModel> ArticleList(QueryModel view) => await _articleService.frontEndList(view, user);


        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultModel> ArticleInfo(QueryModel view) => await _articleService.frontEndDetails(view, user);


        /// <summary>
        ///提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> SumbitArticle(ArticleView view)
        {
            var res = new ResultModel();
            var _mapper = new ArticleMapper();

            var info = _mapper.ToModel(view);
            bool isok = false;
            info.status = 1;
            info.userId = (int)_claimsAccessor.UserId;
            info.userName = _claimsAccessor.UserName;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _articleService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _articleService.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "提交" + (isok ? "成功" : "失败");
            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelArticle(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _articleService.UpdateAsync(it => new Article { status = 99 }, it => it.userId == _claimsAccessor.UserId && SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");

            return res;

        }
        #endregion


        #region 浏览记录
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> BrowsesList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Browses>();
            exWhere.And(a => a.State == 0);
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.BrowseType == queryModel.queryType.Value);
            }
            if (queryModel.userId.HasValue)
            {
                exWhere.And(a => a.UserId == queryModel.userId.Value && a.ShareUserId != queryModel.userId.Value);
            }
            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.ShareUserId == queryModel.parentId.Value && a.UserId != queryModel.parentId.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.BrowsesTitle.Contains(queryModel.queryName) || a.UserName.Contains(queryModel.queryName) || a.ShareName.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.CreateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.CreateTime <= queryModel.endTime.Value);
            }

            var data = await _browsesService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
            var list = new List<BrowsesView>();
            if (data.Count > 0)
            {
                list = browsesMapper.ToViewList(data);

                var uids = data.Select(a => a.UserId).Distinct().ToList();
                uids.AddRange(data.Select(a => a.ShareUserId).Distinct().ToList());
                uids = uids.Distinct().ToList();
                var ulist = await _userInfoService.GetListAsync(a => SqlFunc.ContainsArray(uids, a.Id));
                foreach (var item in list)
                {
                    item.UserAvatar = ulist != null && ulist.Count(a => a.Id == item.UserId) > 0 ? GetFileUrl(ulist.FirstOrDefault(a => a.Id == item.UserId).avatar) : "";
                    item.ShareAvatar = ulist != null && ulist.Count(a => a.Id == item.ShareUserId) > 0 ? GetFileUrl(ulist.FirstOrDefault(a => a.Id == item.ShareUserId).avatar) : "";
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
        public async Task<ResultModel> OperationBrowses(BrowsesView model)
        {
            var res = new ResultModel();
            var info = browsesMapper.ToModel(model);
            bool isok = false;
            info.State = 0;
            info.BrowsesEndTime = DateTime.Now;
            info.ReadTimes = (int)(info.BrowsesEndTime - info.BrowsesTime).TotalSeconds;
            info.UserId = (int)_claimsAccessor.UserId;
            info.UserName = _claimsAccessor.UserName;


            var shareuser = await _userInfoService.GetByIdAsync(model.ShareUserId);
            if (shareuser == null || shareuser.Id <= 0)
            {
                res.msg = "分享用户不存在";
                return res;
            }
            info.ShareName = shareuser.nickName;

            if (info.Id > 0)
            {
                isok = await _browsesService.UpdateAsync(info);
            }
            else
            {
                info.CreateTime = DateTime.Now;
                isok = await _browsesService.InsertAsync(info);
                if (isok)
                {
                    var total = _browsesService.CountAsync(a => a.BrowseType == info.BrowseType && a.BrowsesId == model.BrowsesId && a.State == 0).Result;
                    total += 1;
                    switch (model.BrowseType)
                    {
                        case 1:
                            await _articleService.UpdateAsync(a => new Article { browseNum = total }, a => a.Id == model.BrowsesId);
                            break;
                        default:
                            break;
                    }
                }
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion

        #region 留言
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> MessageList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<ArticleMessage>(a => a.State == 0);
            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.ParentId == queryModel.parentId.Value);
            }
            if (queryModel.queryId.HasValue)
            {
                exWhere.And(a => a.ArticleId == queryModel.queryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.ArticleTitle.Contains(queryModel.queryName) || a.Url.Contains(queryModel.queryName) || a.Intro.Contains(queryModel.queryName) || a.UserName.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.CreateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.CreateTime <= queryModel.endTime.Value);
            }

            var data = await _articleMessageService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
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
        public async Task<ResultModel> OperationMessage(ArticleMessageView model)
        {
            var res = new ResultModel();
            var info = articleMessageMapper.ToModel(model);
            bool isok = false;
            info.ReadState = 0;
            info.UserId = (int)_claimsAccessor.UserId;
            info.UserName = _claimsAccessor.UserName;
            if (info.Id > 0)
            {
                info.UpdateTime = DateTime.Now;
                isok = await _articleMessageService.UpdateAsync(info);
            }
            else
            {
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _articleMessageService.InsertAsync(info);
                if (isok)
                {
                    var total = _articleMessageService.CountAsync(a => a.ArticleId == model.ArticleId && a.State == 0).Result;
                    await _articleService.UpdateAsync(a => new Article { msgNum = total }, a => a.Id == model.ArticleId);
                }
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion

        #region 点赞
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> LikesList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Likes>();
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
                else exWhere.And(a => a.Title.Contains(queryModel.queryName) || a.UserName.Contains(queryModel.queryName));

            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.LikesType == queryModel.queryType.Value);
            }
            if (queryModel.queryId.HasValue)
            {
                exWhere.And(a => a.LikesId == queryModel.queryId.Value);
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.CreateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.CreateTime <= queryModel.endTime.Value);
            }

            var data = await _likesService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
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
        public async Task<ResultModel> OperationLikes(LikesView model)
        {
            var res = new ResultModel();
            var info = likesMapper.ToModel(model);
            bool isok = false;
            info.ReadState = 0;
            info.UserId = (int)_claimsAccessor.UserId;
            info.UserName = _claimsAccessor.UserName;
            switch (model.LikesType)
            {
                case 0:
                    var ainfo = await _articleService.GetByIdAsync(model.LikesId);
                    if (ainfo == null || ainfo.Id <= 0)
                    {
                        res.msg = "被收藏信息不存在";
                        return res;
                    }
                    info.Title = ainfo.title;
                    info.ToUserId = ainfo.userId;
                    break;
                default:
                    break;
            }
            if (_likesService.CountAsync(a => a.LikesType == info.LikesType && a.LikesId == model.LikesId && a.UserId == info.UserId).Result > 0)
            {
                var like = await _likesService.GetSingleAsync(a => a.LikesType == info.LikesType && a.LikesId == model.LikesId && a.UserId == info.UserId);
                like.State = like.State == 0 ? 1 : 0;
                like.UpdateTime = DateTime.Now;
                isok = await _likesService.UpdateAsync(like);
            }
            else
            {
                info.State = 1;
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _likesService.InsertAsync(info);
            }
            if (isok && info.LikesType == 0)
            {
                var total = _likesService.CountAsync(a => a.LikesId == model.LikesId && a.LikesType == 0 && a.State == 1).Result;
                await _articleService.UpdateAsync(a => new Article { likeNum = total }, a => a.Id == model.LikesId);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }

        #endregion

        #region 收藏
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> CollectionList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<CollectionRecord>();
            exWhere.And(a => a.status < 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.cType == queryModel.queryType.Value);
            }
            if (queryModel.queryId.HasValue)
            {
                exWhere.And(a => a.cId == queryModel.queryId.Value);
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await _collectionRecordService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
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
        public async Task<ResultModel> OperationCollection(CollectionRecordView model)
        {
            var res = new ResultModel();
            var info = collectionRecordMapper.ToModel(model);
            bool isok = false;
            info.readState = 0;
            info.userId = (int)_claimsAccessor.UserId;

            switch (model.cType)
            {
                case 0:

                    var ainfo = await _articleService.GetByIdAsync(model.cId);
                    if (ainfo == null || ainfo.Id <= 0)
                    {
                        res.msg = "被收藏信息不存在";
                        return res;
                    }
                    break;
                default:
                    break;
            }



            if (_collectionRecordService.CountAsync(a => a.cType == info.cType && a.cId == model.cId && a.userId == info.userId).Result > 0)
            {
                var coll = await _collectionRecordService.GetSingleAsync(a => a.cType == info.cType && a.cId == model.cId && a.userId == info.userId);
                coll.status = coll.status == 0 ? 1 : 0;
                coll.updateTime = DateTime.Now;
                isok = await _collectionRecordService.UpdateAsync(coll);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _collectionRecordService.InsertAsync(info);
            }
            if (isok && info.cType == 0)
            {
                var total = _collectionRecordService.CountAsync(a => a.cType == 0 && a.cId == model.cId && a.status == 1).Result;
                await _articleService.UpdateAsync(a => new Article { likeNum = total }, a => a.Id == model.cId);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion

        #region 关注
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> FollowsList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Follows>(a => a.State < 99);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.UserName.Contains(queryModel.queryName) || a.ToUserName.Contains(queryModel.queryName));
            }
            if (queryModel.queryType.HasValue && queryModel.queryType.Value == 1)
            {
                //粉丝列表（关注我的用户）
                exWhere.And(a => a.State == 1 && a.ToUserId == _claimsAccessor.UserId);
            }
            else
            {
                //我关注的用户列表
                exWhere.And(a => a.State == 1 && a.UserId == _claimsAccessor.UserId);
            }
            var data = await _followsService.GetPageListAsync(exWhere, p, it => new { it.UpdateTime, it.Id }, OrderByType.Desc);
            var list = new List<FollowsView>();
            if (data.Count > 0)
            {
                list = followsMapper.ToViewList(data);
                var uids = list.Select(a => a.UserId).Distinct().ToList();
                uids.AddRange(list.Select(a => a.ToUserId).Distinct().ToList());

                var myFollow = new List<Follows>();
                if (queryModel.queryType.HasValue && queryModel.queryType.Value == 1)
                {
                    var ids = list.Select(a => a.UserId).Distinct().ToList();
                    myFollow = await _followsService.GetListAsync(a => a.State == 1 && a.UserId == _claimsAccessor.UserId && SqlFunc.ContainsArray(ids, a.ToUserId));

                }

                var ulist = await _userInfoService.GetListAsync(a => SqlFunc.ContainsArray(uids, a.Id));
                foreach (var item in list)
                {
                    if (ulist.Count(a => a.Id == item.UserId) > 0)
                    {
                        var uInfo = ulist.FirstOrDefault(a => a.Id == item.UserId);
                        item.UserName = uInfo.nickName;
                        item.UserAvatar = GetFileUrl(uInfo.avatar);
                    }
                    if (ulist.Count(a => a.Id == item.ToUserId) > 0)
                    {
                        var uInfo = ulist.FirstOrDefault(a => a.Id == item.ToUserId);
                        item.ToUserName = uInfo.nickName;
                        item.ToUserAvatar = GetFileUrl(uInfo.avatar);
                    }
                    if (queryModel.queryType.HasValue && queryModel.queryType.Value == 1)
                    {
                        //粉丝
                        item.IsFollow = myFollow != null && myFollow.Count(a => a.ToUserId == item.UserId) > 0 ? 1 : 0;
                    }
                    else
                    {
                        item.IsFollow = 1;
                    }
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
        public async Task<ResultModel> OperationFollows(FollowsView model)
        {
            var res = new ResultModel();

            if (model.ToUserId <= 0)
            {
                res.msg = "被关注用户不能为空";
                return res;
            }
            if (model.ToUserId == (int)_claimsAccessor.UserId)
            {
                res.msg = "不能关注自我";
                return res;
            }
            var user = await _userInfoService.GetByIdAsync(model.ToUserId);
            if (user == null || user.Id <= 0)
            {
                res.msg = "被关注用户不存在";
                return res;
            }
            model.ToUserName = user.nickName;
            var info = followsMapper.ToModel(model);
            bool isok = false;
            info.ReadState = 0;
            info.UserId = (int)_claimsAccessor.UserId;
            info.UserName = _claimsAccessor.UserName;
            if (_followsService.CountAsync(a => a.UserId == _claimsAccessor.UserId && a.ToUserId == model.ToUserId).Result > 0)
            {
                var follow = await _followsService.GetSingleAsync(a => a.UserId == _claimsAccessor.UserId && a.ToUserId == model.ToUserId);
                follow.State = follow.State == 0 ? 1 : 0;
                follow.UpdateTime = DateTime.Now;
                isok = await _followsService.UpdateAsync(follow);
            }
            else
            {
                info.State = 1;
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _followsService.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion

    }
}
