using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using My_receipt_gate_pass.Models;
using System.Data.SqlClient;

namespace GatePass.DataAccess.MyReceipt
{
    public class MyReceiptRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MyReceiptRepository> _logger;

        public MyReceiptRepository(IConfiguration configuration, ILogger<MyReceiptRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public List<MyreceiptModel> Index(string session_no)
        {
            string query = "";
            List<MyreceiptModel> requests = new List<MyreceiptModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                  
                        query = "SELECT DISTINCT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
                            "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no," +
                            "ui.Name " +
                            "FROM Requests r " +
                            "INNER JOIN UserInfo ui ON r.Sender_service_no = ui.ServiceNo " +
                            "WHERE r.Receiver_service_no IS NOT NULL  AND r.Receiver_service_no = @session_no " +
                            "AND r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 7 )";



                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@session_no", session_no);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MyreceiptModel request = new MyreceiptModel
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Sender_service_no = reader.GetString(1),
                                    In_location_name = reader.GetString(2),
                                    Out_location_name = reader.GetString(3),
                                    Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                    Created_date = reader.GetDateTime(5),
                                    ExO_service_no = reader.GetString(6),
                                    Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                    Name = reader.GetString(8),
                                };

                                requests.Add(request);
                            }
                        }
                    }
                }

                return requests;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for Index.");
                throw;
            }
        }

        public List<myreceiptdetailsModel> myreceiptdetails(int id)
        {
            List<myreceiptdetailsModel> requests = new List<myreceiptdetailsModel>();


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description,i.Item_Quantity, i.Returnable_status, i.Request_ref_no, i.Attaches " +
                        "FROM Items i " +
                        "WHERE i.Request_ref_no = @id";
                try
                {

                    using (SqlCommand command = new SqlCommand(query, connection))

                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                myreceiptdetailsModel request = new myreceiptdetailsModel
                                {
                                    Item_id = reader.GetInt32(0), // Include Item_id
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_description = reader.GetString(3),
                                    Item_Quantity = reader.GetInt32(4),
                                    Returnable_status = reader.GetString(5),
                                    Request_ref_no = reader.GetInt32(6), // Include Request_ref_no
                                    Attaches = reader["Attaches"] as byte[],
                                };

                                requests.Add(request);
                            }
                        }
                    }
                    return requests;

                }


                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching data for myreceiptdetails.");
                    throw;
                }
            }
        }

        public void Receive(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 9, Update_date = GETDATE(), Progress_status = 'Rc Received' WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Receive action.");
                throw;
            }
        }

        public void Reject(int requestRefNo, string rejectComment)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 10, Update_date = GETDATE(), Any_comment = @rejectComment, Progress_status = 'Rc Rejected' WHERE Request_ref_no = @requestRefNo";


                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.Parameters.AddWithValue("@rejectComment", rejectComment);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the Reject action.");
                throw;
            }
        }

    }
}