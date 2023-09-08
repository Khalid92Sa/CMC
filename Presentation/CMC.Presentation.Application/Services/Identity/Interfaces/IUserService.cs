using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Identity;
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
        Task<Response<UserDTO>> CreateUser(UserDTO userDTO);

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        Task<Response<UserDTO>> Login(LoginDTO loginDTO);

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        Task<Response> Logout();

        
    }
}
