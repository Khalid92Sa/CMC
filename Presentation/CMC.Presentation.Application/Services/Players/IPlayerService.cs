using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Players;
using CMC.Presentation.Application.DTOs.Questions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Players
{
    public interface IPlayerService : IApplicationService
    {
        /// <summary>
        /// Add Or update Player
        /// </summary>
        /// <param name="categoriesVM"></param>
        /// <returns></returns>
        Task<Response> AddOrUpdatePlayer(PlayerDTO playerDTO);

        /// <summary>
        /// Get Players
        /// </summary>
        /// <returns></returns>
        Task<PagedResult<PlayerDTO>> GetPlayers(SearchPlayersDTO searchPlayers);

        /// <summary>
        /// Get player by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response<PlayerDTO>> GetPlayer(int id);

        /// <summary>
        /// Get All players based on if they are a City mall team or not.
        /// </summary>
        /// <param name="isCityMall"></param>
        /// <returns></returns>
        Task<Response<List<LookupModel>>> GetPlayers(bool isCityMall);

        /// <summary>
        /// Delete player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeletePlayer(int id);
    }
}
