using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
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
        /// Finish competition
        /// </summary>
        /// <param name="competitionsDTO"></param>
        /// <returns></returns>
        Task<Response> FinishCompetition(CompetitionsDTO competitionsDTO);
        /// <summary>
        /// Get all competitions for Host
        /// </summary>
        /// <param name="hostId"></param>
        /// <returns></returns>
        Task<Response<List<CompetitionsDTO>>> GetCompetitionByHostId(int  hostId);

        /// <summary>
        /// Get Competitions by search
        /// </summary>
        /// <param name="searchCompetitionDTO"></param>
        /// <returns></returns>
        Task<PagedResult<CompetitionListDTO>> GetCompetitions(SearchCompetitionDTO searchCompetitionDTO);
        
        /// <summary>
        /// Get Competition by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<Response<CompetitionsDTO>> GetCompetition(int Id);

        /// <summary>
        /// Get Competition for view score
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<Response<ViewCompetitionScoresDTO>> ViewCompetitionScore(int Id);

        /// <summary>
        /// Get Latest scores for last competitions
        /// </summary>
        /// <returns></returns>
        Task<Response<List<LatestCompeitionsScore>>> GetLatestScores();

        /// <summary>
        /// Delete Competition
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeleteCompetition(int id);

        /// <summary>
        /// Start competition
        /// </summary>
        /// <returns></returns>
        Task<Response<CompetitionStartDTO>> StartCompetiton(int id);

        /// <summary>
        /// Players answered on questions
        /// </summary>
        /// <param name="answerOnQuestionDTO"></param>
        /// <returns></returns>
        Task<Response> AnswerOnQuestions(int competitionId,AnswerOnQuestionDTO answerOnQuestionDTO);
    }
}
