using CMC.Kernel.Core.Persistence;
using CMC.Presentation.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Infrastructure.Persistence.Repositories.OTP
{
    public interface IOTPRepository : IRepository<OneTimePassword>
    {
        Task<OneTimePassword> InsertOTP(OneTimePassword otp);
        Task<OneTimePassword> UpdateOtp(OneTimePassword otp);
        Task<OneTimePassword> GetByMobileNumber(string mobileNumber);
    }
}
