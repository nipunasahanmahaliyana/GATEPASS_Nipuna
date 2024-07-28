using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace GatePass.DataAccess.ItemCategory
{
    public class NewRequestRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewRequestRepository> _logger;
        private readonly IEmailSender _emailSender;

        public NewRequestRepository(IConfiguration configuration, ILogger<NewRequestRepository> logger,IEmailSender emailSender)
        {
            _configuration = configuration;
            _logger = logger;
            _emailSender = emailSender;
        }

        public List<string> GetItemCategories()
        {
            List<string> itemCategories = new List<string>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Category_name FROM Item_Category", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            itemCategories.Add(reader["Category_name"].ToString());
                        }
                    }
                }
            }
            return itemCategories;
        }

        public List<string> GetLocations()
        {
            List<string> locations = new List<string>();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Loc_name FROM Locations WHERE IsDeleted IS NULL", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            locations.Add(reader["Loc_name"].ToString());
                        }
                    }
                }
            }
            return locations;
        }

        public object GetUserInfoDetails(string serviceNo)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var query = $@"SELECT Sub, Name, ServiceNo, " +
                                $"Designation, Phone, Section, " +
                                $"Groups, Email, Grade, Division " +
                                $"FROM UserInfo WHERE ServiceNo = @serviceNo  AND IsCurrent = 1";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@serviceNo", serviceNo);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                if (reader.Read())
                                {
                                    var data = new
                                    {
                                        sub = reader["Sub"].ToString(),
                                        name = reader["Name"].ToString(),
                                        serviceNo = reader["ServiceNo"].ToString(),
                                        designation = reader["Designation"].ToString(),
                                        phone = reader["Phone"].ToString(),
                                        section = reader["Section"].ToString(),
                                        group = reader["Groups"].ToString(),
                                        email = reader["Email"].ToString(),
                                        grade = reader["Grade"].ToString(),
                                        division = reader["Division"].ToString()
                                    };

                                    return data;
                                }
                            }
                            else
                            {
                                return new { error = "Invalid Service Number" };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the request");
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }

            return null;
        }

        public object GetExecutiveOfficers(string group)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var query = $@"SELECT Name FROM ExecutiveInfo WHERE Groups = @group";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@group", group);

                        using (var reader = command.ExecuteReader())
                        {
                            var officers = new List<string>();

                            while (reader.Read())
                            {
                                officers.Add(reader["Name"].ToString());
                            }

                            if (officers.Count > 0)
                            {
                                return officers;
                            }
                            else
                            {
                                return new { error = "No executive officers found for this group" };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the request");
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }

            return null;
        }

        public object GetReceiverDetails(string serviceNo)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    var query = $"SELECT Employee_group_name, CONCAT(Employee_firstname, ' ', Employee_surname) AS ReceiverName, Employee_mobile_phone " +
                                $"FROM TempUsers WHERE Employee_number = @serviceNo";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@serviceNo", serviceNo);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var data = new
                                {
                                    group = reader["Employee_group_name"].ToString(),
                                    name = reader["ReceiverName"].ToString(),
                                    contactNo = reader["Employee_mobile_phone"].ToString()
                                };

                                return data;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the request");
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }

            return null;
        }

        public string GetEmployeeServiceNoByName(string ExecutiveOfficer)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve the Employee number by name
                var query = "SELECT ServiceNo FROM ExecutiveInfo WHERE Name = @ExecutiveOfficerName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ExecutiveOfficerName", ExecutiveOfficer);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return result.ToString();
#pragma warning restore CS8603 // Possible null reference return.
                    }
                }
            }

            // Return a default value if not found (you may want to handle this differently)
            return "Unknown";
        }

        public string GetExecEmail(string ExecutiveOfficer)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve the Employee number by name
                var query = "SELECT Email FROM ExecutiveInfo WHERE ServiceNo = @ExecutiveOfficerServiceNo";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ExecutiveOfficerServiceNo", ExecutiveOfficer);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
#pragma warning disable CS8603 // Possible null reference return.
                        return result.ToString();
#pragma warning restore CS8603 // Possible null reference return.
                    }
                }
            }

            // Return a default value if not found (you may want to handle this differently)
            return "Unknown";
        }

        public int GetRequestRefNoForItem()
        {
            // Implement logic to retrieve the RequestRefNo from the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve RequestRefNo
                var query = "SELECT MAX(Request_ref_no) + 1 FROM Requests";

                using (var command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }


        public int GetCategoryIdForItem(string itemCategory)
        {
            // Implement logic to retrieve the CategoryId based on itemCategory
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve CategoryId based on itemCategory
                var query = "SELECT Category_id FROM Item_Category WHERE Category_name = @CategoryName";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", itemCategory);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }


        public int GetItemIdForAttach()
        {
            // Implement logic to retrieve the RequestRefNo from the database
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                // Construct a SQL query to retrieve RequestRefNo
                var query = "SELECT MAX(Item_id) + 1 FROM Items";

                using (var command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 1;
        }

        public async Task<int> ProcessRequest(bool VerifiedItems, IFormCollection form, IFormFileCollection annexLoc, IConfiguration _configuration)
        {
            // Access the form data
            var formData = form;

            // Parse the requestData and carrierData dictionaries
            Dictionary<string, string> requestData = JsonConvert.DeserializeObject<Dictionary<string, string>>(formData["requestData"]);
            Dictionary<string, string> carrierData = JsonConvert.DeserializeObject<Dictionary<string, string>>(formData["carrierData"]);

            // Parse the itemDetails list
            List<Dictionary<string, string>> itemDetails = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(formData["itemDetails"]);

            // Access the uploaded files
            var files = annexLoc;

            int isdone = 0;
            string serviceNo = null;
            string ExecutiveOfficer = null;
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                int requestRefNo = GetRequestRefNoForItem();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Begin the transaction
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Use the requestData and carrierData dictionaries
                            serviceNo = requestData["ServiceNo"];
                            string sltInLocation = requestData["SLTInLocation"];
                            string outLocation = requestData["OutLocation"];
                            ExecutiveOfficer = requestData["ExecutiveOfficer"];
                            string nicNo = requestData["NICNo"];
                            string receiverServiceNo = requestData["ReceiverServiceNo"];

                            string executiveOfficer = GetEmployeeServiceNoByName(ExecutiveOfficer);

                            // Additional Query 2: Insert into Carrier_Details
                            if (!string.IsNullOrEmpty(carrierData["NICNo"]))
                            {
                                string insertSql2 = "INSERT INTO Carrier_Details (Carrier_name, NIC_no, Contact_no ,travel_date ,vehicle_no) VALUES (@Carriername, @Carriernic, @Carriercontact,@CarrierTravel,@CarrierVehicle)";

                                using (SqlCommand cmd2 = new SqlCommand(insertSql2, connection, transaction))
                                {
                                    cmd2.Transaction = transaction;

                                    cmd2.Parameters.AddWithValue("@Carriername", carrierData["CarrierName"]);
                                    cmd2.Parameters.AddWithValue("@Carriernic", carrierData["NICNo"]);
                                    cmd2.Parameters.AddWithValue("@Carriercontact", carrierData["Contactno"]);
                                    cmd2.Parameters.AddWithValue("@CarrierTravel", carrierData["Travedate"]);
                                    cmd2.Parameters.AddWithValue("@CarrierVehicle", carrierData["Vehicleno"]);

                                    int rowsAffected = cmd2.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        isdone = 1;
                                    }
                                    else
                                    {
                                        isdone = -1; // Carrier details insertion failed
                                    }
                                }
                            }

                            // Additional Query 4: Insert into Requests
                            string insertSql4 = "INSERT INTO Requests (Sender_service_no, In_location_name, Out_location_name, Receiver_service_no, Created_date, ExO_service_no, Carrier_nic_no) VALUES (@ServiceNo, @SLTInLocation, @OutLocation, @Receiver_service_no, @dates, @ExecutiveOfficerServiceNo, @NICNo)";

                            using (SqlCommand cmd4 = new SqlCommand(insertSql4, connection, transaction))
                            {
                                cmd4.Transaction = transaction;

                                cmd4.Parameters.AddWithValue("@ServiceNo", serviceNo);
                                cmd4.Parameters.AddWithValue("@SLTInLocation", sltInLocation);
                                cmd4.Parameters.AddWithValue("@OutLocation", outLocation);
                                cmd4.Parameters.AddWithValue("@Receiver_service_no", string.IsNullOrEmpty(receiverServiceNo) || receiverServiceNo == "null" ? (object)DBNull.Value : receiverServiceNo);
                                cmd4.Parameters.AddWithValue("@dates", DateTime.Now);
                                cmd4.Parameters.AddWithValue("@ExecutiveOfficerServiceNo", executiveOfficer);
                                cmd4.Parameters.AddWithValue("@NICNo", string.IsNullOrEmpty(nicNo) ? (object)DBNull.Value : nicNo);

                                int rowsAffected = cmd4.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    isdone = 3;
                                }
                                else
                                {
                                    isdone = -3; // Sender details insertion failed
                                }
                            }

                            // Additional Query 5: Insert into Workprogress
                            string insertSql5 = "INSERT INTO Workprogress (Request_ref_no, Stage_id, Progress_status, Update_date, Viewed) VALUES (@Requestrefno, @Stageid, @ProgressStatus, @Updatedate, @Viewed)";

                            using (SqlCommand cmd5 = new SqlCommand(insertSql5, connection, transaction))
                            {
                                cmd5.Transaction = transaction;

                                cmd5.Parameters.AddWithValue("@Requestrefno", requestRefNo);
                                cmd5.Parameters.AddWithValue("@Stageid", 1);
                                cmd5.Parameters.AddWithValue("@ProgressStatus", "Executive Pending");
                                cmd5.Parameters.AddWithValue("@Updatedate", DateTime.Now);
                                cmd5.Parameters.AddWithValue("@Viewed", 0);

                                int rowsAffected = cmd5.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    isdone = 4;
                                }
                                else
                                {
                                    isdone = -4; // Workprogress details insertion failed
                                }
                            }

                            // Loop through the itemDetails list
                            for (var i = 0; i < itemDetails.Count; i++)
                            {
                                var item = itemDetails[i];
                                var annex = annexLoc[i];

                                byte[] imageBytes;
                                using (var memoryStream = new MemoryStream())
                                {
                                    await annex.CopyToAsync(memoryStream);
                                    imageBytes = memoryStream.ToArray();
                                }

                                string imageBase64 = Convert.ToBase64String(imageBytes);

                                // Use the item dictionary
                                string itemName = item["ItemName"];
                                string itemSerialNo = item["ItemSerialNo"];
                                string itemQuantity = item["Quantity"];
                                string itemCategory = item["ItemCategory"];
                                string itemDescription = item["ItemDescription"];
                                string returnable = item["Returnable"];

                                int categoryId = GetCategoryIdForItem(itemCategory);

                                // Additional Query 3: Insert into Items
                                string insertSql3 = "INSERT INTO Items (Item_serial_no, Item_name, Item_description, Returnable_status, Created_date, Category_id, Request_ref_no, Verified_items, Attaches,Item_Quantity) VALUES (@Itemserialno, @Itemname, @Itemdescription, @Returnablestatus, @Duedate, @Categoryid, @Requestrefno, @Verifieditems, @Attaches,@itemQuantity)";

                                using (SqlCommand cmd3 = new SqlCommand(insertSql3, connection, transaction))
                                {
                                    cmd3.Transaction = transaction;

                                    cmd3.Parameters.AddWithValue("@Itemserialno", itemSerialNo);
                                    cmd3.Parameters.AddWithValue("@Itemname", itemName);
                                    cmd3.Parameters.AddWithValue("@Itemdescription", itemDescription);

                                    if (returnable == "Yes")
                                    {
                                        cmd3.Parameters.AddWithValue("@Returnablestatus", "Yes");
                                    }
                                    else
                                    {
                                        cmd3.Parameters.AddWithValue("@Returnablestatus", "No");
                                    }

                                    cmd3.Parameters.AddWithValue("@Duedate", DateTime.Now);
                                    cmd3.Parameters.AddWithValue("@Categoryid", categoryId);
                                    cmd3.Parameters.AddWithValue("@Requestrefno", requestRefNo); // Need to define requestRefNo
                                    cmd3.Parameters.AddWithValue("@Verifieditems", VerifiedItems); // Define VerifiedItems
                                    cmd3.Parameters.AddWithValue("@itemQuantity", itemQuantity);
                                    cmd3.Parameters.Add("@Attaches", SqlDbType.VarBinary, -1).Value = imageBytes;

                                    int rowsAffected = cmd3.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        isdone = 2;
                                    }
                                    else
                                    {
                                        isdone = -2; // Item details insertion failed
                                    }
                                }
                            }
                          

                            transaction.Commit();
                            isdone = 1; // Success
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if any command fails
                            transaction.Rollback();
                            Console.WriteLine(ex.StackTrace);
                            isdone = -1; // Error
                        }
                    }
                }
                try
                {
                    string executiveOfficerServiceNo = GetEmployeeServiceNoByName(ExecutiveOfficer);
                    string email = GetExecEmail(executiveOfficerServiceNo);

                    var subject = "Excutive Approvel";
                    var message = $"Requesting your approvel from service number {serviceNo}: ,please login GATE PASS System";

                    await _emailSender.SendEMailAsync(email, subject, message);
                    Console.WriteLine("Message not sent");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Message not sent");
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return isdone = -14; // Error
            }

            return isdone = 14;
        }


        public string GetResultMessage(int isdone)
        {
            switch (isdone)
            {
                case 1:
                    return "Item 1 added successfully";
                case 2:
                    return "Item 2 added successfully";
                case 3:
                    return "Item 3 added successfully";
                case 4:
                    return "Item 4 added successfully";
                case 5:
                    return "Item 5 added successfully";
                case 6:
                    return "Item 6 added successfully";
                case 7:
                    return "Item 7 added successfully";
                case 8:
                    return "Item 8 added successfully";
                case 9:
                    return "Item 9 added successfully";
                case 10:
                    return "Item 10 added successfully";
                // Add cases for other possible values of isdone

                case -1:
                    return "Error adding Item 1";
                case -2:
                    return "Error adding Item 2";
                case -3:
                    return "Error adding Item 3";
                case -4:
                    return "Error adding Item 4";
                case -5:
                    return "Error adding Item 5";
                case -6:
                    return "Error adding Item 6";
                case -7:
                    return "Error adding Item 7";
                case -8:
                    return "Error adding Item 8";
                case -9:
                    return "Error adding Item 9";
                case -10:
                    return "Error adding Item 10";
                // Add cases for other possible error values of isdone

                default:
                    return "Unknown result";
            }
        }

    }
}