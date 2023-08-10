using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using CMC.Kernel.Core.Wrappers;

namespace CMC.Kernel.Core.Controllers
{
    public abstract class BaseController : Controller
    {
        protected static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        protected ActionResult ProcessResponse<T>(T data)
        {
            var response = new Response<T>(data);

            if (data == null)
            {
                response.Succeeded = false;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Error"; //** Todo: Bring default localized error message from resources store.
                return StatusCode((int)response.StatusCode, response);
            }

            response.Message = "Success";//** Todo: Bring default localized error message from resources store.
            return StatusCode((int)response.StatusCode, response);
        }
        protected ActionResult Ok(Response response)
        {
            if (string.IsNullOrEmpty(response.Message))
            {
                if (response.Succeeded)
                    response.Message = "Success"; //** Todo: Bring default localized error message from resources store.
                else
                    response.Message = "Error";//** Todo: Bring default localized error message from resources store.
            }
            return StatusCode((int)response.StatusCode, response);
        }

        protected string ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
        {
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());
                vResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}