using System;
using System.Threading.Tasks;

namespace VAM.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryBase<Entities.User> Users { get; }
        IRepositoryBase<Entities.Category> Categories { get; }
        IRepositoryBase<Entities.Farm> Farms { get; }
        IRepositoryBase<Entities.Product> Products { get; }
        IRepositoryBase<Entities.Order> Orders { get; }
        IRepositoryBase<Entities.OrderItem> OrderItems { get; }
        IRepositoryBase<Entities.Payment> Payments { get; }
        IRepositoryBase<Entities.Review> Reviews { get; }
        IRepositoryBase<Entities.SellerProfile> SellerProfiles { get; }
        IRepositoryBase<Entities.BusinessProfile> BusinessProfiles { get; }
        IRepositoryBase<Entities.PayoutTransaction> PayoutTransactions { get; }
        
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
