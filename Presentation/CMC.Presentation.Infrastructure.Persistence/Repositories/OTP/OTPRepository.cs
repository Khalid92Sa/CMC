using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Infrastructure.Persistence.Repositories;
using CMC.Presentation.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Infrastructure.Persistence.Repositories.OTP
{
    public class OTPRepository : Repository<OneTimePassword>, IOTPRepository
    {

        public OTPRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Insert one time password
        /// </summary>
        public async Task<OneTimePassword> InsertOTP(OneTimePassword otp)
        {
            try
            {
                await InsertAsync(otp);
                await UnitOfWork.SaveChangesAsync();
                return otp;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<OneTimePassword> UpdateOtp(OneTimePassword otp)
        {
            try
            {
                var oneTimePassword = await FindAsync(otp.Id);
                if (oneTimePassword != null)
                {
                    oneTimePassword.UnlockedDate = otp.UnlockedDate;
                    oneTimePassword.NoOfGenerations = otp.NoOfGenerations;
                    oneTimePassword.ModifiedDate = otp.ModifiedDate;
                    oneTimePassword.NoOfTrials = otp.NoOfTrials;
                    oneTimePassword.SecurityCode = otp.SecurityCode;
                    Update(oneTimePassword);
                    await UnitOfWork.SaveChangesAsync();
                }

                return otp;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<OneTimePassword> GetByMobileNumber(string mobileNumber)
        {
            try
            {
                return await GetAll().Where(x => x.MobileNumber == mobileNumber).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
