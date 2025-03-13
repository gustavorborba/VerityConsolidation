using ConsolidationApi.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsolidationApi.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        //types defined according to SQLite support
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transaction");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Type)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(t => t.Value)
                   .HasColumnType("NUMERIC")
                   .IsRequired();

            builder.Property(t => t.DateCreated)
                   .HasColumnType("TEXT")
                   .HasConversion(
                       v => v.ToString("yyyy-MM-dd HH:mm:ss"),
                       v => DateTime.Parse(v)
                   )
                   .IsRequired();
        }
    }
}
