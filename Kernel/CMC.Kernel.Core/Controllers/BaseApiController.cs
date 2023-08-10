using Microsoft.AspNetCore.Mvc;

namespace CMC.Kernel.Core.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class BaseApiController : BaseController
    {
    }
}
