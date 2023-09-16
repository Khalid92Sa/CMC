using CMC.Kernel.Core.Constants;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMC.Presentation.Application.Validators.Competitions
{
    public class AddCompetitionValidator : AbstractValidator<CompetitionsDTO>
    {
        public AddCompetitionValidator(IStringLocalizer<CompetitionsDTO> localizer)
        {
            RuleFor(a => a.Name).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.HostID).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.StartDate).NotNull().WithMessage(localizer["FieldRequired"]);
            RuleFor(a => a.QuestionCount).NotNull().WithMessage(localizer["FieldRequired"]);

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
                    int team1PlayerCount = new[] { dto.Team1.Player1, dto.Team1.Player2, dto.Team1.Player3 }
                        .Count(player => player.HasValue);

                    int team2PlayerCount = new[] { dto.Team2.Player1, dto.Team2.Player2, dto.Team2.Player3 }
                        .Count(player => player.HasValue);

                    if (team1PlayerCount != team2PlayerCount)
                    {
                        context.AddFailure("Team1.Player1", localizer["TeamsMustHaveSamePlayerCount"]);
                        context.AddFailure("Team2.Player1", localizer["TeamsMustHaveSamePlayerCount"]);
                    }

                    if (dto.QuestionCount < team1PlayerCount)
                    {
                        context.AddFailure("QuestionCount", localizer["QuestionCountMustBeGreaterThanOrEqualToPlayerCount"]);
                    }
                });
        }
    }
}
