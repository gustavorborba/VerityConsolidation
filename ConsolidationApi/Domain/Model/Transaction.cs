namespace ConsolidationApi.Domain.Model
{
    public class Transaction : BaseMongo
    {
        public TransactionType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
