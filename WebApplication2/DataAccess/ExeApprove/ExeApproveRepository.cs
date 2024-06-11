using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GatePass.DataAccess.ExeApprove
{
    public class ExeApproveRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ExeApproveRepository> _logger;

        public ExeApproveRepository(IConfiguration configuration, ILogger<ExeApproveRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public List<ExeApproveModel> ExeApprove(string serviceNo)
        {
            List<ExeApproveModel> requests = new List<ExeApproveModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
    "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no, " +
    "ui.Name " +
    "FROM Requests r " +
    "INNER JOIN UserInfo ui ON r.Sender_service_no = ui.ServiceNo " +
    "WHERE r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = 1) AND r.ExO_service_no = @ServiceNo ORDER BY Created_date DESC";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ServiceNo", serviceNo);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ExeApproveModel request = new ExeApproveModel
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Sender_service_no = reader.GetString(1),
                                    In_location_name = reader.GetString(2),
                                    Out_location_name = reader.GetString(3),
                                    Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                    Created_date = reader.GetDateTime(5),
                                    ExO_service_no = reader.GetString(6),
                                    Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                    Name = reader.GetString(8)
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
                _logger.LogError(ex, "An error occurred while retrieving requests for verification.");
                throw;
            }
        }

        public void Approve(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 2, Update_date = GETDATE(), Progress_status = 'Executive Approved', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while approving the request.");
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
                    string updateQuery = "UPDATE Workprogress SET Stage_id = 3, Update_date = GETDATE(),  Any_comment = @rejectComment, Progress_status = 'Executive Rejected', Viewed = 0 WHERE Request_ref_no = @requestRefNo";

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
                _logger.LogError(ex, "An error occurred while rejecting the request.");
                throw;
            }
        }

        public int GetComments()
        {
            int commentCount = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(Viewed) as CommentCount FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0";

                try
                {
                    using (SqlCommand command = new SqlCommand(countQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                commentCount = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving the comment count.");
                    throw;
                }
            }

            return commentCount;
        }

        public List<Workprogress> GetWorkProgressDataComments()
        {
            List<Workprogress> workProgressData = new List<Workprogress>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Request_ref_no, Stage_id, Progress_status, Update_date, Any_comment " +
                               "FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0 " +
                               "ORDER BY Update_date DESC";  // Order by Update_date in descending order

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Workprogress progress = new Workprogress
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Stage_id = reader.GetInt32(1),
                                    Progress_status = reader.GetString(2),
                                    Update_date = reader.GetDateTime(3),
                                    Any_comment = reader.IsDBNull(4) ? null : reader.GetString(4)
                                };

                                workProgressData.Add(progress);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving work progress data with comments.");
                    throw;
                }
            }

            return workProgressData;
        }

        public List<ExeApproveModel> GetDetailsById(int id)
        {
            List<ExeApproveModel> itemsList = new List<ExeApproveModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT i.Item_id, i.Item_serial_no, i.Item_name, i.Item_description, i.Returnable_status, " +
                            " i.Request_ref_no,  i.Attaches " +
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
                                ExeApproveModel request = new ExeApproveModel
                                {
                                    Item_id = reader.GetInt32(0), // Include Item_id
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Returnable_status = reader.GetString(4),
                                    Request_ref_no = reader.GetInt32(5), // Include Request_ref_no
                                    Attaches = reader["Attaches"] as byte[],
                                };

                                itemsList.Add(request);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving item details.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return itemsList;
        }

        public int GetCommentCount()
        {
            int commentCount = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(Viewed) as CommentCount FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0";

                try
                {
                    using (SqlCommand command = new SqlCommand(countQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                commentCount = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving the comment count.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return commentCount;
        }

        public List<Workprogress> GetWorkProgressDataWithComments()
        {
            List<Workprogress> workProgressData = new List<Workprogress>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Request_ref_no, Stage_id, Progress_status, Update_date, Any_comment " +
                               "FROM Workprogress WHERE Any_comment IS NOT NULL AND Viewed = 0 " +
                               "ORDER BY Update_date DESC";  // Order by Update_date in descending order

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Workprogress progress = new Workprogress
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Stage_id = reader.GetInt32(1),
                                    Progress_status = reader.GetString(2),
                                    Update_date = reader.GetDateTime(3),
                                    Any_comment = reader.IsDBNull(4) ? null : reader.GetString(4)
                                };

                                workProgressData.Add(progress);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving work progress data with comments.");
                    // Handle the error as needed, e.g., return an error view or redirect to an error page.
                }
            }

            return workProgressData;
        }

        public void MarkNotificationAsViewed(int requestRefNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Workprogress SET Viewed = 1 WHERE Request_ref_no = @requestRefNo";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@requestRefNo", requestRefNo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Viewed column.");
                throw;
            }
        }

        public List<ExeApproveModel> GetRequestsByStageId(int stageId)
        {
            List<ExeApproveModel> requests = new List<ExeApproveModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
            SELECT DISTINCT
                r.Request_ref_no, 
                r.Sender_service_no, 
                r.In_location_name, 
                r.Out_location_name, 
                r.Receiver_service_no, 
                r.Created_date, 
                r.ExO_service_no, 
                r.Carrier_nic_no, 
                ui.Name 
            FROM 
                Requests r
                INNER JOIN UserInfo ui ON r.Sender_service_no = ui.ServiceNo
            WHERE 
                r.Request_ref_no IN (SELECT Request_ref_no FROM Workprogress WHERE Stage_id = @stageId)";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@stageId", stageId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ExeApproveModel request = new ExeApproveModel
                                {
                                    Request_ref_no = reader.GetInt32(0),
                                    Sender_service_no = reader.GetString(1),
                                    In_location_name = reader.GetString(2),
                                    Out_location_name = reader.GetString(3),
                                    Receiver_service_no = reader.IsDBNull(4) ? "No Specific Receiver" : reader.GetString(4),
                                    Created_date = reader.GetDateTime(5),
                                    ExO_service_no = reader.GetString(6),
                                    Carrier_nic_no = reader.IsDBNull(7) ? "No Specific Carrier" : reader.GetString(7),
                                    Name = reader.GetString(8)
                                };

                                requests.Add(request);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while retrieving requests with stage_id {stageId}.");
                    throw;
                }
            }

            return requests;
        }

        

    }

}
