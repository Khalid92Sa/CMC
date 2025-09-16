using CMC.Kernel.Core.Services;
using CMC.Presentation.Application.DTOs.Competitions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMC.Presentation.Application.Services.Competitions
{
    public interface ICompetitionUpdateQueue
    {
        void QueueUpdate(CompetitionStateDto competitionStateData);
    }
}
