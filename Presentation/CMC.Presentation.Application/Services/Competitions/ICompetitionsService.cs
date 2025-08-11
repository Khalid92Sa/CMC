using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Domain.Entities;
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
        /// Get Competitions lookup
        /// </summary>
        /// <returns></returns>
        Task<Response<List<LookupModel>>> GetCompetitionsLookup();
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

        /// <summary>
        /// Get Rounds time
        /// </summary>
        /// <param name="competionId"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        int GetRoundTime(int competionId, int round);

        /// <summary>
        /// Get round points
        /// </summary>
        /// <param name="competionId"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        int GetRoundPoints(int competionId, int round);

        /// <summary>
        /// Get Score details for player
        /// </summary>
        /// <param name="competitionId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        Task<Response<CompetitionsPlayerDTO>> GetPlayerScoreDetails(int competitionId,int playerId);

        Task<Response> UpdateCompeititonState(CompetitionStartDTO competitionStartDTO);

        Task<Response<string>> GetBackgroundAttachment();

        Task<Response<CompetitionStartDTO>> GetPlayersProfilePictures(CompetitionStartDTO  competitionStartDTO);
    }
}
