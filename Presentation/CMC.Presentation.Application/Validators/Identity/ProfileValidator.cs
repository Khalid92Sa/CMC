using CMC.Kernel.Core.Validators;
using CMC.Presentation.Application.DTOs.Identity;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.Validators.Identity
{
    public class ProfileValidator : AbstractValidator<ProfileDTO>
    {
        public ProfileValidator(IStringLocalizer<ProfileDTO> localizer)
        {
            //Name
            RuleFor(a => a.Name).NotNull().WithMessage(localizer["FieldRequired"]);

            //Email
            RuleFor(a => a.EmailAddress).NotNull().WithMessage(localizer["FieldRequired"])
                .Matches(RegularExpressionsValidator.EmailAddress).WithMessage(localizer["EmailAddressInvalid"])
                .MaximumLength(60).WithMessage(String.Format(localizer["FieldMaximumLength"], 60));

            //PhoneNumber
            RuleFor(a => a.PhoneNumber).NotNull().WithMessage(localizer["FieldRequired"]);

            When(a => !string.IsNullOrEmpty(a.NewPassword), () =>
            {
                RuleFor(a => a.CurrentPassword).NotNull().WithMessage(localizer["FieldRequired"]);
                RuleFor(a => a.ConfirmNewPassword).NotNull().WithMessage(localizer["FieldRequired"]);
                RuleFor(a => a.ConfirmNewPassword).Equal(a => a.NewPassword).WithMessage(localizer["PasswordsDoNotMatch"]);
            });
        }
    }
}
