namespace CMC.Kernel.Core.Configurations
{
    public class Configuration
    {
        public virtual ConnectionString ConnectionStrings { get; set; }
        public bool HttpLogEnabled { get; set; }
        public bool ExceptionLogEnabled { get; set; }
    }
}
