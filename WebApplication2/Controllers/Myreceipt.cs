using My_receipt_gate_pass.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GatePass.DataAccess.MyReceipt;
using GatePass.DataAccess.Dispatch;

namespace My_receipt_gate_pass.Controllers
{
    public class Myreceipt : Controller
    {
        private readonly ILogger<Myreceipt> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly MyReceiptRepository _receiptRepository;

        public Myreceipt(ILogger<Myreceipt> logger, IConfiguration configuration, MyReceiptRepository receiptRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _receiptRepository = receiptRepository;
        }

        public IActionResult Index()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                var requests = _receiptRepository.Index();
                return View(requests);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        public IActionResult myreceiptdetails(int id)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                var requests = _receiptRepository.myreceiptdetails(id);
                return View(requests);
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public IActionResult Receive(int requestRefNo)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                _receiptRepository.Receive(requestRefNo);
                return RedirectToAction("Index");
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
                _receiptRepository.Reject(requestRefNo, rejectComment);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error");
            }
        }
    }
}