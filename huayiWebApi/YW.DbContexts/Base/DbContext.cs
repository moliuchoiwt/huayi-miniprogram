namespace YW.DbContexts
{
    public class DbContext<T> where T : class, new()
    {
        private readonly string connectStr = PubConstant.ConnectionString;
        public DbContext()
        {
            db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connectStr,
                DbType = DbType.SqlServer,
                InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                IsAutoCloseConnection = true//开启自动释放模式和EF原理一样我就不多解释了
            });//用来处理事务多表查询和复杂的操作
            //调式代码 用来打印SQL
            //Db.Aop.OnLogExecuting = (sql, pars) =>
            //{
            //    LogHelper.Debug(sql + "\r\n" + Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
            //};
        }
        //注意：不能写成静态的
        public SqlSugarClient db;//用来处理事务多表查询和复杂的操作
        public SimpleClient<T> CurrentDb { get { return new SimpleClient<T>(db); } }//用来操作当前表的数据


        #region Db操作
        public SimpleClient<AdminLog> AdminLogDb { get { return new SimpleClient<AdminLog>(db); } } //用来处理 AdminLog 表的常用操作        
        public SimpleClient<AfterSale> AfterSaleDb { get { return new SimpleClient<AfterSale>(db); } } //用来处理 AfterSale 表的常用操作
        public SimpleClient<Article> ArticleDb { get { return new SimpleClient<Article>(db); } } //用来处理 Article 表的常用操作
        public SimpleClient<ArticleMessage> ArticleMessageDb { get { return new SimpleClient<ArticleMessage>(db); } } //用来处理 ArticleMessage 表的常用操作
        public SimpleClient<Banner> BannerDb { get { return new SimpleClient<Banner>(db); } } //用来处理 Banner 表的常用操作
        public SimpleClient<Browses> BrowsesDb { get { return new SimpleClient<Browses>(db); } } //用来处理 Browses 表的常用操作
        public SimpleClient<Cart> CartDb { get { return new SimpleClient<Cart>(db); } } //用来处理 Cart 表的常用操作
        public SimpleClient<Class> ClassDb { get { return new SimpleClient<Class>(db); } } //用来处理 Class 表的常用操作
        public SimpleClient<CollectionRecord> CollectionRecordDb { get { return new SimpleClient<CollectionRecord>(db); } } //用来处理 CollectionRecord 表的常用操作
        public SimpleClient<Comment> CommentDb { get { return new SimpleClient<Comment>(db); } } //用来处理 Comment 表的常用操作
        public SimpleClient<Coupon> CouponDb { get { return new SimpleClient<Coupon>(db); } } //用来处理 Coupon 表的常用操作
        public SimpleClient<CouponRole> CouponRoleDb { get { return new SimpleClient<CouponRole>(db); } } //用来处理 CouponRole 表的常用操作
        public SimpleClient<Express> ExpressDb { get { return new SimpleClient<Express>(db); } } //用来处理 Express 表的常用操作
        public SimpleClient<Feedback> FeedbackDb { get { return new SimpleClient<Feedback>(db); } } //用来处理 Feedback 表的常用操作
        public SimpleClient<Follows> FollowsDb { get { return new SimpleClient<Follows>(db); } } //用来处理 Follows 表的常用操作
        public SimpleClient<Goods> GoodsDb { get { return new SimpleClient<Goods>(db); } } //用来处理 Goods 表的常用操作
        public SimpleClient<GoodSku> GoodSkuDb { get { return new SimpleClient<GoodSku>(db); } } //用来处理 GoodSku 表的常用操作
        public SimpleClient<GoodsOrder> GoodsOrderDb { get { return new SimpleClient<GoodsOrder>(db); } } //用来处理 GoodsOrder 表的常用操作
        public SimpleClient<GoodsOrderDetail> GoodsOrderDetailDb { get { return new SimpleClient<GoodsOrderDetail>(db); } } //用来处理 GoodsOrderDetail 表的常用操作
        public SimpleClient<GoodsOrderDivide> GoodsOrderDivideDb { get { return new SimpleClient<GoodsOrderDivide>(db); } } //用来处理 GoodsOrderDivide 表的常用操作        
        public SimpleClient<Likes> LikesDb { get { return new SimpleClient<Likes>(db); } } //用来处理 Likes 表的常用操作
        public SimpleClient<Logistics> LogisticsDb { get { return new SimpleClient<Logistics>(db); } } //用来处理 Logistics 表的常用操作
        public SimpleClient<Msg> MsgDb { get { return new SimpleClient<Msg>(db); } } //用来处理 Msg 表的常用操作
        public SimpleClient<Shop> ShopDb { get { return new SimpleClient<Shop>(db); } } //用来处理 Shop 表的常用操作
        public SimpleClient<Sms> SmsDb { get { return new SimpleClient<Sms>(db); } } //用来处理 Sms 表的常用操作
        public SimpleClient<UserAddress> UserAddressDb { get { return new SimpleClient<UserAddress>(db); } } //用来处理 UserAddress 表的常用操作
        public SimpleClient<UserCoupon> UserCouponDb { get { return new SimpleClient<UserCoupon>(db); } } //用来处理 UserCoupon 表的常用操作
        public SimpleClient<UserInfo> UserInfoDb { get { return new SimpleClient<UserInfo>(db); } } //用来处理 UserInfo 表的常用操作
        public SimpleClient<WalletLog> WalletLogDb { get { return new SimpleClient<WalletLog>(db); } } //用来处理 WalletLog 表的常用操作        
        public SimpleClient<Withdrawal> WithdrawalDb { get { return new SimpleClient<Withdrawal>(db); } } //用来处理 Withdrawal 表的常用操作
        public SimpleClient<RewardSet> RewardSetDb { get { return new SimpleClient<RewardSet>(db); } } //用来处理 RewardSet 表的常用操作
        public SimpleClient<RewardReceive> RewardReceiveDb { get { return new SimpleClient<RewardReceive>(db); } } //用来处理 RewardReceive 表的常用操作
        public SimpleClient<RewardRelation> RewardRelationDb { get { return new SimpleClient<RewardRelation>(db); } } //用来处理 RewardRelation 表的常用操作		

        public SimpleClient<UserGrade> UserGradeDb { get { return new SimpleClient<UserGrade>(db); } } //用来处理 UserGrade 表的常用操作		        


        //网站
        public SimpleClient<WebNavMenu> WebNavMenuDb { get { return new SimpleClient<WebNavMenu>(db); } } //用来处理 WebNavMenu 表的常用操作		
        public SimpleClient<WebChannel> WebChannelDb { get { return new SimpleClient<WebChannel>(db); } } //用来处理 WebChannel 表的常用操作		
        public SimpleClient<WebCategory> WebCategoryDb { get { return new SimpleClient<WebCategory>(db); } } //用来处理 WebCategory 表的常用操作

        public SimpleClient<TaskOrder> TaskOrderDb { get { return new SimpleClient<TaskOrder>(db); } } //用来处理 TaskOrder 表的常用操作
        public SimpleClient<OrderTaskApply> OrderTaskApplyDb { get { return new SimpleClient<OrderTaskApply>(db); } } //用来处理 OrderTaskApply 表的常用操作		
        #endregion

        #region 系统
        public SimpleClient<SysUser> sysUserDb { get { return new SimpleClient<SysUser>(db); } } //用来处理 sysUser 表的常用操作
        public SimpleClient<SysMenu> sysMenuDb { get { return new SimpleClient<SysMenu>(db); } } //用来处理 sysMenu 表的常用操作
        public SimpleClient<SysRole> sysRoleDb { get { return new SimpleClient<SysRole>(db); } } //用来处理 sysRole 表的常用操作
        public SimpleClient<SysRoleMenu> sysRoleMenuDb { get { return new SimpleClient<SysRoleMenu>(db); } } //用来处理 sysRoleMenu 表的常用操作

        #endregion

        #region Db操作-视图
        #endregion

    }


}
