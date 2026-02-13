using Microsoft.AspNetCore.Mvc;
using WebTestKhoMau.Models;
using WebTestKhoMau.Services;

namespace WebTestKhoMau.Controllers
{
    [ApiController]
    [Route("LisReceiver/web")]
    public class PatientOrderController : ControllerBase
    {
        private readonly IMockDatabaseService _mockDatabaseService;
        private readonly IValidationService _validationService;
        private readonly ILogService _logService;
        private readonly ILogger<PatientOrderController> _logger;

        public PatientOrderController(IMockDatabaseService mockDatabaseService, IValidationService validationService, ILogService logService, ILogger<PatientOrderController> logger)
        {
            _mockDatabaseService = mockDatabaseService;
            _validationService = validationService;
            _logService = logService;
            _logger = logger;
        }

        /// <summary>
        /// Save patient order with blood transfusion details
        /// </summary>
        /// <param name="request">Patient order request containing patient and order information</param>
        /// <returns>Success or failure response</returns>
        [HttpPost("SavePatient")]
        [Produces("application/json")]
        public async Task<ActionResult<PatientOrderResponse>> SavePatient([FromBody] PatientOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    var errorResponse = new PatientOrderResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Request body is required"
                    };
                    await _logService.LogApiCallAsync("SavePatient", request, errorResponse, "Failed", "Request body is required");
                    return BadRequest(errorResponse);
                }

                // Validate dữ liệu đầu vào với kiểm tra tồn kho
                var (isValid, errorMessage) = await _validationService.ValidatePatientOrderWithInventoryAsync(request, _mockDatabaseService);
                if (!isValid)
                {
                    _logger.LogWarning($"Validation failed: {errorMessage}");
                    var validationErrorResponse = new PatientOrderResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage
                    };
                    await _logService.LogApiCallAsync("SavePatient", request, validationErrorResponse, "Failed", errorMessage);
                    return BadRequest(validationErrorResponse);
                }

                _logger.LogInformation($"Saving patient order - OrderID: {request.OrderID}, PatientName: {request.PatientName}");

                // Lưu và trừ tồn kho
                var result = await _mockDatabaseService.SavePatientOrderWithInventoryDeductionAsync(request);

                if (result)
                {
                    var successResponse = new PatientOrderResponse
                    {
                        IsSuccess = true,
                        ErrorMessage = ""
                    };
                    await _logService.LogApiCallAsync("SavePatient", request, successResponse, "Success");
                    return Ok(successResponse);
                }

                var failureResponse = new PatientOrderResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to save patient order"
                };
                await _logService.LogApiCallAsync("SavePatient", request, failureResponse, "Failed", "Failed to save patient order");
                return BadRequest(failureResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving patient order");
                var exceptionResponse = new PatientOrderResponse
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
                await _logService.LogApiCallAsync("SavePatient", request, exceptionResponse, "Error", ex.Message);
                return BadRequest(exceptionResponse);
            }
        }

        /// <summary>
        /// Get all patient orders
        /// </summary>
        /// <returns>List of all patient orders</returns>
        [HttpGet("GetPatientOrders")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<List<PatientOrderRequest>>> GetPatientOrders()
        {
            try
            {
                _logger.LogInformation("Getting all patient orders");

                var orders = await _mockDatabaseService.GetPatientOrdersAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient orders");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new patient order
        /// </summary>
        /// <param name="request">Patient order to create</param>
        /// <returns>Success message</returns>
        [HttpPost("CreatePatientOrder")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> CreatePatientOrder([FromBody] PatientOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Patient order is required" });
                }

                // Validate dữ liệu đầu vào
                var (isValid, errorMessage) = _validationService.ValidatePatientOrder(request);
                if (!isValid)
                {
                    _logger.LogWarning($"Validation failed: {errorMessage}");
                    return BadRequest(new { IsSuccess = false, ErrorMessage = errorMessage });
                }

                _logger.LogInformation($"Creating patient order: OrderID={request.OrderID}, PatientName={request.PatientName}");

                var success = await _mockDatabaseService.CreatePatientOrderAsync(request);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Patient order created successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to create patient order" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient order");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Update a patient order
        /// </summary>
        /// <param name="index">Index of order to update</param>
        /// <param name="request">Updated patient order</param>
        /// <returns>Success message</returns>
        [HttpPut("UpdatePatientOrder/{index}")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> UpdatePatientOrder(int index, [FromBody] PatientOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Patient order is required" });
                }

                // Validate dữ liệu đầu vào
                var (isValid, errorMessage) = _validationService.ValidatePatientOrder(request);
                if (!isValid)
                {
                    _logger.LogWarning($"Validation failed: {errorMessage}");
                    return BadRequest(new { IsSuccess = false, ErrorMessage = errorMessage });
                }

                _logger.LogInformation($"Updating patient order at index {index}");

                var success = await _mockDatabaseService.UpdatePatientOrderAsync(index, request);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Patient order updated successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to update patient order" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient order");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Delete a patient order
        /// </summary>
        /// <param name="index">Index of order to delete</param>
        /// <returns>Success message</returns>
        [HttpDelete("DeletePatientOrder/{index}")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<object>> DeletePatientOrder(int index)
        {
            try
            {
                _logger.LogInformation($"Deleting patient order at index {index}");

                var success = await _mockDatabaseService.DeletePatientOrderAsync(index);
                
                if (success)
                {
                    return Ok(new { IsSuccess = true, ErrorMessage = "", Message = "Patient order deleted successfully" });
                }
                else
                {
                    return BadRequest(new { IsSuccess = false, ErrorMessage = "Failed to delete patient order" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient order");
                return BadRequest(new { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }
    }
}
