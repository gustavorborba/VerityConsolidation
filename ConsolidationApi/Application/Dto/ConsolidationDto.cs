namespace ConsolidationApi.Application.Dto
{
    public class ConsolidationDto
    {
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal Total { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
