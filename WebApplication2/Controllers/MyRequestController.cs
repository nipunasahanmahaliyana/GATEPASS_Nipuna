using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GatePass_Project.DataAccess.MyRequest;

namespace GatePass_Project.Controllers
{
    public class MyRequestController : Controller
    {
        private readonly ILogger<MyRequestController> _logger;
        private readonly MyRequestRepository _myRequestRepository;


        public MyRequestController(ILogger<MyRequestController> logger, MyRequestRepository myRequestRepository)
        {
            _logger = logger;
            _myRequestRepository = myRequestRepository;

        }

        public IActionResult MyRequest()
        {
           
            
            var serviceNo = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = serviceNo;
            List<MyRequestModel> requests = _myRequestRepository.GetRequests(serviceNo);
            return View(requests);
        }

        public IActionResult RequestStatus(int id)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            MyRequestModel requestStatus = _myRequestRepository.GetRequestStatusById(id);

            if (requestStatus == null)
            {
                return RedirectToAction("Error");
            }

            int requestRefNo = _myRequestRepository.GetRequestRefNoById(id);
            requestStatus.Request_ref_no = requestRefNo;

            return View(requestStatus);
        }

        public IActionResult MyRequestItemDetail(int id)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            List<MyRequestModel> items = _myRequestRepository.GetDetailsById(id);

            if (items.Count == 0)
            {
                return RedirectToAction("Error");
            }

            return View(items);
        }
        
        //public IActionResult MyRequestDetails(int id)
        //{
            //MyRequestModel item = _myRequestRepository.GetRequestDetailsByRefNo(id);

            //if (item == null)
           // {
               // return RedirectToAction("Error");
            //}

            //return View(item);
        //}
    }
}
