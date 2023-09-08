using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CMC.Presentation.Application.Validators.Questions
{
    public class AddQuestionValidator : AbstractValidator<QuestionVM>
    {
        public AddQuestionValidator(IStringLocalizer<QuestionVM> localizer)
        {
            RuleFor(a => a.CategoryId).NotNull().WithMessage(localizer["FieldRequired"])
                .NotEqual(0).WithMessage(localizer["FieldRequired"]);

            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar")
                RuleFor(a => a.TextAr).NotNull().WithMessage(localizer["FieldRequired"]);
            else
                RuleFor(a => a.TextEn).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.Points).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.Time).NotNull().WithMessage(localizer["FieldRequired"]);

            RuleFor(a => a.Answers).NotNull().WithMessage(localizer["PleaseAddTwoOptionsAtLeast"]);
            When(a => a.Answers != null && a.Answers.Count > 0, () =>
            {
                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar")
                {
                    RuleFor(a => a.Answers)
                        .Must(answers =>
                            (answers.Count(a => !string.IsNullOrEmpty(a.TextAr)) == 2 &&
                             answers.Any(a => a.IsAnswer)) ||
                            answers.Count(a => !string.IsNullOrEmpty(a.TextAr)) >= 3)
                        .WithMessage(localizer["PleaseAddTwoOptionsAtLeastAndSelectCorrectAnswer"]);

                    RuleForEach(a => a.Answers)
                    .Custom((answer, context) =>
                    {
                        if (answer.IsAnswer)
                        {
                            if (string.IsNullOrEmpty(answer.TextAr))
                            {
                                context.AddFailure(localizer["PleaseCheckTheCorrectAnswerValues"]);
                            }
                        }
                    });
                }
                else
                {
                    RuleFor(a => a.Answers)
                        .Must(answers =>
                            (answers.Count(a => !string.IsNullOrEmpty(a.TextEn)) == 2 &&
                             answers.Any(a => a.IsAnswer)) ||
                            answers.Count(a => !string.IsNullOrEmpty(a.TextEn)) >= 3)
                        .WithMessage(localizer["PleaseAddTwoOptionsAtLeastAndSelectCorrectAnswer"]);

                    RuleForEach(a => a.Answers)
                     .Custom((answer, context) =>
                     {
                         if (answer.IsAnswer)
                         {
                             if (string.IsNullOrEmpty(answer.TextAr))
                             {
                                 context.AddFailure(localizer["PleaseCheckTheCorrectAnswerValues"]);
                             }
                         }
                     });
                }
            });
        }
    }
}
