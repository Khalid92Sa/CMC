using CMC.Kernel.Core.Enums;
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
            RuleFor(a => a.AnswertType).NotNull().WithMessage(localizer["FieldRequired"]);

            RuleFor(a => a.Answers).NotNull().WithMessage(localizer["PleaseAddTwoOptionsAtLeast"]);
            When(a => a.Answers != null && a.Answers.Count > 0, () =>
            {
                When(a => a.AnswertType == (int)AnswersTypes.Text, () =>
                {
                    if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar")
                    {
                        //Arabic Text
                        RuleFor(a => a.Answers)
                            .Must(answers =>
                                (answers.Count(a => !string.IsNullOrEmpty(a.TextAr)) == 2 &&
                                 answers.Any(a => a.IsAnswer)) ||
                                answers.Count(a => !string.IsNullOrEmpty(a.TextAr)) >= 3)
                            .WithMessage(localizer["PleaseAddTwoOptionsAtLeast"]);

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
                        //English Text
                        RuleFor(a => a.Answers)
                            .Must(answers =>
                                (answers.Count(a => !string.IsNullOrEmpty(a.TextEn)) == 2 &&
                                 answers.Any(a => a.IsAnswer)) ||
                                answers.Count(a => !string.IsNullOrEmpty(a.TextEn)) >= 3)
                            .WithMessage(localizer["PleaseAddTwoOptionsAtLeast"]);

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
                When(a => a.AnswertType == (int)AnswersTypes.Image, () =>
                {
                    RuleFor(a => a.Answers)
                    .Custom((answers, context) =>
                    {
                        var existingIds = answers.Where(a => a.Id != null).ToList();
                        var newImages = answers.Where(a => a.Id == null && a.Img != null).ToList();
                        if (existingIds.Count >= 2 || newImages.Count >= 2)
                        {
                            if (answers.Any(a => a.IsAnswer))
                            {
                                // At least two options with Ids or Images, and at least one is marked as correct
                                return;
                            }
                            else
                            {
                                context.AddFailure(localizer["PleaseCheckTheCorrectAnswerValues"]);
                                return;
                            }
                        }

                        var answerWithNoImages = answers.Where(a => a.IsAnswer && (a.Img == null && a.Id == null)).Any();
                        if (answerWithNoImages && (existingIds.Count > 0 || newImages.Count > 0))
                        {
                            context.AddFailure(localizer["PleaseCheckTheCorrectAnswerValues"]);
                            return;
                        }

                        if(existingIds.Count == 1 && newImages.Count == 1)
                        {
                            //Valid because there is two option, one already is exist, and one new.
                            return;
                        }

                        context.AddFailure(localizer["PleaseAddTwoImagesAtLeast"]);
                    });
                });
            });
        }
    }
}
