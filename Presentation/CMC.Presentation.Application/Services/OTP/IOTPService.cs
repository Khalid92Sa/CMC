using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Presentation.Application.DTOs.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.OTP
{
    public interface IOTPService : IApplicationService
    {
        Task<Response> Test();
        Task<Response<OneTimePasswordDTO>> GetOTPModel(string mobile, string type = "", string code = null);
        //Task<Response<object>> GetOTPModel(string mobileNubmer, string type, OTPBack oTPBack, string code = null);
        Task<Response<OneTimePasswordDTO>> GenerateOneTimePassword(string mobile);
        Task<Response> ValidateOTP(string code);
        Task<OneTimePasswordDTO> AddOrUpdate(OneTimePasswordDTO dto);
        Task<OneTimePasswordDTO> GetByMobileNumber(string mobileNumber);
        Response SendOTP(string mobile, string message, string lang);
    }
}
