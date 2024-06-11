using GatePass.DataAccess.Dispatch;
using GatePass.DataAccess.DoVerify;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using My_receipt_gate_pass.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class DispatchController : Controller
    {
        private readonly ILogger<DispatchController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly DispatchRepository _dispatchRepository;

        public DispatchController(ILogger<DispatchController> logger, IConfiguration configuration, DispatchRepository dispatchRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _dispatchRepository = dispatchRepository;

        
        }

        public IActionResult DispatchVerifyDetails(int id)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            // Retrieve the request details for the given Request_ref_no (id)
            List<DispatchModel> request = _dispatchRepository.GetDetailsById(id);



            if (request == null)
            {
                return NotFound(); // Handle the case where the request is not found.
            }

            return View(request);
        }

        public IActionResult Dispatch()
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                var requests = _dispatchRepository.GetDispatchRequests();
                return View(requests);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public IActionResult Dispatch(int requestRefNo)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                _dispatchRepository.Dispatch(requestRefNo);
                return RedirectToAction("Dispatch");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }


        [HttpPost]
        public IActionResult Reject(int requestRefNo, string rejectComment)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                _dispatchRepository.Reject(requestRefNo, rejectComment);
                return RedirectToAction("Dispatch");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult ViewDispatchDetails(int id)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            // Retrieve the request details for the given Request_ref_no (id)
            List<DispatchModel> request = _dispatchRepository.GetDetailsById(id);

            if (request == null)
            {
                return NotFound(); // Handle the case where the request is not found.
            }

            return View(request);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }

}