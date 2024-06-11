using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewFeatures;  // Add this namespace for ITempDataDictionary
using Microsoft.AspNetCore.Mvc;
using GatePass_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MessagePack;

namespace GatePass.DataAccess.Login
{
    public class LoginRepository
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginRepository> _logger;

        public LoginRepository(SqlConnection connection, IConfiguration configuration, ILogger<LoginRepository> logger)
        {
            _connection = connection;
            _configuration = configuration;
            _logger = logger;
        }


        public async Task<IActionResult> NewLogin(MyLogin model, HttpContext httpContext, ITempDataDictionary tempData)
        {
            try
            {
                if (model == null)
                {
                    tempData["Error"] = "Invalid model.";
                    return new BadRequestResult();
                }

                if (string.IsNullOrWhiteSpace(model.NIC) || string.IsNullOrWhiteSpace(model.Non_slt_Id))
                {
                    tempData["Error"] = "NIC and Non SLT Id are required.";
                    return new BadRequestResult();
                }

                var query = "SELECT * FROM Non_SLT_Users WHERE NIC = @NIC AND Non_slt_Id = @Non_slt_Id";

                using (var command = new SqlCommand(query, _connection))
                {
                    command.Parameters.AddWithValue("@NIC", model.NIC);
                    command.Parameters.AddWithValue("@Non_slt_Id", model.Non_slt_Id);

                    await _connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            var claims = new List<Claim>();
                            var role_id = reader.GetInt32(reader.GetOrdinal("Role_id"));


                            var NIC = reader["NIC"].ToString();

                            if (role_id == 1)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Security"));
                                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "Security")));
                                httpContext.Session.SetString("UserName", NIC );
                                tempData["Message"] = "Security " + model.NIC + " logged in successfully!";
                                return new RedirectToActionResult("Index", "Home", null);
                            }
                            else if (role_id == 2)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "NonSLT"));
                                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "NonSLT")));

                                httpContext.Session.SetString("UserName", model.NIC);

                                tempData["Message"] = "NonSLT " + model.NIC + " logged in successfully!";
                                return new RedirectToActionResult("Index", "Home", NIC);
                            }
                            else if (role_id == 3)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "Admin")));
                                httpContext.Session.SetString("UserName", model.NIC);
                                tempData["Message"] = "Admin" + model.NIC + " logged in successfully!";
                                return new RedirectToActionResult("Index", "Home", NIC);
                            }

                            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "custom")));

                            tempData["Message"] = $"{reader.GetString(reader.GetOrdinal("RoleName"))} {model.NIC} logged in successfully!";
                            return new RedirectToActionResult("Index", "Home", NIC);
                        }
                    }
                }

                // Handle case where user credentials are not valid
                tempData["Error"] = "Invalid login attempt";
                tempData["ErrorDetails"] = "Invalid NIC or Non SLT Id!";
                return new ViewResult { ViewName = "Login", ViewData = new ViewDataDictionary<MyLogin>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model } };
            }
            catch (Exception ex)
            {
                // Log the exception details
                _logger.LogError(ex, "An error occurred in the NewLogin method");

                tempData["Error"] = "An error occurred while processing your request. Please try again later.";
                return new ViewResult { ViewName = "Login", ViewData = new ViewDataDictionary<MyLogin>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model } };
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
    }
}
