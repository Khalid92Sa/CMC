using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Kernel.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMC.Presentation.Marketplace.Infrastructure.Persistence.Services.Repositories.Identity
{
    public class LoginRepository : Repository<Login>, ILoginRepository
    {
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        public LoginRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
    }
}
