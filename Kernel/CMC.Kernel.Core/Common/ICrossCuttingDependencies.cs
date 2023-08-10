using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Infrastructure;

namespace CMC.Kernel.Core.Common
{
    public interface ICrossCuttingDependencies
    {
        IApplicationLogger Logger { get; }
        Configuration Configuration { get; }
    }
}
