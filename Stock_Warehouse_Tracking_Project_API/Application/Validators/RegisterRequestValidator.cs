using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Auth;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("Ad alanı zorunludur.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
        RuleFor(x => x.RoleId).InclusiveBetween(1, 3).WithMessage("Geçerli bir rol seçiniz (1-3).");
    }
}
