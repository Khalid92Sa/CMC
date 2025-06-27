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
    public class BulkQuestionsValidator : AbstractValidator<BulkQuestionsDTO>
    {
        public BulkQuestionsValidator(IStringLocalizer<BulkQuestionsDTO> localizer)
        {
            // Validate that Questions collection is not null or empty
            RuleFor(a => a.Questions)
                .NotNull().WithMessage(localizer["QuestionsCollectionRequired"])
                .NotEmpty().WithMessage(localizer["QuestionsCollectionRequired"]);

            // Validate each question in the collection
            When(a => a.Questions != null && a.Questions.Any(), () =>
            {
                RuleForEach(a => a.Questions).SetValidator(new BulkQuestionItemValidator(localizer));
            });
        }
    }

    public class BulkQuestionItemValidator : AbstractValidator<QuestionVM>
    {
        public BulkQuestionItemValidator(IStringLocalizer<BulkQuestionsDTO> localizer)
        {
            // Category is required
            RuleFor(a => a.CategoryId)
                .NotNull().WithMessage(localizer["FieldRequired"])
                .NotEqual(0).WithMessage(localizer["FieldRequired"]);

            // Question text validation based on culture
            if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar")
            {
                RuleFor(a => a.TextAr)
                    .NotNull().WithMessage(localizer["FieldRequired"])
                    .NotEmpty().WithMessage(localizer["FieldRequired"]);
            }
            else
            {
                RuleFor(a => a.TextEn)
                    .NotNull().WithMessage(localizer["FieldRequired"])
                    .NotEmpty().WithMessage(localizer["FieldRequired"]);
            }

            // Answer type is required
            RuleFor(a => a.AnswertType)
                .NotNull().WithMessage(localizer["FieldRequired"]);

            // Answers collection validation
            RuleFor(a => a.Answers)
                .NotNull().WithMessage(localizer["PleaseAddTwoOptionsAtLeast"]);

            When(a => a.Answers != null && a.Answers.Count > 0, () =>
            {
                // Text answers validation
                When(a => a.AnswertType == (int)AnswersTypes.Text, () =>
                {
                    if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar")
                    {
                        // Arabic Text validation
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
                        // English Text validation
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
                                    if (string.IsNullOrEmpty(answer.TextEn))
                                    {
                                        context.AddFailure(localizer["PleaseCheckTheCorrectAnswerValues"]);
                                    }
                                }
                            });
                    }
                });

                // Image answers validation
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

                            if (existingIds.Count == 1 && newImages.Count == 1)
                            {
                                // Valid because there are two options, one already exists, and one new.
                                return;
                            }

                            context.AddFailure(localizer["PleaseAddTwoImagesAtLeast"]);
                        });
                });

                // At least one correct answer must be specified
                RuleFor(a => a.Answers)
                    .Must(answers => answers.Any(a => a.IsAnswer))
                    .WithMessage(localizer["AtLeastOneCorrectAnswerRequired"]);
            });

            // Optional: Validate points if they exist
            //When(a => a.Points == 0, () =>
            //{
            //    RuleFor(a => a.Points)
            //        .GreaterThan(0).WithMessage(localizer["PointsMustBeGreaterThanZero"]);
            //});
        }
    }
}