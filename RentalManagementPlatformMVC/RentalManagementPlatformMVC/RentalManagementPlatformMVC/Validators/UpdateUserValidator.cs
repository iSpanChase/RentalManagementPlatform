using FluentValidation;
using RentalManagementPlatformMVC.UserDTOs;

namespace RentalManagementPlatformMVC.Validators
{
	public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
	{
		public UpdateUserValidator()
		{
			RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(512);
			RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
			RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
			RuleFor(x => x.ProfileImageurl).MaximumLength(512).When(x => !string.IsNullOrWhiteSpace(x.ProfileImageurl));
		}
	}
}
