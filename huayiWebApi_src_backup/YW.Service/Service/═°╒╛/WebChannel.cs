namespace YW.Service
{
    public partial interface IWebChannelService : IBaseRepository<WebChannel>
    {
    }
    public partial class WebChannelService : BaseRepository<WebChannel>, IWebChannelService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public WebChannelService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }
}