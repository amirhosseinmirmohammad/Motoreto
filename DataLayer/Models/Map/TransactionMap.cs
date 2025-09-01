using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models.Mapping
{
    class TransactionMap
      : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Transaction>
    {
        public TransactionMap()
        {
            this.Property(current => current.Acceptor)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode()
                .IsVariableLength();

            this.Property(current => current.AcceptorPhoneNumber)
                .IsRequired();

            this.Property(current => current.AcceptorPostalCode)
                .IsRequired();

            this.Property(current => current.BankName)
                .HasMaxLength(50)
                .IsRequired()
                .IsUnicode()
                .IsVariableLength();

            this.Property(current => current.CardNumber)
                .IsRequired();

            this.Property(current => current.InvoiceNumber)
                .IsRequired();

            this.Property(current => current.Number)
                .IsRequired();

            //this.Property(current => current.OrderId)
            //    .IsRequired();

            this.HasOptional(current => current.Order)
                .WithMany(Order => Order.Transactions)
                .HasForeignKey(current => current.OrderId)
                .WillCascadeOnDelete(false);

            this.Property(current => current.RecievedDocumentNumber)
                .IsRequired();

            this.Property(current => current.RecievedDocumentDate)
                .IsOptional();

            this.Property(current => current.TerminalNumber)
                .IsRequired();
        }
    }
}
