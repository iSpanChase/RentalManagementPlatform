using FluentValidation;
using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;

namespace RentalManagementPlatformMVC.Areas.User.Validators
{
	public class CreateUserValidator : AbstractValidator<CreateUserDto>
	{
		public CreateUserValidator()
		{
			RuleFor(x => x.Username).NotEmpty().MaximumLength(512);
			RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(512);
			RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
			RuleFor(x => x.PasswordHash).NotEmpty().MinimumLength(8);
			RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
			RuleFor(x => x.ProfileImageurl).MaximumLength(512).When(x => !string.IsNullOrWhiteSpace(x.ProfileImageurl));
		}
	}
}
