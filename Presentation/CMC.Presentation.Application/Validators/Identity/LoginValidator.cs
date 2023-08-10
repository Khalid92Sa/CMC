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

            When(x => !x.IsRIB, () =>
            {
                RuleFor(x => x.MobileNumber).NotNull().WithMessage(localizer["FieldRequired"])
               .Matches(RegularExpressionsSettings.SaudiMobileNumber).WithMessage(localizer["InvalidSaudiMobile"]);
            });

            //When(x => x.IsMortgage, () =>
            //{
            //    RuleFor(x => x.MobileNumber).NotNull().WithMessage(localizer["FieldRequired"])
            //   .Matches(RegularExpressionsSettings.SaudiMobileNumber).WithMessage(localizer["InvalidSaudiMobile"]);
            //});


            //When(x => !x.IsMortgage && !x.IsRIB, () =>
            //{
            //    RuleFor(x => x.NationalID).NotNull().WithMessage(localizer["FieldRequired"])
            //         .Matches(RegularExpressionsSettings.IDIqamaNumber).WithMessage(localizer["ID_IQ_Format"]);

            //    When(x => !x.IsLogin, () =>
            //    {
            //        //Register validation
            //        RuleFor(x => x.MobileNumber).NotNull().WithMessage(localizer["FieldRequired"])
            //        .Matches(RegularExpressionsSettings.SaudiMobileNumber).WithMessage(localizer["InvalidSaudiMobile"]);

            //        RuleFor(x => x.Captcha).NotNull().WithMessage(localizer["FieldRequired"])
            //        .Equal(a => a.MatchCaptcha).WithMessage(localizer["CaptchaInvalidCode"]);

            //        RuleFor(x => x.TCAccepted).Equal(true).WithMessage(localizer["FieldRequired"]);
            //    });
            //});
        }
    }
}
