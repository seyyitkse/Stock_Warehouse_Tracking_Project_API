using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.User;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ad alanı zorunludur.")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Geçerli bir rol seçiniz.");
    }
}
