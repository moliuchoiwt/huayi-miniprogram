namespace YW.Service
{


    public partial class WxReplyService : BaseRepository<WxReply>, IWxReplyService
    {
        private readonly IClaimsAccessor _claimsAccessor;
        public WxReplyService()
        {
        }
        public WxReplyService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }


    public partial class RewardRelationService : BaseRepository<RewardRelation>, IRewardRelationService
    {
        private readonly RewardRelationMapper _mapper;
        private readonly IClaimsAccessor _claimsAccessor;

        public RewardRelationService(IClaimsAccessor claimsAccessor, RewardRelationMapper mapper)
        {
            _claimsAccessor = claimsAccessor;
            _mapper = mapper;
        }

    }
    public partial class RewardReceiveService : BaseRepository<RewardReceive>, IRewardReceiveService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public RewardReceiveService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }


    public partial class AdminLogService : BaseRepository<AdminLog>, IAdminLogService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public AdminLogService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }






    public partial class ArticleMessageService : BaseRepository<ArticleMessage>, IArticleMessageService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public ArticleMessageService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }









    public partial class CouponRoleService : BaseRepository<CouponRole>, ICouponRoleService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public CouponRoleService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }

    public partial class ExpressService : BaseRepository<Express>, IExpressService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public ExpressService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }



    public partial class FollowsService : BaseRepository<Follows>, IFollowsService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public FollowsService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }

    public partial class GoodSkuService : BaseRepository<GoodSku>, IGoodSkuService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public GoodSkuService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }


    public partial class GoodsOrderDetailService : BaseRepository<GoodsOrderDetail>, IGoodsOrderDetailService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public GoodsOrderDetailService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }

    public partial class GoodsOrderDivideService : BaseRepository<GoodsOrderDivide>, IGoodsOrderDivideService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public GoodsOrderDivideService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }


    public partial class LikesService : BaseRepository<Likes>, ILikesService
    {

        private readonly IClaimsAccessor _claimsAccessor;

        public LikesService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }


    public partial class SmsService : BaseRepository<Sms>, ISmsService
    {

        private readonly IClaimsAccessor _claimsAccessor;

        public SmsService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }











}
