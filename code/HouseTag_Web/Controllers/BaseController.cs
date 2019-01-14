using Microsoft.AspNetCore.Mvc;


namespace HouseTag_Web.Controllers
{
    public class BaseController : Controller
    {
        protected string GetIP()
        {
            return Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }

    }
}
