using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class UpdateWarehouseRequestValidator : AbstractValidator<UpdateWarehouseRequest>
{
    public UpdateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Depo adı zorunludur.");
        RuleFor(x => x.Location).MaximumLength(250).When(x => x.Location is not null);
    }
}
