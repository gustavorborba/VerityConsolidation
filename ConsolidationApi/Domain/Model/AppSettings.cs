namespace ConsolidationApi.Domain.Model
{
    public class AppSettings
    {
        public MongoSettings MongoSettings { get; set; } = new();
        public string JwtToken { get; set; } = string.Empty;
        public string LedgerConnectionString { get; set; } = string.Empty;
        public string CronConfiguration { get; set; } = string.Empty;
    }
}
