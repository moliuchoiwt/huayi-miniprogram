namespace YW.Service
{
    public partial interface IOrderTaskApplyService : IBaseRepository<OrderTaskApply>
    {
    }
    public partial class OrderTaskApplyService : BaseRepository<OrderTaskApply>, IOrderTaskApplyService
    {
        private readonly OrderTaskApplyMapper _mapper = new();

    }
}