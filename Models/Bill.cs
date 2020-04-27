using System;

namespace Shared_House_Bills.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public int BillTypeId { get; set; }
        public double Value { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}