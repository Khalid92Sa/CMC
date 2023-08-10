using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Persistence;

namespace CMC.Kernel.Infrastructure.Persistence.Repositories
{
    public abstract class BaseRepository : IBaseRepository
    {
        public IUnitOfWork UnitOfWork { get; }
        protected DbContext DbContext { get; }

        public BaseRepository(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
            DbContext = unitOfWork as DbContext;
        }
    }
}
