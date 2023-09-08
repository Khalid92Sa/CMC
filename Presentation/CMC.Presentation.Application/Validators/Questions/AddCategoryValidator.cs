using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.DTOs.Questions;
using FluentValidation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.Validators.Questions
{
    public class AddCategoryValidator : AbstractValidator<CategoryDTO>
    {
        public AddCategoryValidator(IStringLocalizer<CategoryDTO> localizer)
        {
            RuleFor(a => a.NameAr).NotNull().WithMessage(localizer["FieldRequired"]);
        }
    }
}
