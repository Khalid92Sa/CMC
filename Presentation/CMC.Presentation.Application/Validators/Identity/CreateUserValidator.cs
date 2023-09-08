using CMC.Kernel.Core.Validators;
using CMC.Presentation.Application.DTOs.Identity;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMC.Presentation.Application.Validators.Identity
{
    public class CreateUserValidator : AbstractValidator<UserDTO>
    {
        public CreateUserValidator(IStringLocalizer<UserDTO> localizer)
        {
            //Name
            RuleFor(a => a.Name).NotNull().WithMessage(localizer["FieldRequired"]);
            
            //UserName
            RuleFor(a => a.UserName).NotNull().WithMessage(localizer["FieldRequired"]);
            
            //Email
            RuleFor(a => a.EmailAddress).NotNull().WithMessage(localizer["FieldRequired"])
                .Matches(RegularExpressionsValidator.EmailAddress).WithMessage("EmailAddressInvalid")
                .MaximumLength(60).WithMessage(String.Format(localizer["FieldMaximumLength"], 60));
            
            //PhoneNumber
            RuleFor(a => a.PhoneNumber).NotNull().WithMessage(localizer["FieldRequired"]);
            
            //Roles
            RuleFor(a => a.Roles)
            .Must(roles => roles != null && roles.Count > 0 && roles.All(role => role.Id != 0))
            .WithMessage(localizer["FieldRequired"]);
        }
    }
}
