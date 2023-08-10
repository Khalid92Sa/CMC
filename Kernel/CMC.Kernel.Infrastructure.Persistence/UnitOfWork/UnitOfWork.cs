using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Persistence;

namespace CMC.Kernel.Infrastructure.Persistence.UnitOfWork
{
    /// <summary>
    /// Unit Of Work
    /// </summary>
    public class UnitOfWork : UnitOfWorkBase<IEntityMapping>
    {
        public UnitOfWork(DbContextOptions<UnitOfWork> options) : base(options) { }
    }
}
