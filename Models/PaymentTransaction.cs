using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web1.Models
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "COD"; // COD, VNPay, BankTransfer

        [StringLength(100)]
        public string TransactionId { get; set; } // VNPay transaction number

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed, Cancelled

        [StringLength(500)]
        public string ResponseMessage { get; set; } // VNPay response message

        [StringLength(1000)]
        public string RawResponse { get; set; } // Log full VNPay response (JSON)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }
    }
}
