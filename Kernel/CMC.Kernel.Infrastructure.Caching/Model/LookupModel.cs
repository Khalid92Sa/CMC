namespace CMC.Kernel.Infrastructure.Caching.Model
{
    /// <summary>
    /// Global Lookup Model 
    /// </summary>
    public class LookupModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string OtherCode { get; set; }
        public bool IsHighRisk { get; set; }
    }
}
