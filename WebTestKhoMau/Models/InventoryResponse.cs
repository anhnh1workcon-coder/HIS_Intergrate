namespace WebTestKhoMau.Models
{
    public class InventoryResponse
    {
        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; }
        public List<InventoryInfo>? InventoryInfo { get; set; }
    }
}
