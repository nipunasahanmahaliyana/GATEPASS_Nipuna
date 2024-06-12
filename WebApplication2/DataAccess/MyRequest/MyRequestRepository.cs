using GatePass_Project.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace GatePass_Project.DataAccess.MyRequest
{
    public class MyRequestRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MyRequestRepository> _logger;

        public MyRequestRepository(IConfiguration configuration, ILogger<MyRequestRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public List<MyRequestModel> GetRequests(string serviceNo)
        {
            List<MyRequestModel> requests = new List<MyRequestModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT DISTINCT r.Request_ref_no, r.Sender_service_no, r.In_location_name, r.Out_location_name, " +
                        "r.Receiver_service_no, r.Created_date, r.ExO_service_no, r.Carrier_nic_no, " +
                        "ui.Name " +
                        "FROM Requests r " +
                        "INNER JOIN UserInfo ui ON r.Sender_service_no = ui.ServiceNo " +
                        "WHERE r.Sender_service_no = @ServiceNo ORDER BY r.Created_date DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ServiceNo", serviceNo);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MyRequestModel request = new MyRequestModel
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


        public MyRequestModel GetRequestStatusById(int id)
        {
            MyRequestModel requestStatus = new MyRequestModel();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Progress_status FROM Workprogress WHERE Request_ref_no = @id ORDER BY Update_date DESC";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requestStatus.Progress_status = reader["Progress_status"] as string;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving request status.");
                }
            }

            return requestStatus;
        }

        public int GetRequestRefNoById(int id)
        {
            int requestRefNo = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Request_ref_no FROM Requests WHERE Request_ref_no = @id";

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requestRefNo = reader.GetInt32(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving request reference number.");
                }
            }

            return requestRefNo;
        }

        public List<MyRequestModel> GetDetailsById(int id)
        {
            List<MyRequestModel> itemsList = new List<MyRequestModel>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT i.Item_id, i.Item_serial_no,i.Item_name, i.Item_description,i.Item_Quantity, i.Returnable_status, " +
                             "i.Request_ref_no, i.Attaches " +
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
                                MyRequestModel request = new MyRequestModel
                                {
                                    Item_id = reader.GetInt32(0),
                                    Item_serial_no = reader.GetString(1),
                                    Item_name = reader.GetString(2),
                                    Item_Description = reader.GetString(3),
                                    Item_Quantity = reader.GetInt32(4),
                                    Returnable_status = reader.GetString(5),
                                    Request_ref_no = reader.GetInt32(6),
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
                }
            }

            return itemsList;
        }

        

        //PDF
       // public MyRequestModel GetRequestDetailsByRefNo(int id)
        //{
           // MyRequestModel model = new MyRequestModel();

            //using (SqlConnection connection = new SqlConnection(_connectionString))
           // {

               // connection.Open();
                // query = "SELECT r.Sender_service_no,r.In_location_name,r.Out_location_name,r.Reciever_service_no,r.Created_date,r.ExO_service_no,r.Carrier_nic_no FROM Requests r WHERE r.Request_ref_no = @id";

              //  try
              //  {
                  //  using (SqlCommand command = new SqlCommand(query, connection))
                  //  {
                     //   command.Parameters.AddWithValue("@id", id);

                      //  using (SqlDataReader reader = command.ExecuteReader())
                       // {
                           // while (reader.Read())
                           // {

                              //  model.Sender_service_no = reader.GetString(0);
                              //  model.In_location_name = reader.GetString(1);
                               // model.Out_location_name = reader.GetString(2);
                               // model.Receiver_service_no = reader.GetString(3);
                               // model.Created_date = reader.GetDateTime(4);
                               // model.ExO_service_no = reader.GetString(5);
                               // model.Carrier_nic_no = reader.GetString(6);






                          //  }

        
                      // }
                  //  }
                //}
               // catch (Exception ex)
               // {
                   // _logger.LogError(ex, "An error occurred while retrieving item details.");
               // }
          //  }
          //  return model;

       // }

    }
}
