using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Competitions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Competitions
{
    public interface ICompetitionsService : IApplicationService
    {
        /// <summary>
        /// Add or update competition
        /// </summary>
        /// <param name="competitionsDTO"></param>
        /// <returns></returns>
        Task<Response> AddOrUpdateCompetition(CompetitionsDTO competitionsDTO);

        /// <summary>
        /// Get all competitions for Host
        /// </summary>
        /// <param name="hostId"></param>
        /// <returns></returns>
        Task<Response<List<CompetitionsDTO>>> GetCompetitionByHostId(int  hostId);
    }
}
