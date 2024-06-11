using GatePass.Controllers;
using GatePass.DataAccess.ItemCategory;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class NewRequestController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SLTLoginController> _logger;
        private readonly NewRequestRepository _newRequestRepository;

        public NewRequestController(IConfiguration configuration, ILogger<SLTLoginController> logger, NewRequestRepository newRequestRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _newRequestRepository = newRequestRepository;
        }

        [HttpGet]
        [Route("~/GatePass/NewRequest/NewRequest")]
        public IActionResult NewRequest()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var itemCategories = _newRequestRepository.GetItemCategories();
            var locations = _newRequestRepository.GetLocations();

            ViewBag.ItemCategories = itemCategories;
            ViewBag.Locations = locations;

            return View();
        }

        [HttpGet]
        public object GetUserInfoDetails()
        {
            // Retrieve userInfo from the session
            var serviceNo = HttpContext.Session.GetString("UserName");

            return _newRequestRepository.GetUserInfoDetails(serviceNo);

        }


        public object GetExecutiveOfficers(string group)
        {
            return _newRequestRepository.GetExecutiveOfficers(group);

        }


        [HttpGet]
        public IActionResult GetReceiverDetails(string serviceNo)
        {
            try
            {
                var data = _newRequestRepository.GetReceiverDetails(serviceNo);
                if (data != null)
                {
                    return Json(data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while processing the request");
            }
        }


        //Save request into database

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost("CheckLogin")]
        public async Task<IActionResult> CheckLogin(bool VerifiedItems)
        {
            int isdone = 0;

            var result = await _newRequestRepository.ProcessRequest(VerifiedItems, Request.Form, Request.Form.Files, _configuration);

            // Handle the result here...
            if (result > 0)
            {
                return Ok(new { isSuccess = isdone > 0, message = _newRequestRepository.GetResultMessage(isdone) });
            }
            else
            {
                return BadRequest();
            }
        }

    }

}