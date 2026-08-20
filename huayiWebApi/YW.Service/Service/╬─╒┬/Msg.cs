namespace YW.Service
{
    public partial interface IMsgService : IBaseRepository<Msg>
    {

    }
    public partial class MsgService : BaseRepository<Msg>, IMsgService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public MsgService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }



    }
}
