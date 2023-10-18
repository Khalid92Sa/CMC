using CMC.Kernel.Core.Enums;
using CMC.Presentation.Application.DTOs.Competitions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System.Linq;

namespace CMC.Presentation.Application.Validators.Competitions
{
    public class AddCompetitionValidator : AbstractValidator<CompetitionsDTO>
    {
        public AddCompetitionValidator(IStringLocalizer<CompetitionsDTO> localizer)
        {
            RuleFor(a => a.Name).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.HostID).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.StartDate).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.CompettionQuestionType).NotNull().WithMessage(localizer["FieldRequired"]);

            When(a => a.CompettionQuestionType == (int)CompetitionQuestionType.QuestionsPerPlayer, () =>
            {
                When(a => !a.IsFinalCompetition, () =>
                {
                    RuleFor(a => a.CompettionQuestionType).Equal(1).WithMessage(localizer["OptionQuestionForEachPlayerOnlyForFinalCompetition"]);
                });

                RuleFor(a => a.Round1Points).NotNull().WithMessage(localizer["FieldRequired"]);
                RuleFor(a => a.Round1Time).NotNull().WithMessage(localizer["FieldRequired"]);
            });

            When(a => a.CompettionQuestionType == (int)CompetitionQuestionType.Rounds, () =>
            {
                RuleFor(a => a.RoundCount).GreaterThan(0).WithMessage(localizer["FieldRequired"]);

                for (int i = 1; i <= 4; i++)
                {
                    int roundNumber = i;

                    When(a => a.RoundCount >= roundNumber, () =>
                    {
                        RuleFor(a => a.GetType().GetProperty($"Round{roundNumber}Points").GetValue(a))
                            .NotNull().WithMessage(localizer["FieldRequired"]).WithName($"Round{roundNumber}Points");

                        RuleFor(a => a.GetType().GetProperty($"Round{roundNumber}Time").GetValue(a))
                            .NotNull().WithMessage(localizer["FieldRequired"]).WithName($"Round{roundNumber}Time");
                    });
                }
            });


            When(a => a.CompettionQuestionType == (int)CompetitionQuestionType.QuestionsPerPlayer, () =>
            {
                RuleFor(a => a.QuestionForEachPlayer)
                 .NotNull().WithMessage(localizer["FieldRequired"]);
            });
           

            RuleFor(a => a.Team1Name).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.Team2Name).NotNull().WithMessage(localizer["FieldRequired"]);

            RuleFor(a => a.Team1.Player1)
            .NotNull()
            .WithMessage(localizer["PleaseSelectAtLeastOnePlayerFromFirstTeam"])
            .WithName("Team1.Player1");

            RuleFor(a => a.Team2.Player1)
            .NotNull()
            .WithMessage(localizer["PleaseSelectAtLeastOnePlayerFromSecondTeam"])
            .WithName("Team2.Player1");

            // Custom validation
            RuleFor(a => a)
                .Custom((dto, context) =>
                {
                    int team1PlayerCount = new[] { dto.Team1.Player1, dto.Team1.Player2, dto.Team1.Player3, dto.Team1.Player4 }
                        .Count(player => player.HasValue);

                    int team2PlayerCount = new[] { dto.Team2.Player1, dto.Team2.Player2, dto.Team2.Player3, dto.Team2.Player4 }
                        .Count(player => player.HasValue);

                    if (team1PlayerCount != team2PlayerCount)
                    {
                        context.AddFailure("Team1.Player1", localizer["TeamsMustHaveSamePlayerCount"]);
                        context.AddFailure("Team2.Player1", localizer["TeamsMustHaveSamePlayerCount"]);
                    }

                    When(a => a.IsFinalCompetition, () =>
                    {
                        if(team1PlayerCount>1 || team2PlayerCount > 1)
                        {
                            context.AddFailure("Team1.Player1", localizer["FinalCompetitionPlayerCountValidation"]);
                            context.AddFailure("Team2.Player1", localizer["FinalCompetitionPlayerCountValidation"]);
                        }
                    });
                });
        }
    }
}
