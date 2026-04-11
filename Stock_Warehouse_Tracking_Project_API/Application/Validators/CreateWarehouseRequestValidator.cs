using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20).WithMessage("Depo kodu zorunludur, en fazla 20 karakter.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Depo adı zorunludur.");
        RuleFor(x => x.Location).MaximumLength(250).When(x => x.Location is not null);
    }
}
