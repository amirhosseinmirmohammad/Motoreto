using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class ChatMessage : System.Object
    {
        #region Configuration
        internal class Configuration
            : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<ChatMessage>
        {
            public Configuration()
            {
                this.HasKey(current => current.Id);
                this.Property(current => current.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
                this.Property(current => current.Body).IsRequired();
                this.Property(current => current.DateTimeSent).IsRequired();
                this.Property(current => current.OnlineUserId).IsOptional();
                this.Property(current => current.OperatorId).IsOptional();
                this.Property(current => current.ChatId).IsRequired();
                this.HasRequired(current => current.Chat)
                    .WithMany(chat => chat.Messages)
                    .HasForeignKey(current => current.ChatId)
                    .WillCascadeOnDelete(true);
            }
        }
        #endregion Configuration

        public ChatMessage()
        {
            DateTimeSent = DateTime.Now;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        [Bindable(false)]
        public long Id { get; set; }

        [DisplayName("متن پیام")]
        [Display(Name = "متن پیام")]
        [Required(ErrorMessage = "لطفا متن پیام را تعیین نمایید .")]
        public string Body { get; set; }

        [DisplayName("زمان ارسال پیام")]
        [Display(Name = "زمان ارسال پیام")]
        [Required]
        public DateTime DateTimeSent { get; set; }

        public String OnlineUserId { get; set; }

        public String OperatorId { get; set; }

        public long ChatId { get; set; }
        public virtual Chat Chat { get; set; }
    }
}
