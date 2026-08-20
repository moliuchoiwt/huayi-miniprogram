namespace YW.Service
{
    public partial interface IWebCategoryService : IBaseRepository<WebCategory>
    {
    }
    public partial class WebCategoryService : BaseRepository<WebCategory>, IWebCategoryService
    {
        private readonly IClaimsAccessor _claimsAccessor;

        public WebCategoryService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

    }
}