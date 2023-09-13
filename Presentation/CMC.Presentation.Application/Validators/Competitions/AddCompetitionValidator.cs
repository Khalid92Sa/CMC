using CMC.Kernel.Core.Constants;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
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

            RuleFor(a => a.Team1.Player1)
            .NotNull()
            .WithMessage(localizer["PleaseSelectAtLeastOnePlayerFromFirstTeam"])
            .WithName("Team1.Player1");

            RuleFor(a => a.Team2.Player1)
            .NotNull()
            .WithMessage(localizer["PleaseSelectAtLeastOnePlayerFromSecondTeam"])
            .WithName("Team2.Player1");
        }
    }
}
