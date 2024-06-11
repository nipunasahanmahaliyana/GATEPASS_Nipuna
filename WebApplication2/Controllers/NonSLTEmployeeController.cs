using GatePass.DataAccess.ItemCategory;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.SqlClient;

namespace GatePass_Project.Controllers
{
    public class NonSLTEmployeeController : Controller
    {
        private readonly SqlConnection _connection;
        private readonly NonSLTRepository _nonSLTRepository;


        public NonSLTEmployeeController(SqlConnection connection,NonSLTRepository nonSLTRepository)
        {
            _connection = connection;
            _nonSLTRepository = nonSLTRepository;
        }

        public IActionResult Index()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            return View();
        }

        public IActionResult CreateNewNonSLTEmployee()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var roles = _nonSLTRepository.GetRoles();
            ViewBag.Roles = new SelectList(roles, "Role_id", "Role_duty");
            return View();
        }

        public IActionResult NonSLTEmployee()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var nonemployees = _nonSLTRepository.GetNonSLTEmployees();
            ViewBag.Nonemployees = nonemployees;
            return View();
        }

        [HttpPost]
        public IActionResult NewNonSLTEmployee(NonSLTEmployeeModel model)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var result = _nonSLTRepository.NewNonSLTEmployee(model);
            TempData["NonSLTMessage"] = result;
            return RedirectToAction("NonSLTEmployee");
        }

        [HttpPost]
        public IActionResult DeleteNonSLTEmployee(int Non_slt_id)
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var result = _nonSLTRepository.DeleteNonSLTEmployee(Non_slt_id);
            TempData["NonSLTMessage"] = result;
            return RedirectToAction("NonSLTEmployee");
        }
    }
}