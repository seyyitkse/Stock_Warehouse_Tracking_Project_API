using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class StockTransferRequestValidator : AbstractValidator<StockTransferRequest>
{
    public StockTransferRequestValidator()
    {
        RuleFor(x => x.MaterialNo).NotEmpty().MaximumLength(50).WithMessage("Malzeme numarası zorunludur.");
        RuleFor(x => x.SourceWarehouseId).NotEmpty().MaximumLength(20).WithMessage("Kaynak depo kodu zorunludur.");
        RuleFor(x => x.DestWarehouseId).NotEmpty().MaximumLength(20).WithMessage("Hedef depo kodu zorunludur.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");
        RuleFor(x => x).Must(x => x.SourceWarehouseId != x.DestWarehouseId)
            .WithMessage("Kaynak ve hedef depo aynı olamaz.");
        RuleFor(x => x.RefNo).MaximumLength(100).When(x => x.RefNo is not null);
    }
}
