using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.DTOs.Players;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Identity.Interfaces
{
    public interface IUserService : IApplicationService
    {

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        Task<Response<UserDTO>> CreateOrUpdateUser(UserDTO userDTO);

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response<UserDTO>> GetUser(int id);

        /// <summary>
        /// Get Logged in user
        /// </summary>
        /// <returns></returns>
        UserDTO GetLoggedInUser();
        /// <summary>
        /// Get All users with search
        /// </summary>
        /// <param name="searchUserDTO"></param>
        /// <returns></returns>
        Task<PagedResult<UserListDTO>> GetUsers(SearchUserDTO searchUserDTO);
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        Task<Response<UserDTO>> Login(LoginDTO loginDTO);

        /// <summary>
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeleteUser(int id);

        /// <summary>
        /// Check permission for user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        Task<bool> CheckCurrentUserPermissions(int userId,params string[] permissions);

        /// <summary>
        /// Get all hosts
        /// </summary>
        /// <returns></returns>
        Task<List<LookupModel>> GetHosts();
    }
}
