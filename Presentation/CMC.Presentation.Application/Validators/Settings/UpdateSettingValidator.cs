using CMC.Presentation.Application.DTOs;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMC.Presentation.Application.Validators.Settings
{
    public class UpdateSettingValidator : AbstractValidator<SettingDTO>
    {
        public UpdateSettingValidator(IStringLocalizer<SettingDTO> localizer)
        {
            RuleFor(x => x.SystemFontSize)
                .NotEmpty().WithMessage(localizer["SettingSystemFontSizeRequired"])
                .Must(BeValidFontSize).WithMessage(localizer["SettingInvalidFontSize"]);

            RuleFor(x => x.CompetitionFontSize)
                .NotEmpty().WithMessage(localizer["SettingCompetitionFontSizeRequired"])
                .Must(BeValidFontSize).WithMessage(localizer["SettingInvalidFontSize"]);

            When(x => x.BackgroundImg != null, () =>
            {
                RuleFor(x => x.BackgroundImg)
                    .Must(BeValidImageFile).WithMessage(localizer["SettingInvalidImageFile"])
                    .Must(BeValidFileSize).WithMessage(localizer["SettingFileSizeExceeded"]);
            });
        }

        private bool BeValidFontSize(string fontSize)
        {
            if (string.IsNullOrEmpty(fontSize)) return false;

            if (fontSize.EndsWith("px"))
            {
                var numberPart = fontSize.Substring(0, fontSize.Length - 2);
                if (int.TryParse(numberPart, out int size))
                {
                    return size >= 10 && size <= 30; // Allow font sizes between 10px and 30px
                }
            }

            return false;
        }

        private bool BeValidImageFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return true; // Optional file

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();

            return allowedExtensions.Contains(fileExtension);
        }

        private bool BeValidFileSize(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null) return true; // Optional file

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            return file.Length <= maxFileSize;
        }
    }
}
