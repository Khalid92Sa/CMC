using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        void BeginTransaction();
        Task<int> SaveChangesAsync();
        void Commit();
        void Rollback();
    }
}
