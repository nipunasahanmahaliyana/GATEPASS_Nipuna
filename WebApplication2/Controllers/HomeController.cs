using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Data.SqlClient;
using GatePass.Models;
using Microsoft.AspNetCore.Mvc.Filters;



namespace GatePass.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlConnection _connection;

        public HomeController(ILogger<HomeController> logger, SqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public IActionResult Index()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> SubmitForm(UserInfo userInfo)
        //{
        //    string query = "INSERT INTO UserInfo (Sub, Name, ServiceNo, Designation, Phone, Section, [Group], Email, Grade, Division) VALUES (@sub, @name, @number, @designation, @phone, @section, @group, @email, @grade, @division)";

        //    using (SqlCommand command = new SqlCommand(query, _connection))
        //    {
        //        command.Parameters.AddWithValue("@sub", userInfo.Sub);
        //        command.Parameters.AddWithValue("@name", userInfo.Name);
        //        command.Parameters.AddWithValue("@number", userInfo.ServiceNo);
        //        command.Parameters.AddWithValue("@designation", userInfo.Designation);
        //        command.Parameters.AddWithValue("@phone", userInfo.Phone);
        //        command.Parameters.AddWithValue("@section", userInfo.Section);
        //        command.Parameters.AddWithValue("@group", userInfo.Group);
        //        command.Parameters.AddWithValue("@email", userInfo.Email);
        //        command.Parameters.AddWithValue("@grade", userInfo.Grade);
        //        command.Parameters.AddWithValue("@division", userInfo.Division);

        //        _connection.Open();
        //        await command.ExecuteNonQueryAsync();
        //        _connection.Close();
        //    }

        //    return View("~/Views/NewRequest/NewRequest.cshtml");
        //}


    }
}