namespace ProductManagementApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "inactive"; // Default value is inactive
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string ApprovalStatus { get; set; } = "approved"; // pending, approved, or rejected
        public decimal? PreviousPrice { get; set; } = null;
    }
}

// Models/ApprovalQueue.cs
