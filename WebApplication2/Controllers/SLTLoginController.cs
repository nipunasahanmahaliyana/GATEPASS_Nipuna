using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;  // Add this line
using NuGet.Common;
using Microsoft.Extensions.Logging;
using GatePass.Models;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Web;

namespace GatePass.Controllers
{
    public class SLTLoginController : Controller
    {
        private readonly SqlConnection _connection;
        private readonly ILogger<SLTLoginController> _logger;
        private readonly IConfiguration _configuration;

        
        public SLTLoginController(IConfiguration configuration, ILogger<SLTLoginController> logger, SqlConnection connection)
        {
            _configuration = configuration;
            _logger = logger;
            _connection = connection;
        }


        public RedirectResult SLTLogin()
        {
            //    using (var client = new HttpClient())
            //    {
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://oneidentitytest.slt.com.lk/authServer/auth?response_type=code&client_id=GatePass&redirect_uri=https://220.247.224.226:9300/SLTLogin/SLTLoginRedirect&scope=openid%20email&state=1234zyx");

            //// Add your cookies to the request headers
            //request.Headers.Add("Cookie", ".AspNetCore.Antiforgery.TmywGdJytIY=CfDJ8KHpbe-VjV9Hv0cg12Jx5BibFhPbHZifF6kaHXdkQLdqUBdYhF8HYWeVjHXEDjwyhlA2J0_xSesgod-5LBMxallXYwJyK1xAU4XGOOn99aR4exIl7OW8qg2SRzGLy-xasgVMHOzSTGLvD7gqHow0E_w; .AspNetCore.Session=CfDJ8KHpbe%2BVjV9Hv0cg12Jx5BibY6c%2FCDkFYNey0%2F%2BaFq2te6k%2FCXvq8ciEfbpSkVRAWiO0DGJfz5htelrG9tjYtPBFJrQfCcXfHZXCFYNKYLYazmQ5q7h4GxkStygTAsefZDaKmr7%2BEWlJCdiFyJbpqb8xSwxSHtJ59feFZyNmI%2BWP");

            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            //// Read and return the response content
            //var responseData = await response.Content.ReadAsStringAsync();

            // You can do further processing or return the data as needed
            return Redirect("https://oneidentitytest.slt.com.lk/authServer/auth?response_type=code&client_id=GatePass&redirect_uri=https://220.247.224.226:9301/SLTLogin/SLTLoginRedirect&scope=openid%20email&state=1234zyx");
            //}
        }

        [HttpGet]
        [HttpGet]
        public async Task<ActionResult> SLTLoginRedirect(string code, string state)
        {
            var resultToken = await GetToken(code, state);
            string token = "";

            // Checking if the result is a ViewResult
            if (resultToken is ViewResult viewResult)
            {
                // Extracting the token
                token = viewResult.Model as string;
            }

            // Getting the user info using the token
            var resultUserInfo = await GetUserInfo(token);
            object userInfo = null; // Initialize userInfo

            // Checking if the result is a ViewResult
            if (resultUserInfo is ViewResult viewResultUserInfo)
            {
                // Extracting user info from the model
                userInfo = viewResultUserInfo.Model;
            }

            // After retrieving the user info...
            var serviceNo = userInfo.GetType().GetProperty("ServiceNo")?.GetValue(userInfo)?.ToString();

            //Store the serviceNo in session
            HttpContext.Session.SetString("ServiceNo", serviceNo);

            await StoreUserInfoInDatabase(userInfo);

            var group = userInfo.GetType().GetProperty("Group")?.GetValue(userInfo)?.ToString();

            //Store the group in session
            HttpContext.Session.SetString("Group", group);

            // Call the method to store exec info in database
            await GetExecutiveList(token, group);

            ViewBag.UserInfo = userInfo;

            return View("~/Views/Home/Index.cshtml");
        }

        // Correct Method to check and insert executive list to the table
        [HttpGet]
        public async Task<ActionResult> GetExecutiveList(string token, string group)
        {
            string baseUrl = "https://oneidentitytest.slt.com.lk/authServer/execInfo";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var builder = new UriBuilder(baseUrl);
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["groupName"] = group;
                builder.Query = query.ToString();
                string urlWithParams = builder.ToString();

                var response = await client.GetAsync(urlWithParams);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var executiveList = JsonConvert.DeserializeObject<List<Executive>>(responseContent);

                    // Open a connection to the database
                    await _connection.OpenAsync();

                    foreach (var execInfo in executiveList)
                    {
                        // Check if the record already exists
                        var checkQuery = $"SELECT COUNT(*) FROM ExecutiveInfo WHERE ServiceNo = '{execInfo.ServiceNo}'";
                        using (var commandCheck = new SqlCommand(checkQuery, _connection))
                        {
                            var count = (int)await commandCheck.ExecuteScalarAsync();
                            if (count == 0)
                            {
                                var insertQuery = "INSERT INTO ExecutiveInfo (Sub, Name, ServiceNo, Designation, Phone, Section, Groups, Email, Grade, Division) VALUES (@Sub, @Name, @ServiceNo, @Designation, @Phone, @Section, @Groups, @Email, @Grade, @Division)";
                                using (var commandInsert = new SqlCommand(insertQuery, _connection))
                                {
                                    commandInsert.Parameters.AddWithValue("@Sub", execInfo.Sub);
                                    commandInsert.Parameters.AddWithValue("@Name", execInfo.Name);
                                    commandInsert.Parameters.AddWithValue("@ServiceNo", execInfo.ServiceNo);
                                    commandInsert.Parameters.AddWithValue("@Designation", execInfo.Designation);
                                    commandInsert.Parameters.AddWithValue("@Phone", execInfo.Phone);
                                    commandInsert.Parameters.AddWithValue("@Section", execInfo.Section);
                                    commandInsert.Parameters.AddWithValue("@Groups", execInfo.Group);
                                    commandInsert.Parameters.AddWithValue("@Email", execInfo.Email);
                                    commandInsert.Parameters.AddWithValue("@Grade", execInfo.Grade);
                                    commandInsert.Parameters.AddWithValue("@Division", execInfo.Division);

                                    await commandInsert.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }

                    return Ok("Exec info inserted successfully.");
                }
                else
                {
                    return BadRequest("Request to execInfo endpoint failed.");
                }
            }
        }

        public async Task<ActionResult> StoreUserInfoInDatabase(object userInfo)
        {
            try
            {
                // Extracting values from the userInfo object
                var sub = userInfo.GetType().GetProperty("Sub")?.GetValue(userInfo)?.ToString();
                var name = userInfo.GetType().GetProperty("Name")?.GetValue(userInfo)?.ToString();
                var serviceNo = userInfo.GetType().GetProperty("ServiceNo")?.GetValue(userInfo)?.ToString();
                var designation = userInfo.GetType().GetProperty("Designation")?.GetValue(userInfo)?.ToString();
                var phone = userInfo.GetType().GetProperty("Phone")?.GetValue(userInfo)?.ToString();
                var section = userInfo.GetType().GetProperty("Section")?.GetValue(userInfo)?.ToString();
                var group = userInfo.GetType().GetProperty("Group")?.GetValue(userInfo)?.ToString();
                var email = userInfo.GetType().GetProperty("Email")?.GetValue(userInfo)?.ToString();
                var grade = userInfo.GetType().GetProperty("Grade")?.GetValue(userInfo)?.ToString();
                var division = userInfo.GetType().GetProperty("Division")?.GetValue(userInfo)?.ToString();

                // Open a connection to the database
                await _connection.OpenAsync();

                // Check if the record already exists
                var checkQuery = $"SELECT COUNT(*) FROM UserInfo WHERE ServiceNo = @ServiceNo";
                using (var commandCheck = new SqlCommand(checkQuery, _connection))
                {
                    commandCheck.Parameters.AddWithValue("@ServiceNo", serviceNo);
                    if ((int)await commandCheck.ExecuteScalarAsync() == 0)
                    {
                        var insertQuery = "INSERT INTO UserInfo (Sub, Name, ServiceNo, Designation, Phone, Section, Groups, Email, Grade, Division, CreatedAt, IsCurrent) VALUES (@Sub, @Name, @ServiceNo, @Designation, @Phone, @Section, @Groups, @Email, @Grade, @Division, @CreatedAt, @IsCurrent)";
                        using (var commandInsert = new SqlCommand(insertQuery, _connection))
                        {
                            commandInsert.Parameters.AddWithValue("@Sub", sub);
                            commandInsert.Parameters.AddWithValue("@Name", name);
                            commandInsert.Parameters.AddWithValue("@ServiceNo", serviceNo);
                            commandInsert.Parameters.AddWithValue("@Designation", designation);
                            commandInsert.Parameters.AddWithValue("@Phone", phone);
                            commandInsert.Parameters.AddWithValue("@Section", section);
                            commandInsert.Parameters.AddWithValue("@Groups", group);
                            commandInsert.Parameters.AddWithValue("@Email", email);
                            commandInsert.Parameters.AddWithValue("@Grade", grade);
                            commandInsert.Parameters.AddWithValue("@Division", division);
                            commandInsert.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            commandInsert.Parameters.AddWithValue("@IsCurrent", 1);

                            await commandInsert.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        var checkQuerys = $@"
IF EXISTS (
    SELECT 1 FROM UserInfo
    WHERE ServiceNo = @ServiceNo AND IsCurrent = 1 AND (
        Sub != @Sub OR Name != @Name OR Designation != @Designation OR Phone != @Phone OR Section != @Section OR Groups != @Groups OR Email != @Email OR Grade != @Grade OR Division != @Division
    )
)
BEGIN
    UPDATE UserInfo SET IsCurrent = 0 WHERE ServiceNo = @ServiceNo;

    INSERT INTO UserInfo (Sub, Name, ServiceNo, Designation, Phone, Section, Groups, Email, Grade, Division, CreatedAt, IsCurrent)
    VALUES (@Sub, @Name, @ServiceNo, @Designation, @Phone, @Section, @Groups, @Email, @Grade, @Division, @CreatedAt, @IsCurrent);
END
";

                        using (var command = new SqlCommand(checkQuerys, _connection))
                        {
                            command.Parameters.AddWithValue("@Sub", sub);
                            command.Parameters.AddWithValue("@Name", name);
                            command.Parameters.AddWithValue("@ServiceNo", serviceNo);
                            command.Parameters.AddWithValue("@Designation", designation);
                            command.Parameters.AddWithValue("@Phone", phone);
                            command.Parameters.AddWithValue("@Section", section);
                            command.Parameters.AddWithValue("@Groups", group);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Grade", grade);
                            command.Parameters.AddWithValue("@Division", division);
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            command.Parameters.AddWithValue("@IsCurrent", 1);

                            await command.ExecuteNonQueryAsync();
                        }

                    }
                }


                // Close the connection
                _connection.Close();

                // Log success or perform any other necessary actions
                _logger.LogInformation("User information stored successfully");

                // Return a success view or redirect to another page
                return View("StoreUserInfoSuccess");
            }
            catch (Exception ex)
            {
                // Log the actual exception message along with other relevant information
                _logger.LogError($"Error storing user information in the database. Exception: {ex.Message}");

                // Return an error view
                return View("Error");
            }
        }




        public async Task<ActionResult> GetToken(string code, string state)
        {
            string Code1 = code;
            string State1 = state;
            string clientId = "GatePass";
            string clientSecret = "1b1900640e8f4842818d0ec8ea518ba6";
            string preBase64 = clientId + ":" + clientSecret;

            var messageBytes = Encoding.UTF8.GetBytes(preBase64);
            var encodeMessage = Convert.ToBase64String(messageBytes);

            string baseUrl = "https://oneidentitytest.slt.com.lk/authServer/token";

            using (HttpClient client = new HttpClient())
            {
                // Adding parameters to the URL
                string urlWithParams = baseUrl + "?" + "grant_type=authorization_code&code=" + Code1 + "&" + "redirect_uri=https://220.247.224.226:9301/SLTLogin/SLTLoginRedirect";

                client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodeMessage);

                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(urlWithParams, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(responseContent);
                    var idToken = jo["id_token"].ToString();
                    var jwtEncodedString = idToken;

                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);

                    if (jo["access_token"] != null && jo["token_type"] != null)
                    {
                        var accessToken = jo["access_token"].ToString();
                        var tokenType = jo["token_type"].ToString();
                        return View("Index", accessToken); // Return the access token to the view
                    }
                    else
                    {
                        _logger.LogError("Request failed"); ;
                        return View("Error"); // Return an error view
                    }
                }
                else
                {
                    Console.WriteLine("Request failed");
                    return View("Error"); // Return an error view
                }
            }
        }

        public async Task<ActionResult> GetUserInfo(string token)
        {
            string baseUrl = "https://oneidentitytest.slt.com.lk/authServer/userinfo";

            using (HttpClient client = new HttpClient())
            {
                // Set the Authorization header with the token
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                // Send a GET request to the userinfo endpoint
                var response = await client.GetAsync(baseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(responseContent);

                    // Extract the user info from the response
                    var sub = jo["sub"].ToString();
                    var name = jo["name"].ToString();
                    var serviceNo = jo["serviceNo"].ToString();
                    var designation = jo["designation"].ToString();
                    var phone = jo["phone"].ToString();
                    var section = jo["section"].ToString();
                    var group = jo["group"].ToString();
                    var email = jo["email"].ToString();
                    var grade = jo["grade"].ToString();
                    var division = jo["division"].ToString();


                    // Create a new object with the user info
                    var userInfo = new
                    {
                        Sub = sub,
                        Name = name,
                        ServiceNo = serviceNo,
                        Designation = designation,
                        Phone = phone,
                        Section = section,
                        Group = group,
                        Email = email,
                        Grade = grade,
                        Division = division
                    };


                    // Return the user info to the view
                    return View("UserSuccessPage", userInfo);
                }
                else
                {
                    Console.WriteLine("Request failed");
                    return View("Error"); // Return an error view
                }
            }
        }



        
    }
}


