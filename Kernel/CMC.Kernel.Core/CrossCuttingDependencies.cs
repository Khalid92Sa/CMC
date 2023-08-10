using CMC.Kernel.Core.Common;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Infrastructure;

namespace CMC.Kernel.Core
{
    public class CrossCuttingDependencies : ICrossCuttingDependencies
    {
        public CrossCuttingDependencies(IApplicationLogger logger, Configuration config)
        {
            Logger = logger;
            Configuration = config;
        }

        public Configuration Configuration { get; }
        public IApplicationLogger Logger { get; }
    }
}
