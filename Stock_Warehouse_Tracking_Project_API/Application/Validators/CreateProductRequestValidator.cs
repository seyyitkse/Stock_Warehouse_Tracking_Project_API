using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Product;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).WithMessage("Ürün kodu zorunludur, en fazla 50 karakter.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Ürün adı zorunludur.");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20).WithMessage("Birim zorunludur (örn: KG, PC, L).");
        RuleFor(x => x.Category).MaximumLength(100).When(x => x.Category is not null);
        RuleFor(x => x.Barcode).MaximumLength(100).When(x => x.Barcode is not null);
    }
}
