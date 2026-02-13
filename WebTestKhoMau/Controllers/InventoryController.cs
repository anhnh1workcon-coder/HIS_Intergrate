using Microsoft.AspNetCore.Mvc;
using WebTestKhoMau.Models;
using WebTestKhoMau.Services;

namespace WebTestKhoMau.Controllers
{
    [ApiController]
    [Route("LisReceiver/web")]
    public class InventoryController : ControllerBase
    {
        private readonly IMockDatabaseService _mockDatabaseService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IMockDatabaseService mockDatabaseService, ILogger<InventoryController> logger)
        {
            _mockDatabaseService = mockDatabaseService;
            _logger = logger;
        }

        /// <summary>
        /// Get inventory information based on blood type, RH factor, element ID and volume
        /// </summary>
        /// <param name="request">Inventory request containing Rh, ABO, ElementID, and Volume (all optional - empty body returns all inventory)</param>
        /// <returns>Inventory information with quantity available</returns>
        [HttpPost("GetInventory")]
        [Produces("application/json")]
        public async Task<ActionResult<InventoryResponse>> GetInventory([FromBody] InventoryRequest? request)
        {
            try
            {
                // Nếu không có request hoặc tất cả các tham số đều null/empty, trả về toàn bộ inventory
                if (request == null || 
                    (string.IsNullOrWhiteSpace(request.ABO) && 
                     string.IsNullOrWhiteSpace(request.Rh) && 
                     string.IsNullOrWhiteSpace(request.ElementID) && 
                     request.Volume == 0))
                {
                    _logger.LogInformation("Getting all inventory (no filters)");
                    var allInventory = await _mockDatabaseService.GetAllInventoryAsync();
                    
                    return Ok(new InventoryResponse
                    {
                        ErrorMessage = null,
                        IsSuccess = true,
                        InventoryInfo = allInventory
                    });
                }

                _logger.LogInformation($"Getting inventory with filters: ABO={request.ABO}, Rh={request.Rh}, ElementID={request.ElementID}, Volume={request.Volume}");

                var inventoryInfo = await _mockDatabaseService.GetInventoryAsync(
                    request.ABO,
                    request.Rh,
                    request.ElementID,
                    request.Volume);

                return Ok(new InventoryResponse
                {
                    ErrorMessage = null,
                    IsSuccess = true,
                    InventoryInfo = inventoryInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory");
                return BadRequest(new InventoryResponse
                {
                    ErrorMessage = ex.Message,
                    IsSuccess = false,
                    InventoryInfo = new List<InventoryInfo>()
                });
            }
        }

        /// <summary>
        /// Get all data from mockdb.json (Inventory and PatientOrders)
        /// </summary>
        /// <returns>All database data as JSON</returns>
        [HttpGet("GetAllData")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> GetAllData()
        {
            try
            {
                _logger.LogInformation("Getting all data from mockdb.json");
                var allData = await _mockDatabaseService.GetAllDataAsync();
                return Ok(allData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all data");
                return BadRequest(new { ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Create a new inventory item
        /// </summary>
        /// <param name="inventoryItem">Inventory item to create</param>
        /// <returns>Success message</returns>
        [HttpPost("CreateInventory")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> CreateInventory([FromBody] InventoryInfo inventoryItem)
        {
            try
            {
                if (inventoryItem == null)
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Inventory item is required" });
                }

                _logger.LogInformation($"Creating inventory item: ABO={inventoryItem.ABO}, Rh={inventoryItem.Rh}, ElementID={inventoryItem.ElementID}");

                var success = await _mockDatabaseService.CreateInventoryAsync(inventoryItem);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Inventory item created successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to create inventory item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Update an inventory item
        /// </summary>
        /// <param name="index">Index of item to update</param>
        /// <param name="inventoryItem">Updated inventory item</param>
        /// <returns>Success message</returns>
        [HttpPut("UpdateInventory/{index}")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> UpdateInventory(int index, [FromBody] InventoryInfo inventoryItem)
        {
            try
            {
                if (inventoryItem == null)
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Inventory item is required" });
                }

                _logger.LogInformation($"Updating inventory item at index {index}");

                var success = await _mockDatabaseService.UpdateInventoryAsync(index, inventoryItem);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Inventory item updated successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to update inventory item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory item");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Delete an inventory item
        /// </summary>
        /// <param name="index">Index of item to delete</param>
        /// <returns>Success message</returns>
        [HttpDelete("DeleteInventory/{index}")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> DeleteInventory(int index)
        {
            try
            {
                _logger.LogInformation($"Deleting inventory item at index {index}");

                var success = await _mockDatabaseService.DeleteInventoryAsync(index);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Inventory item deleted successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to delete inventory item" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }
    }
}
