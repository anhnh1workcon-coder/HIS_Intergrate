using WebTestKhoMau.Models;

namespace WebTestKhoMau.Services
{
    public interface IValidationService
    {
        (bool isValid, string errorMessage) ValidatePatientOrder(PatientOrderRequest request);
        Task<(bool isValid, string errorMessage)> ValidatePatientOrderWithInventoryAsync(PatientOrderRequest request, IMockDatabaseService databaseService);
    }

    public class ValidationService : IValidationService
    {
        public (bool isValid, string errorMessage) ValidatePatientOrder(PatientOrderRequest request)
        {
            if (request == null)
            {
                return (false, "Request không được phép null");
            }

            // Validate bắt buộc: PID
            if (string.IsNullOrWhiteSpace(request.PID))
            {
                return (false, "PID là bắt buộc (Mã số bệnh nhân không được để trống)");
            }

            // Validate bắt buộc: OrderID
            if (string.IsNullOrWhiteSpace(request.OrderID))
            {
                return (false, "OrderID là bắt buộc (Mã phiếu không được để trống)");
            }

            // Validate bắt buộc: PatientName
            if (string.IsNullOrWhiteSpace(request.PatientName))
            {
                return (false, "PatientName là bắt buộc (Tên bệnh nhân không được để trống)");
            }

            // Validate bắt buộc: OrderDate
            if (string.IsNullOrWhiteSpace(request.OrderDate))
            {
                return (false, "OrderDate là bắt buộc (Thời gian chỉ định không được để trống)");
            }

            // Validate định dạng OrderDate
            if (!DateTime.TryParse(request.OrderDate, out _))
            {
                return (false, $"OrderDate không hợp lệ (Định dạng: 'yyyy-MM-dd HH:mm:ss', nhận được: '{request.OrderDate}')");
            }

            // Validate bắt buộc: Age
            if (string.IsNullOrWhiteSpace(request.Age))
            {
                return (false, "Age là bắt buộc (Tuổi không được để trống)");
            }

            // Validate Age là số dương
            if (!int.TryParse(request.Age, out int age) || age < 0 || age > 150)
            {
                return (false, $"Age không hợp lệ (Phải là số từ 0 đến 150, nhận được: '{request.Age}')");
            }

            // Validate bắt buộc: Sex
            if (string.IsNullOrWhiteSpace(request.Sex))
            {
                return (false, "Sex là bắt buộc (Giới tính không được để trống)");
            }

            // Validate Sex (chỉ M hoặc F)
            if (request.Sex != "M" && request.Sex != "F")
            {
                return (false, $"Sex không hợp lệ (Chỉ chấp nhận 'M' hoặc 'F', nhận được: '{request.Sex}')");
            }

            // Validate BloodGroup nếu có (A, B, AB, O)
            if (!string.IsNullOrWhiteSpace(request.BloodGroup))
            {
                if (request.BloodGroup != "A" && request.BloodGroup != "B" && 
                    request.BloodGroup != "AB" && request.BloodGroup != "O")
                {
                    return (false, $"BloodGroup không hợp lệ (Chỉ chấp nhận 'A', 'B', 'AB', 'O', nhận được: '{request.BloodGroup}')");
                }
            }

            // Validate Rh nếu có (+ hoặc -)
            if (!string.IsNullOrWhiteSpace(request.Rh))
            {
                if (request.Rh != "+" && request.Rh != "-")
                {
                    return (false, $"Rh không hợp lệ (Chỉ chấp nhận '+' hoặc '-', nhận được: '{request.Rh}')");
                }
            }

            // Validate ListOrder nếu có
            if (request.ListOrder != null && request.ListOrder.Count > 0)
            {
                for (int i = 0; i < request.ListOrder.Count; i++)
                {
                    var item = request.ListOrder[i];

                    if (string.IsNullOrWhiteSpace(item.Quantity))
                    {
                        return (false, $"ListOrder[{i}].Quantity không được để trống");
                    }

                    if (!int.TryParse(item.Quantity, out int quantity) || quantity < 1)
                    {
                        return (false, $"ListOrder[{i}].Quantity không hợp lệ (Phải là số dương, nhận được: '{item.Quantity}')");
                    }

                    if (string.IsNullOrWhiteSpace(item.ElementID))
                    {
                        return (false, $"ListOrder[{i}].ElementID không được để trống");
                    }

                    if (item.Volume <= 0)
                    {
                        return (false, $"ListOrder[{i}].Volume không hợp lệ (Phải lớn hơn 0, nhận được: {item.Volume})");
                    }
                }
            }

            return (true, "");
        }

        public async Task<(bool isValid, string errorMessage)> ValidatePatientOrderWithInventoryAsync(PatientOrderRequest request, IMockDatabaseService databaseService)
        {
            // Validate cơ bản trước
            var (isValid, errorMessage) = ValidatePatientOrder(request);
            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Kiểm tra tồn kho cho từng item trong ListOrder
            if (request.ListOrder != null && request.ListOrder.Count > 0)
            {
                for (int i = 0; i < request.ListOrder.Count; i++)
                {
                    var item = request.ListOrder[i];
                    
                    // Lấy thông tin tồn kho
                    var inventoryInfo = await databaseService.GetInventoryAsync(
                        request.BloodGroup,
                        request.Rh,
                        item.ElementID);

                    // Tìm item có Volume phù hợp
                    var matchedInventory = inventoryInfo.FirstOrDefault(inv => inv.Volume == item.Volume);

                    if (matchedInventory == null)
                    {
                        return (false, $"ListOrder[{i}]: Không tìm thấy túi máu loại '{item.ElementID}' với nhóm máu {request.BloodGroup}{request.Rh}, thể tích {item.Volume}ml trong kho");
                    }

                    int requestedQuantity = int.Parse(item.Quantity ?? "0");
                    if (matchedInventory.Quantity < requestedQuantity)
                    {
                        return (false, $"ListOrder[{i}]: Không đủ tồn kho. Yêu cầu {requestedQuantity} túi '{item.ElementID}' ({request.BloodGroup}{request.Rh}, {item.Volume}ml), còn lại {matchedInventory.Quantity} túi");
                    }
                }
            }

            return (true, "");
        }
    }
}
