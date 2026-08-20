namespace YW.Service
{
    public partial interface IWebNavMenuService : IBaseRepository<WebNavMenu>
    {
    }
    public partial class WebNavMenuService : BaseRepository<WebNavMenu>, IWebNavMenuService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public WebNavMenuService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }
}