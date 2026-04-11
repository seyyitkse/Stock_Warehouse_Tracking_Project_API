using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class StockOutRequestValidator : AbstractValidator<StockOutRequest>
{
    public StockOutRequestValidator()
    {
        RuleFor(x => x.MaterialNo).NotEmpty().MaximumLength(50).WithMessage("Malzeme numarası zorunludur.");
        RuleFor(x => x.WarehouseId).NotEmpty().MaximumLength(20).WithMessage("Depo kodu zorunludur.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");
        RuleFor(x => x.RefNo).MaximumLength(100).When(x => x.RefNo is not null);
    }
}
