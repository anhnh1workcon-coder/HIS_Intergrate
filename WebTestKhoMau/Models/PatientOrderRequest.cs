namespace WebTestKhoMau.Models
{
    using System.Text.Json.Serialization;

    public class PatientOrderRequest
    {
        [JsonPropertyName("PID")]
        public string? PID { get; set; }

        [JsonPropertyName("OrderID")]
        public string? OrderID { get; set; }

        [JsonPropertyName("PatientName")]
        public string? PatientName { get; set; }

        [JsonPropertyName("InsureNumber")]
        public string? InsureNumber { get; set; }

        [JsonPropertyName("TREATMENT_CODE")]
        public string? TREATMENT_CODE { get; set; }

        [JsonPropertyName("OrderDate")]
        public string? OrderDate { get; set; }

        [JsonPropertyName("Age")]
        public string? Age { get; set; }

        [JsonPropertyName("Sex")]
        public string? Sex { get; set; }

        [JsonPropertyName("BloodGroup")]
        public string? BloodGroup { get; set; }

        [JsonPropertyName("Rh")]
        public string? Rh { get; set; }

        [JsonPropertyName("Address")]
        public string? Address { get; set; }

        [JsonPropertyName("DoctorID")]
        public string? DoctorID { get; set; }

        [JsonPropertyName("DoctorName")]
        public string? DoctorName { get; set; }

        [JsonPropertyName("LocationID")]
        public string? LocationID { get; set; }

        [JsonPropertyName("LocationName")]
        public string? LocationName { get; set; }

        [JsonPropertyName("ListOrder")]
        public List<OrderItem>? ListOrder { get; set; }
    }
}
