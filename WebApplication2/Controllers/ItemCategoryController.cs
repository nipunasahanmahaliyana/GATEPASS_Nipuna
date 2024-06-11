using GatePass.DataAccess.ItemCategory;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace GatePass.Controllers
{
    public class ItemCategoryController : Controller
    {
        private readonly SqlConnection _connection;
        private readonly ItemCategoryRepository _categoryrepository;



        public ItemCategoryController(SqlConnection connection, ItemCategoryRepository categoryRepository)
        {
            _connection = connection;
            _categoryrepository = categoryRepository;
        }

        public IActionResult ItemCategory()
        {
            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;
            try
            {
                var (itemcategorieswithoutremoved, itemcategorywithremoved) = _categoryrepository.GetItemCategories();
                ViewBag.ItemCategoriesWithRemoved = itemcategorywithremoved;
                ViewBag.ItemCategoriesWithoutRemoved = itemcategorieswithoutremoved;
                return View("ItemCategory");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }


        public IActionResult CreateNewCategory()
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            return View();
        }

        public IActionResult WithRemoved()
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            try
            {
                var itemcategorywithremoved = _categoryrepository.GetItemCategoriesWithRemoved();
                ViewBag.ItemCategoriesWithRemoved = itemcategorywithremoved;
                return View("ItemCategory");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public IActionResult UploadCsv(IFormFile csvFile)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var result = _categoryrepository.UploadCsv(csvFile);
            TempData["ItemMessage"] = result;
            return RedirectToAction("ItemCategory");
        }

        [HttpPost]
        public IActionResult AddItemCategory(ItemCategoryModel model)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var result = _categoryrepository.AddItemCategory(model);
            TempData["ItemMessage"] = result;
            return RedirectToAction("ItemCategory");
        }

        [HttpPost]
        public IActionResult DeleteItemCategory(int categoryId)
        {

            var sessionUserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = sessionUserName;

            var result = _categoryrepository.DeleteItemCategory(categoryId);
            TempData["ItemMessage"] = result;
            return RedirectToAction("ItemCategory");
        }


    }
}
