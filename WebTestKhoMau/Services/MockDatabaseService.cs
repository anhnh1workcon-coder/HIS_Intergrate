using WebTestKhoMau.Models;
using System.Text.Json;

namespace WebTestKhoMau.Services
{
    public interface IMockDatabaseService
    {
        Task<List<InventoryInfo>> GetInventoryAsync(string? abo, string? rh, string? elementId, int volume = 0);
        Task<List<InventoryInfo>> GetAllInventoryAsync();
        Task<bool> SavePatientOrderAsync(PatientOrderRequest request);
        Task<bool> SavePatientOrderWithInventoryDeductionAsync(PatientOrderRequest request);
        Task<List<PatientOrderRequest>> GetPatientOrdersAsync();
        Task<object> GetAllDataAsync();
        Task<bool> CreateInventoryAsync(InventoryInfo item);
        Task<bool> UpdateInventoryAsync(int index, InventoryInfo item);
        Task<bool> DeleteInventoryAsync(int index);
        Task<bool> CreatePatientOrderAsync(PatientOrderRequest request);
        Task<bool> UpdatePatientOrderAsync(int index, PatientOrderRequest request);
        Task<bool> DeletePatientOrderAsync(int index);
    }

    public class MockDatabaseService : IMockDatabaseService
    {
        private readonly string _dataFilePath;
        private readonly ILogger<MockDatabaseService> _logger;

        public MockDatabaseService(IWebHostEnvironment env, ILogger<MockDatabaseService> logger)
        {
            _logger = logger;
            _dataFilePath = Path.Combine(env.ContentRootPath, "Data", "mockdb.json");
        }

        public async Task<List<InventoryInfo>> GetInventoryAsync(string? abo, string? rh, string? elementId, int volume = 0)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (!root.TryGetProperty("Inventory", out var inventory))
                    return new List<InventoryInfo>();

                var result = new List<InventoryInfo>();

                foreach (var item in inventory.EnumerateArray())
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                    
                    if (inv == null) continue;

                    // Filter theo từng field nếu có giá trị
                    bool match = true;

                    if (!string.IsNullOrWhiteSpace(abo))
                    {
                        match = match && inv.ABO?.Trim() == abo.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(rh))
                    {
                        match = match && inv.Rh?.Trim() == rh.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(elementId))
                    {
                        match = match && inv.ElementID?.Trim() == elementId.Trim();
                    }

                    if (volume > 0)
                    {
                        match = match && inv.Volume == volume;
                    }

                    if (match)
                    {
                        result.Add(inv);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading inventory");
                return new List<InventoryInfo>();
            }
        }

        public async Task<List<InventoryInfo>> GetAllInventoryAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (!root.TryGetProperty("Inventory", out var inventory))
                    return new List<InventoryInfo>();

                var result = new List<InventoryInfo>();

                foreach (var item in inventory.EnumerateArray())
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                    if (inv != null)
                    {
                        result.Add(inv);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading all inventory");
                return new List<InventoryInfo>();
            }
        }

        public async Task<bool> SavePatientOrderAsync(PatientOrderRequest request)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                
                // Deserialize to dynamic object to manipulate
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, options);

                if (data == null)
                {
                    data = new Dictionary<string, JsonElement>();
                }

                // Create a list to hold the existing orders
                var orders = new List<PatientOrderRequest>();

                if (data.TryGetValue("PatientOrders", out var ordersElement))
                {
                    foreach (var item in ordersElement.EnumerateArray())
                    {
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(
                            item.GetRawText(),
                            options);
                        if (order != null)
                            orders.Add(order);
                    }
                }

                // Add the new order
                orders.Add(request);

                // Create new data structure
                var newData = new Dictionary<string, object>
                {
                    { "Inventory", data.ContainsKey("Inventory") ? (object)data["Inventory"] : new object() },
                    { "PatientOrders", orders }
                };

                // Write back to file
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = null
                });

                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving patient order");
                return false;
            }
        }

        public async Task<List<PatientOrderRequest>> GetPatientOrdersAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var orders = new List<PatientOrderRequest>();

                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var item in ordersElement.EnumerateArray())
                    {
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(
                            item.GetRawText(),
                            options);
                        if (order != null)
                            orders.Add(order);
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading patient orders");
                return new List<PatientOrderRequest>();
            }
        }

        public async Task<object> GetAllDataAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                // Return data with proper structure
                var result = new Dictionary<string, object>();
                
                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    var inventoryList = new List<Dictionary<string, object>>();
                    foreach (var item in inventoryElement.EnumerateArray())
                    {
                        var inv = new Dictionary<string, object>
                        {
                            { "abo", item.GetProperty("ABO").GetString() ?? "" },
                            { "rh", item.GetProperty("Rh").GetString() ?? "" },
                            { "elementID", item.GetProperty("ElementID").GetString() ?? "" },
                            { "elementName", item.GetProperty("ElementName").GetString() ?? "" },
                            { "volume", item.GetProperty("Volume").GetInt32() },
                            { "quantity", item.GetProperty("Quantity").GetInt32() }
                        };
                        inventoryList.Add(inv);
                    }
                    result["inventory"] = inventoryList;
                }
                
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    var ordersList = new List<object>();
                    foreach (var item in ordersElement.EnumerateArray())
                    {
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(
                            item.GetRawText(),
                            options);
                        
                        if (order != null)
                        {
                            ordersList.Add(order);
                        }
                    }
                    result["patientOrders"] = ordersList;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading all data");
                return new { error = ex.Message };
            }
        }

        public async Task<bool> CreateInventoryAsync(InventoryInfo item)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();

                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    foreach (var invItem in inventoryElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var inv = JsonSerializer.Deserialize<InventoryInfo>(invItem.GetRawText(), options);
                        if (inv != null) inventory.Add(inv);
                    }
                }

                inventory.Add(item);

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                        if (order != null) patientOrders.Add(order);
                    }
                }

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                return false;
            }
        }

        public async Task<bool> UpdateInventoryAsync(int index, InventoryInfo item)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();

                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    int currentIndex = 0;
                    foreach (var invItem in inventoryElement.EnumerateArray())
                    {
                        if (currentIndex == index)
                        {
                            inventory.Add(item);
                        }
                        else
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var inv = JsonSerializer.Deserialize<InventoryInfo>(invItem.GetRawText(), options);
                            if (inv != null) inventory.Add(inv);
                        }
                        currentIndex++;
                    }
                }

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                        if (order != null) patientOrders.Add(order);
                    }
                }

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item");
                return false;
            }
        }

        public async Task<bool> DeleteInventoryAsync(int index)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();

                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    int currentIndex = 0;
                    foreach (var invItem in inventoryElement.EnumerateArray())
                    {
                        if (currentIndex != index)
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var inv = JsonSerializer.Deserialize<InventoryInfo>(invItem.GetRawText(), options);
                            if (inv != null) inventory.Add(inv);
                        }
                        currentIndex++;
                    }
                }

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                        if (order != null) patientOrders.Add(order);
                    }
                }

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item");
                return false;
            }
        }

        public async Task<bool> CreatePatientOrderAsync(PatientOrderRequest request)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();
                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    foreach (var item in inventoryElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                        if (inv != null) inventory.Add(inv);
                    }
                }

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                        if (order != null) patientOrders.Add(order);
                    }
                }

                patientOrders.Add(request);

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient order");
                return false;
            }
        }

        public async Task<bool> UpdatePatientOrderAsync(int index, PatientOrderRequest request)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();
                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    foreach (var item in inventoryElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                        if (inv != null) inventory.Add(inv);
                    }
                }

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    int currentIndex = 0;
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        if (currentIndex == index)
                        {
                            patientOrders.Add(request);
                        }
                        else
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                            if (order != null) patientOrders.Add(order);
                        }
                        currentIndex++;
                    }
                }

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient order");
                return false;
            }
        }

        public async Task<bool> DeletePatientOrderAsync(int index)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();
                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    foreach (var item in inventoryElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                        if (inv != null) inventory.Add(inv);
                    }
                }

                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    int currentIndex = 0;
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        if (currentIndex != index)
                        {
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                            if (order != null) patientOrders.Add(order);
                        }
                        currentIndex++;
                    }
                }

                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient order");
                return false;
            }
        }

        public async Task<bool> SavePatientOrderWithInventoryDeductionAsync(PatientOrderRequest request)
        {
            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                var inventory = new List<InventoryInfo>();
                if (root.TryGetProperty("Inventory", out var inventoryElement))
                {
                    foreach (var item in inventoryElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var inv = JsonSerializer.Deserialize<InventoryInfo>(item.GetRawText(), options);
                        if (inv != null) inventory.Add(inv);
                    }
                }

                // Trừ tồn kho cho từng item trong ListOrder
                if (request.ListOrder != null && request.ListOrder.Count > 0)
                {
                    foreach (var orderItem in request.ListOrder)
                    {
                        int requestedQuantity = int.Parse(orderItem.Quantity ?? "0");
                        
                        // Tìm item tồn kho phù hợp
                        // Match dựa trên: ABO, Rh, ElementID, Volume từ order item
                        var matchedInventory = inventory.FirstOrDefault(inv =>
                            inv.ABO?.Trim() == request.BloodGroup?.Trim() &&
                            inv.Rh?.Trim() == request.Rh?.Trim() &&
                            inv.ElementID?.Trim() == orderItem.ElementID?.Trim() &&
                            inv.Volume == orderItem.Volume);

                        if (matchedInventory != null)
                        {
                            // Trừ số lượng
                            matchedInventory.Quantity -= requestedQuantity;
                            _logger.LogInformation($"Deducted {requestedQuantity} units of {orderItem.ElementID} ({request.BloodGroup}{request.Rh}, {orderItem.Volume}ml). Remaining: {matchedInventory.Quantity}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find matching inventory for: ABO={request.BloodGroup}, Rh={request.Rh}, ElementID={orderItem.ElementID}, Volume={orderItem.Volume}ml");
                        }
                    }
                }

                // Lưu lại patient order
                var patientOrders = new List<PatientOrderRequest>();
                if (root.TryGetProperty("PatientOrders", out var ordersElement))
                {
                    foreach (var orderItem in ordersElement.EnumerateArray())
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var order = JsonSerializer.Deserialize<PatientOrderRequest>(orderItem.GetRawText(), options);
                        if (order != null) patientOrders.Add(order);
                    }
                }

                patientOrders.Add(request);

                // Lưu toàn bộ dữ liệu
                var newData = new { Inventory = inventory, PatientOrders = patientOrders };
                var updatedJson = JsonSerializer.Serialize(newData, new JsonSerializerOptions { WriteIndented = true });
                
                await File.WriteAllTextAsync(_dataFilePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving patient order with inventory deduction");
                return false;
            }
        }
    }
}
