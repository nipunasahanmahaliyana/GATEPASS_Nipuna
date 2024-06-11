using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using GatePass_Project.Models;
using GatePass.DataAccess.Login;
using System.Threading.Tasks;

namespace GatePass.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginRepository _loginRepository;
        private readonly ILogger<LoginController> _logger;

        public LoginController(LoginRepository loginRepository, ILogger<LoginController> logger)
        {
            _loginRepository = loginRepository;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewLogin(MyLogin model)
        {
            var result = await _loginRepository.NewLogin(model, HttpContext, TempData);

            // Handle the result and return the appropriate view
            if (result is RedirectToActionResult)
            {
                // Successful login
                return result;
            }
            else if (result is BadRequestResult)
            {
                // Bad request, invalid model or missing credentials
                return View("Login", model);
            }
            else
            {
                // Handle other error cases, log the error, and return an appropriate view
                _logger.LogError("An unexpected error occurred during login.");
                return View("Login");
            }
        }
    }
}
