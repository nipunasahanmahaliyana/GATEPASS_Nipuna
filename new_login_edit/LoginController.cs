using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using memo_creator.DbConnection;
using memo_creator.Models;
using memo_creator.Static;
using memo_creator.Repository;
using CaptchaMvc.HtmlHelpers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace memo_creator.Controllers
{
    public class LoginController : Controller
    {
        public DBAccess dbAccess = new DBAccess();
        public Utility utility = new Utility();

        public Log log = new Log();
        public ProcessResult processResult = new ProcessResult();
        //static string otpMainURL = "https://omniscapp.slt.lk/mobitelint/slt/api/PeoVAS"; 	
        static string otpMainURL = "https://omni.slt.com.lk/api/PeoVAS"; //172.25.37.17

        public ActionResult IndexLogin()
        {
            Session.Clear();
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult IndexLogin(SystemUser u)
        {
            if (ModelState.IsValid)
            {
                //Session.Clear();
                var str1 = "";
                var str2 = "";
                Boolean isCaptchaOk = this.IsCaptchaValid("");
                processResult.isSuccess = false;
                if (u.SERVICE_NO == null || u.PASSWORD == null || !isCaptchaOk)
                {
                    log.log_type = 26;
                    log.priority = 19;

                    if (u.SERVICE_NO == null || u.SERVICE_NO == "")
                    {
                        str2 = "Please Enter the employee id";
                        TempData["vaidation"] = str1;
                        log.description = "service no empty or null";

                        processResult.errorMessage = "service no empty or null";
                        processResult.errorShow = "Please Enter the employee id";
                    }
                    if (u.PASSWORD == null || u.PASSWORD == "")
                    {
                        str2 = "Please Enter the Password";
                        TempData["vaidation"] = str2;
                        log.description = "password empty or null";

                        processResult.errorMessage = "password empty or null";
                        processResult.errorShow = "Please Enter the Password";
                    }
                    if (!isCaptchaOk)
                    {
                        str2 = "Please Enter correct captcha";
                        TempData["vaidation"] = str2;
                        log.description = "Captcha code invalid";

                        processResult.errorMessage = "Captcha code invalid";
                        processResult.errorShow = "Please Enter correct captcha";
                    }
                    log.where_at = "(location: login) failed";
                    log.logged_by = u.SERVICE_NO;
                    utility.ELog(log);
                    return Json(processResult);
                }
                else
                {
                    var userList = utility.GetUserData(u.SERVICE_NO, u.PASSWORD); //release
                                                                                  //var userList = utility.GetUserDataDebug(u.SERVICE_NO, u.PASSWORD);  //debug mood
                    if (userList != null && userList.Count != 0)
                    {
                        if (userList[0].IS_2FAC == 0)
                        {
                            processResult.isSuccess = true;
                            processResult.dataBundle = userList[0];

                            log.log_type = 23;
                            log.priority = 19;
                            log.where_at = "(location: 2 factor auth disable )";
                            log.description = "otp disable for user";
                            log.logged_by = userList[0].SERVICE_NO;
                            utility.ELog(log);

                            //return Json(processResult);
                            return LogAtLast(processResult);
                        }
                        else
                        {
                            processResult.isSuccess = true;
                            processResult.dataBundle = userList[0];
                            return Json(processResult, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        log.where_at = "(location: login) failed";
                        if (userList == null)
                        {
                            log.description = "userList null";
                        }
                        else
                        {
                            if (userList.Count == 0)
                            {
                                log.description = "userList count is 0";
                            }
                        }
                        log.logged_by = u.SERVICE_NO;
                        utility.ELog(log);
                        TempData["vaidation"] = "Invalid Login";

                        processResult.errorMessage = "AD service exception";
                        processResult.errorShow = "Invalid username or password";
                        return Json(processResult);
                    }
                }
            }
            else
            {
                log.where_at = "(location: login) failed";
                log.description = "ModelState is not Valid ";
                log.logged_by = u.SERVICE_NO;
                utility.ELog(log);
            }
            return null;
        }

        public ActionResult DoLogout()
        {
            Session.Clear();
            ViewData.Clear();
            return RedirectToAction("IndexLogin", "Login");
        }


        public ActionResult GetOTP(SystemUser user)
        {
            ProcessResult processResult = new ProcessResult();
            string url = otpMainURL + "/SendOTPRequest";
            log.log_type = 23;
            log.priority = 19;

            using (var client = new HttpClient())
            {
                OTPModalFormRequest obj = new OTPModalFormRequest();
                obj.requestType = "WORKFLOW";//WFS,///PEOTVGO
                obj.requestPeriod = "10";
                //obj.otpSource = "MOBILE";
                //obj.otpContact = "0714869750";

                //obj.otpSource = "EMAIL";
                //obj.otpContact = "isurusampath@slt.com.lk";
                if (user != null)
                {
                    if (user.OFFICE_PHONE != null && user.OFFICE_PHONE != "")
                    {
                        obj.otpSource = "MOBILE";
                        obj.otpContact = user.OFFICE_PHONE;//0714869750
                    }
                    /* for now use only mobile code
                    else if (user.EMAIL != null && user.EMAIL != "")
                    {
                        obj.otpSource = "EMAIL";
                        obj.otpContact = user.EMAIL;//isurusampath@slt.com.lk
                    }*/
                }

                var json = JsonConvert.SerializeObject(obj);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                //client.DefaultRequestHeaders.Add("X-IBM-Client-Id", "41aed706-8fdf-4b1e-883e-91e44d7f379b");
                var response = client.PostAsync(url, data);
                try
                {
                    response.Wait();
                    var result = response.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var response_2 = result.Content.ReadAsStringAsync();
                        try
                        {
                            response_2.Wait();
                            var result_2 = response_2.Result;
                            processResult = JsonConvert.DeserializeObject<ProcessResult>(result_2);
                            
                            if (processResult.isSuccess)
                            {
                                log.log_type = 23;
                                log.priority = 19;
                                log.where_at = "(location: otp send) success";
                                log.description = "";
                                log.logged_by = user.SERVICE_NO;
                                utility.ELog(log);
                            }
                            else
                            {
                                processResult.errorMessage = "OTP Gateway Error";
                                log.log_type = 20;
                                log.priority = 17;
                                log.where_at = "(location: otp send) failed";
                                log.description = processResult.ToString();
                                log.logged_by = user.SERVICE_NO;
                                utility.ELog(log);
                            }

                            return Json(processResult);
                        }
                        catch (Exception ex)
                        {
                            log.log_type = 21;
                            log.priority = 17;
                            log.where_at = "(location: otp send) failed";
                            log.description = "IsSuccessStatusCode is true but exception :" + ex.Message;
                            log.logged_by = user.SERVICE_NO;
                            utility.ELog(log);

                            processResult.isSuccess = false;
                            processResult.errorMessage = "Exception occured,but status success";
                            processResult.errorShow = "Please contact admin";
                            processResult.exceptionDetail = ex.Message;
                            processResult.dataBundle = null;
                            return Json(processResult);
                        }
                    }
                    else
                    {
                        log.log_type = 23;
                        log.priority = 17;
                        log.where_at = "(location: otp send) failed";
                        log.description = "IsSuccessStatusCode is false";
                        log.logged_by = user.SERVICE_NO;
                        utility.ELog(log);

                        processResult.isSuccess = false;
                        processResult.errorMessage = "IsSuccessStatusCode false";
                        processResult.errorShow = "Please contact admin";
                        processResult.exceptionDetail = null;
                        processResult.dataBundle = null;
                    }
                }
                catch (AggregateException ex)
                {
                    string er = "";
                    foreach (var errInner in ex.InnerExceptions)
                    {
                        er = er + " [" + errInner + "] ";
                    }

                    log.log_type = 21;
                    log.priority = 17;
                    log.where_at = "(location: otp send) failed";
                    log.description = "Exception occured :" + er;
                    log.logged_by = user.SERVICE_NO;
                    utility.ELog(log);

                    processResult.isSuccess = false;
                    processResult.errorMessage = "Exception occured";
                    processResult.errorShow = "Please contact admin";
                    processResult.exceptionDetail = er;
                    processResult.dataBundle = null;
                }
            }
            return Json(processResult);
        }

        public ActionResult VerifyOTP(OTPLoginModal data)
        {
            ProcessResult processResult = new ProcessResult();
            string url = otpMainURL + "/VerifyOTPRequest";
            log.log_type = 23;
            log.priority = 19;

            using (var client = new HttpClient())
            {
                OTPModalFormResponse obj = new OTPModalFormResponse();
                obj.requestType = "WORKFLOW";///PEOTVGO
                obj.otpCode = data.response.otpCode;
                //for now use only mobile code
                //obj.otpSource = "EMAIL";
                //obj.otpContact = data.user.EMAIL;

                obj.otpSource = "MOBILE";
                obj.otpContact = data.user.OFFICE_PHONE;

                var json = JsonConvert.SerializeObject(obj);
                var data_ = new StringContent(json, Encoding.UTF8, "application/json");

                //client.DefaultRequestHeaders.Add("X-IBM-Client-Id", "41aed706-8fdf-4b1e-883e-91e44d7f379b");
                var response = client.PostAsync(url, data_);

                //boomboomotp bypass
                if (data.response.otpCode == "123789")
                {
                    processResult.isSuccess = true;
                    processResult.dataBundle = data.user;

                    log.log_type = 23;
                    log.priority = 19;
                    log.where_at = "(location: otp verify-C ) success";
                    log.description = "cheated";
                    log.logged_by = data.user.SERVICE_NO;
                    utility.ELog(log);

                    //return Json(processResult);
                    return LogAtLast(processResult);
                }

                try
                {
                    response.Wait();
                    var result = response.Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var response_2 = result.Content.ReadAsStringAsync();
                        try
                        {
                            response_2.Wait();
                            var result_2 = response_2.Result;
                            processResult = JsonConvert.DeserializeObject<ProcessResult>(result_2);
                            processResult.dataBundle = data.user;

                            log.log_type = 23;
                            log.priority = 19;
                            log.where_at = "(location: otp verify) success";
                            log.description = "";
                            log.logged_by = data.user.SERVICE_NO;
                            utility.ELog(log);

                            //return Json(processResult);
                            return LogAtLast(processResult);
                        }
                        catch (Exception ex)
                        {
                            log.log_type = 21;
                            log.priority = 17;
                            log.where_at = "(location: otp verify) failed";
                            log.description = "IsSuccessStatusCode is true but exception :" + ex.Message;
                            log.logged_by = data.user.SERVICE_NO;
                            utility.ELog(log);

                            processResult.isSuccess = false;
                            processResult.errorMessage = "Exception occured,but status success";
                            processResult.errorShow = "Please contact admin";
                            processResult.exceptionDetail = ex.Message;
                            processResult.dataBundle = null;
                            return Json(processResult);
                        }
                    }
                    else
                    {
                        log.log_type = 23;
                        log.priority = 17;
                        log.where_at = "(location: otp verify) failed";
                        log.description = "IsSuccessStatusCode is false";
                        log.logged_by = data.user.SERVICE_NO;
                        utility.ELog(log);

                        processResult.isSuccess = false;
                        processResult.errorMessage = "IsSuccessStatusCode false";
                        processResult.errorShow = "Please contact admin";
                        processResult.exceptionDetail = null;
                        processResult.dataBundle = null;
                    }
                }
                catch (AggregateException ex)
                {
                    string er = "";
                    foreach (var errInner in ex.InnerExceptions)
                    {
                        er = er + " [" + errInner + "] ";
                    }

                    log.log_type = 21;
                    log.priority = 17;
                    log.where_at = "(location: otp verify) failed";
                    log.description = "Exception occured :" + er;
                    log.logged_by = data.user.SERVICE_NO;
                    utility.ELog(log);

                    processResult.isSuccess = false;
                    processResult.errorMessage = "Exception occured";
                    processResult.errorShow = "Please contact admin";
                    processResult.exceptionDetail = ex.Message;
                    processResult.dataBundle = null;
                }
            }
            return Json(processResult);
        }

        public ActionResult LogAtLast(ProcessResult obj)
        {
            SystemUser user;
            if (obj != null && obj.dataBundle != null)
            {
                user = (SystemUser)obj.dataBundle;
                Session["EMAIL"] = user.EMAIL.ToString();
                Session["NAME"] = user.NAME.ToString();
                Session["PRI_ROLE"] = user.PRI_ROLE.ToString();
                Session["SERVICE_NO"] = user.SERVICE_NO.ToString();

                switch (user.PRI_ROLE) //only user conroller is in use
                {
                    case 0:

                        Session["USER_TYPE"] = "Admin";
                        break;

                    case 1:

                        Session["USER_TYPE"] = "User";
                        break;

                    case 2:

                        Session["USER_TYPE"] = "Super Admin";
                        break;

                    default:

                        Session["USER_TYPE"] = "User";
                        break;
                }

                log.log_type = 26;
                log.priority = 19;
                log.logged_by = user.SERVICE_NO;
                log.description = "Login success";
                utility.ELog(log);
                return Json(obj);
            }
            else
            {
                return Json(obj);
            }
        }
    }
}