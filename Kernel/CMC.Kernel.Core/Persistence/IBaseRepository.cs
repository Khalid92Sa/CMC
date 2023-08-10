using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Kernel.Core.Persistence
{
    public interface IBaseRepository
    {
        IUnitOfWork UnitOfWork { get; }
    }
}
