using FluentValidation;
using Microsoft.Extensions.Localization;
using CMC.Kernel.Core.Constants;
using CMC.Presentation.Application.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.Validators.Identity
{
    /// <summary>
    /// Command Login Validator
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginDTO>
    {
        public LoginValidator(IStringLocalizer<LoginDTO> localizer)
        {
            //UserName
            RuleFor(a => a.UserName).NotNull().WithMessage(localizer["FieldRequired"]);
            //Password
            RuleFor(a => a.Password).NotNull().WithMessage(localizer["FieldRequired"]);
        }
    }
}
