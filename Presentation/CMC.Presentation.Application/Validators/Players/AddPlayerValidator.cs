using CMC.Kernel.Core.Constants;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.DTOs.Questions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.Validators.Players
{
    public class AddPlayerValidator : AbstractValidator<PlayerDTO>
    {
        public AddPlayerValidator(IStringLocalizer<PlayerDTO> localizer)
        {
            RuleFor(a => a.Name).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.PhoneNumber).NotNull().WithMessage(localizer["FieldRequired"])
                .Matches(RegularExpressionsSettings.OnlyNumbers).WithMessage(localizer["InvalidPhoneNumber"]);
            RuleFor(a => a.EmailAddress).Matches(RegularExpressionsSettings.EmailAddress).WithMessage(localizer["InvalidEmailAddress"]);
            RuleFor(a => a.IsEmployee).NotNull().WithMessage(localizer["FieldRequired"]);
        }
    }
}
