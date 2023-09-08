using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
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
        /// Delete player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeletePlayer(int id);
    }
}
