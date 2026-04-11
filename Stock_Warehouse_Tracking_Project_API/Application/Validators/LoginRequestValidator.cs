using FluentValidation;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Auth;

namespace Stock_Warehouse_Tracking_Project_API.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Geçerli bir e-posta giriniz.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
    }
}
