namespace ConsolidationApi.Domain.Model
{
    public class Consolidation : BaseEntity
    {
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal Total { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
