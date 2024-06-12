using GatePass.DataAccess.DoVerify;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class VerifyController : Controller
    {
        private readonly ILogger<VerifyController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly DoVerifyRepository _doVerifyRepository;

        public VerifyController(ILogger<VerifyController> logger, IConfiguration configuration, DoVerifyRepository doVerifyRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _doVerifyRepository = doVerifyRepository;
        }

        public IActionResult Verify()
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                var requests = _doVerifyRepository.Verify(sessionUserName);
                return View(requests);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Approve(int requestRefNo)
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                _doVerifyRepository.Approve(requestRefNo);
                return RedirectToAction("Verify");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }

        }

        [HttpPost]
        public IActionResult Reject(int requestRefNo, string rejectComment)
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                _doVerifyRepository.Reject(requestRefNo, rejectComment);
                return RedirectToAction("Verify");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }

        }

        public IActionResult ViewPendingDetails(int id)
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            // Retrieve item details for the given Request_ref_no
            List<VerifyModel> items = _doVerifyRepository.GetDetailsById(id);

            if (items.Count == 0)
            {
                // Handle the case where no details are found for the given id, e.g., show an error message or redirect to an error page.
                return RedirectToAction("Error");
            }

            return View(items);


        }

        public IActionResult Pending(int id)
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            List<VerifyModel> pendingRequests = _doVerifyRepository.GetRequestsByStageId(2,sessionUserName);
            return View("Verify", pendingRequests);
        }

        public IActionResult Verified()
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            List<VerifyModel> verifiedRequests = _doVerifyRepository.GetRequestsByStageId(5,sessionUserName);
            return View("Verify", verifiedRequests);
        }

        public IActionResult Rejected()
        {
            var sessionUserName = "";
            sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            List<VerifyModel> rejectedRequests = _doVerifyRepository.GetRequestsByStageId(6,sessionUserName);
            return View("Verify", rejectedRequests);
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
