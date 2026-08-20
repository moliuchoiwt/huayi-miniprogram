using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;

namespace YW.DbContexts
{
    public class BaseRepository<T> : DbContext<T>, IBaseRepository<T> where T : class, new()
    {
        #region 异步操作方法

        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<T>> GetListAsync()
        {
            return await CurrentDb.GetListAsync();
        }

        /// <summary>
        /// 根据表达式查询
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await CurrentDb.GetListAsync(whereExpression);
        }


        /// <summary>
        /// 根据表达式查询分页
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel pageModel)
        {
            return await CurrentDb.GetPageListAsync(conditionalList, pageModel);
        }

        /// <summary>
        /// 根据表达式查询分页
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel pageModel)
        {
            return await CurrentDb.GetPageListAsync(whereExpression, pageModel);
        }
        /// <summary>
        /// 根据表达式查询分页并排序
        /// </summary>
        /// <param name="whereExpression">it</param>
        /// <param name="pageModel"></param>
        /// <param name="orderByExpression">it=>it.id或者it=>new{it.id,it.name}</param>
        /// <param name="orderByType">OrderByType.Desc</param>
        /// <returns></returns>
        public virtual async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel pageModel, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return await CurrentDb.GetPageListAsync(whereExpression, pageModel, orderByExpression, orderByType);
        }

        /// <summary>
        /// 根据表达式查询分页并排序
        /// </summary>
        /// <param name="whereExpression">it</param>
        /// <param name="pageModel"></param>
        /// <param name="orderByExpression">it=>it.id或者it=>new{it.id,it.name}</param>
        /// <param name="orderByType">OrderByType.Desc</param>
        /// <returns></returns>
        public virtual async Task<List<T>> GetPageListAsync(List<IConditionalModel> conditionalList, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {

            return await CurrentDb.GetPageListAsync(conditionalList, page, orderByExpression, orderByType);
        }

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <returns></returns>
        public virtual async Task<T> GetByIdAsync(dynamic id)
        {
            return await CurrentDb.GetByIdAsync(id);
        }

        /// <summary>
        /// 根据表达式查询一条数据
        /// </summary>
        /// <returns></returns>
        public virtual async Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression)
        {
            var list = await CurrentDb.GetListAsync(whereExpression);
            if (list != null && list.Count > 0) return list.FirstOrDefault();
            else return null;
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteByIdAsync(dynamic id)
        {
            return await CurrentDb.DeleteByIdAsync(id);
        }


        /// <summary>
        /// 根据实体删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteAsync(T data)
        {
            return await CurrentDb.DeleteAsync(data);
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteAsync(int[] ids)
        {
            return await CurrentDb.AsDeleteable().In(ids).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await CurrentDb.DeleteAsync(whereExpression);
        }


        /// <summary>
        /// 根据实体更新，实体需要有主键
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateAsync(T obj)
        {
            return await CurrentDb.UpdateAsync(obj);
        }

        /// <summary>
        ///批量更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateAsync(List<T> objs)
        {
            return await CurrentDb.UpdateRangeAsync(objs);
        }

        /// <summary>
        /// 根据条件更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return await CurrentDb.UpdateAsync(columns, whereExpression);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> InsertAsync(T obj)
        {
            return await CurrentDb.InsertAsync(obj);
        }

        /// <summary>
        /// 插入返回Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<int> InsertReturnIdentityAsync(T obj)
        {
            return await CurrentDb.InsertReturnIdentityAsync(obj);
        }

        /// <summary>
        /// 批量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> InsertAsync(List<T> objs)
        {
            return await CurrentDb.InsertRangeAsync(objs);
        }

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await CurrentDb.CountAsync(whereExpression);
        }
        #endregion

        #region 同步操作方法
        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetList()
        {
            return CurrentDb.GetList();
        }

        /// <summary>
        /// 根据表达式查询
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetList(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.GetList(whereExpression);
        }


        /// <summary>
        /// 根据表达式查询分页
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetPageList(List<IConditionalModel> conditionalList, PageModel pageModel)
        {
            return CurrentDb.GetPageList(conditionalList, pageModel);
        }

        /// <summary>
        /// 根据表达式查询分页
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel pageModel)
        {
            return CurrentDb.GetPageList(whereExpression, pageModel);
        }
        /// <summary>
        /// 根据表达式查询分页并排序
        /// </summary>
        /// <param name="whereExpression">it</param>
        /// <param name="pageModel"></param>
        /// <param name="orderByExpression">it=>it.id或者it=>new{it.id,it.name}</param>
        /// <param name="orderByType">OrderByType.Desc</param>
        /// <returns></returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel pageModel, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return CurrentDb.GetPageList(whereExpression, pageModel, orderByExpression, orderByType);
        }

        /// <summary>
        /// 根据表达式查询分页并排序
        /// </summary>
        /// <param name="whereExpression">it</param>
        /// <param name="pageModel"></param>
        /// <param name="orderByExpression">it=>it.id或者it=>new{it.id,it.name}</param>
        /// <param name="orderByType">OrderByType.Desc</param>
        /// <returns></returns>
        public virtual List<T> GetPageList(List<IConditionalModel> conditionalList, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return CurrentDb.GetPageList(conditionalList, page, orderByExpression, orderByType);
        }

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <returns></returns>
        public virtual T GetById(dynamic id)
        {
            return CurrentDb.GetById(id);
        }

        /// <summary>
        /// 根据表达式查询一条数据
        /// </summary>
        /// <returns></returns>
        public virtual T GetSingle(Expression<Func<T, bool>> whereExpression)
        {
            var list = CurrentDb.GetList(whereExpression);
            if (list != null && list.Count > 0) return list.FirstOrDefault();
            else return null;
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool DeleteById(dynamic id)
        {
            return CurrentDb.DeleteById(id);
        }


        /// <summary>
        /// 根据实体删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(T data)
        {
            return CurrentDb.Delete(data);
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(int[] ids)
        {
            return CurrentDb.AsDeleteable().In(ids).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.Delete(whereExpression);
        }


        /// <summary>
        /// 根据实体更新，实体需要有主键
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Update(T obj)
        {
            return CurrentDb.Update(obj);
        }

        /// <summary>
        ///批量更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Update(List<T> objs)
        {
            return CurrentDb.UpdateRange(objs);
        }

        /// <summary>
        /// 根据条件更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Update(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.Update(columns, whereExpression);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Insert(T obj)
        {
            return CurrentDb.Insert(obj);
        }

        /// <summary>
        /// 插入返回Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int InsertReturnIdentity(T obj)
        {
            return CurrentDb.InsertReturnIdentity(obj);
        }

        /// <summary>
        /// 批量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Insert(List<T> objs)
        {
            return CurrentDb.InsertRange(objs);
        }

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int Count(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.Count(whereExpression);
        }

        #endregion

        #region 获取流水记录
        /// <summary>
        /// 获取流水记录
        /// </summary>
        /// <param name="uType">用户类型</param>
        /// <param name="orderno">订单编号</param>
        /// <param name="userid">用户id</param>
        /// <param name="title">标题</param>
        /// <param name="change">金额</param>
        /// <param name="type">资金类型</param>
        /// <param name="stype">来源类型</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        public WalletLog GetWalletLog(walletUserTypeEnum uType, walletTypeEnum type, sourceTypeEnum stype, int userid, string title, decimal change, string orderno = "", string remark = "", int ShopId = 0)
        {
            return new WalletLog
            {
                createTime = DateTime.Now,
                updateTime = DateTime.Now,
                orderNo = orderno,
                userType = (int)uType,
                userId = userid,
                title = title,
                change = change,
                wType = (int)type,
                remark = remark,
                sourceType = (int)stype,
                shopId = ShopId
            };
        }

        #endregion
    }
}
