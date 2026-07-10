using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;
using VAM.Data;
using VAM.Entities;

namespace VAM.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRepositoryBase<User> Users { get; private set; }
        public IRepositoryBase<Category> Categories { get; private set; }
        public IRepositoryBase<Farm> Farms { get; private set; }
        public IRepositoryBase<Product> Products { get; private set; }
        public IRepositoryBase<Order> Orders { get; private set; }
        public IRepositoryBase<OrderItem> OrderItems { get; private set; }
        public IRepositoryBase<Payment> Payments { get; private set; }
        public IRepositoryBase<Review> Reviews { get; private set; }
        public IRepositoryBase<SellerProfile> SellerProfiles { get; private set; }
        public IRepositoryBase<BusinessProfile> BusinessProfiles { get; private set; }
        public IRepositoryBase<PayoutTransaction> PayoutTransactions { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new RepositoryBase<User>(_context);
            Categories = new RepositoryBase<Category>(_context);
            Farms = new RepositoryBase<Farm>(_context);
            Products = new RepositoryBase<Product>(_context);
            Orders = new RepositoryBase<Order>(_context);
            OrderItems = new RepositoryBase<OrderItem>(_context);
            Payments = new RepositoryBase<Payment>(_context);
            Reviews = new RepositoryBase<Review>(_context);
            SellerProfiles = new RepositoryBase<SellerProfile>(_context);
            BusinessProfiles = new RepositoryBase<BusinessProfile>(_context);
            PayoutTransactions = new RepositoryBase<PayoutTransaction>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
