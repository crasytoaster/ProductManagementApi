namespace ProductManagementApi.Models
{
    public class ApprovalQueue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string RequestReason { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string ActionType { get; set; } // create, update, delete
        public string Status { get; set; } = "pending"; // pending, approved, rejected

        // Navigation property to link to product
        public Product Product { get; set; }
    }
}
