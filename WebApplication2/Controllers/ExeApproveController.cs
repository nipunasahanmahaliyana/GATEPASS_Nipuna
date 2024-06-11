using GatePass.DataAccess.ExeApprove;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using My_receipt_gate_pass.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace GatePass_Project.Controllers
{
    public class ExeApproveController : Controller
    {
        private readonly ILogger<ExeApproveController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly ExeApproveRepository _repository;

        public ExeApproveController(ILogger<ExeApproveController> logger, IConfiguration configuration, ExeApproveRepository repository)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _repository = repository;
        }

        public IActionResult ExeApprove()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                var serviceNo = HttpContext.Session.GetString("UserName");

                var requests = _repository.ExeApprove(serviceNo);
                return View(requests);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult Approve(int requestRefNo)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                _repository.Approve(requestRefNo);
                return RedirectToAction("ExeApprove");
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
                _repository.Reject(requestRefNo, rejectComment);
                return RedirectToAction("ExeApprove");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }


        public IActionResult PendingDetails(int id)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            // Retrieve item details for the given Request_ref_no
            List<ExeApproveModel> items = _repository.GetDetailsById(id);

            if (items.Count == 0)
            {
                // Handle the case where no details are found for the given id, e.g., show an error message or redirect to an error page.
                return RedirectToAction("Error");
            }

            return View(items);
        }

        public IActionResult Pending()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            List<ExeApproveModel> pendingRequests = _repository.GetRequestsByStageId(1);
            return View("ExeApprove", pendingRequests);
        }

        public IActionResult Approved()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            List<ExeApproveModel> approvedRequests = _repository.GetRequestsByStageId(2);
            return View("ExeApprove", approvedRequests);
        }

        public IActionResult Rejected()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            List<ExeApproveModel> rejectedRequests = _repository.GetRequestsByStageId(3);
            return View("ExeApprove", rejectedRequests);
        }
        public IActionResult Expired()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            List<ExeApproveModel> expiredRequests = _repository.GetRequestsByStageId(12);
            return View("ExeApprove", expiredRequests);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Displaying Notifications when rejecting a request
        public IActionResult Notification()
        {
            int commentCount = _repository.GetCommentCount();
            ViewData["CommentCount"] = commentCount;

            List<Workprogress> workProgressData = _repository.GetWorkProgressDataWithComments();
            ViewData["WorkProgressData"] = workProgressData;

            return View("Notification");
        }

        // Retrieve the count of comments
        private int GetCommentCount()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                return _repository.GetCommentCount();
            }
            catch (Exception)
            {
                // Handle the error as needed, e.g., return an error view or redirect to an error page.
                return 0;
            }
        }

        //updating the workprogress table viewed column as 1
        private List<Workprogress> GetWorkProgressDataWithComments()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                return _repository.GetWorkProgressDataWithComments();
            }
            catch (Exception)
            {
                // Handle the error as needed, e.g., return an error view or redirect to an error page.
                return new List<Workprogress>();
            }
        }

        [HttpPost]
        public IActionResult MarkNotificationAsViewed(int requestRefNo)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                _repository.MarkNotificationAsViewed(requestRefNo);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occurred while updating the Viewed column." });
            }
        }

        private List<ExeApproveModel> GetRequestsByStageId(int stageId)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                return _repository.GetRequestsByStageId(stageId);
            }
            catch (Exception)
            {
                // Handle the error as needed, e.g., return an error view or redirect to an error page.
                return new List<ExeApproveModel>();
            }
        }






        // Retrieve item details for a given Request_ref_no


        //        // Filtering according to the stageId
        //        private List<ExeApproveModel> GetRequestsByStageId(int stageId)
        //        {
        //            List<ExeApproveModel> requests = new List<ExeApproveModel>();

        //            using (SqlConnection connection = new SqlConnection(_connectionString))
        //            {
        //                connection.Open();
        //                string query = "SELECT * FROM Requests WHERE Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = @stageId)";

        //                try
        //                {
        //                    using (SqlCommand command = new SqlCommand(query, connection))
        //                    {
        //                        command.Parameters.AddWithValue("@stageId", stageId);

        //                        using (SqlDataReader reader = command.ExecuteReader())
        //                        {
        //                            while (reader.Read())
        //                            {
        //                                ExeApproveModel request = new ExeApproveModel
        //                                {
        //                                    Request_ref_no = reader.GetInt32(0),
        //                                    Sender_service_no = reader.GetString(1),
        //                                    In_location_name = reader.GetString(2),
        //                                    Out_location_name = reader.GetString(3),
        //                                    Receiver_service_no = reader.GetString(4),
        //                                    Created_date = reader.GetDateTime(5),
        //                                    ExO_service_no = reader.GetString(6),
        //                                    Carrier_nic_no = reader.GetString(7),
        //                                    Name = reader.GetString(8)
        //                                };

        //                                requests.Add(request);
        //                            }


        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogError(ex, $"An error occurred while retrieving requests with stage_id {stageId}.");
        //                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
        //                }
        //            }

        //            return requests;
        //        }
        //    }
        //}

        // Filtering according to the stageId

    }
}