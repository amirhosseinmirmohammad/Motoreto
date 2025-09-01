using GladcherryShopping.Models;
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
    public class Chat : System.Object
    {
        #region Configuration
        internal class Configuration
            : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Chat>
        {
            public Configuration()
            {
                this.HasKey(current => current.Id);
                this.Property(current => current.Id)
                    .IsRequired()
                    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
                this.Property(current => current.DateTimeStart).IsRequired();
                this.Property(current => current.DateTimeLastModified).IsRequired();
                this.Property(current => current.OnlineUserId).IsRequired();
                this.Property(current => current.OperatorId).IsOptional();
                this.HasOptional(current => current.Operator)
                    .WithMany(operatorb => operatorb.Chats)
                    .HasForeignKey(current => current.OperatorId)
                    .WillCascadeOnDelete(false);
            }
        }
        #endregion Configuration

        public Chat()
        {
            this.DateTimeStart = DateTime.Now;
            this.DateTimeLastModified = DateTime.Now;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        [Bindable(false)]
        public long Id { get; set; }

        [DisplayName("زمان شروع چت")]
        [Display(Name = "زمان شروع چت")]
        [Required]
        public DateTime DateTimeStart { get; set; }

        [DisplayName("تعداد پیام های جدید")]
        [Display(Name = "تعداد پیام های جدید")]
        public int? NewMessagesCount { get; set; }

        public String LastMessage { get; set; }

        [DisplayName("زمان آخرین پیام ارسال شده در این چت روم")]
        [Display(Name = "زمان آخرین پیام ارسال شده در این چت روم")]
        [Required]
        public DateTime DateTimeLastModified { get; set; }

        public String OnlineUserId { get; set; }

        public String OperatorId { get; set; }
        public virtual ApplicationUser Operator { get; set; }

        // Table Relations
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }
}
