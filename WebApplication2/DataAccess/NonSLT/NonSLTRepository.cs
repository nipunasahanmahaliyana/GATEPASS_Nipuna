using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GatePass.DataAccess.ItemCategory
{
    public class NonSLTRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<NonSLTRepository> _logger;
        private readonly SqlConnection _connection;

        public NonSLTRepository(IConfiguration configuration, ILogger<NonSLTRepository> logger, SqlConnection connection)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _connection = connection;
        }

        public List<RolesModel> GetRoles()
        {
            List<RolesModel> roles = new List<RolesModel>();
            string fetchRolesSql = "SELECT Role_id, Role_duty FROM Roles";

            using (SqlCommand cmd = new SqlCommand(fetchRolesSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RolesModel role = new RolesModel
                        {
                            Role_id = Convert.ToInt32(reader["Role_id"]),
                            Role_duty = reader["Role_duty"].ToString()
                        };
                        roles.Add(role);
                    }
                }
                _connection.Close();
            }

            return roles;
        }

        public List<NonSLTEmployeeModel> GetNonSLTEmployees()
        {

            List<NonSLTEmployeeModel> nonemployees = new List<NonSLTEmployeeModel>();
            string fetchLocationsSql = "SELECT Non_slt_Id, Role_id, Non_slt_name, NIC FROM Non_SLT_Users";

            using (SqlCommand cmd = new SqlCommand(fetchLocationsSql, _connection))
            {
                _connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NonSLTEmployeeModel nonemployee = new NonSLTEmployeeModel
                        {
                            Non_slt_Id = reader["Non_slt_Id"].ToString(),
                            Role_id = Convert.ToInt32(reader["Role_id"]),
                            Non_slt_name = reader["Non_slt_name"].ToString(),
                            NIC = reader["NIC"].ToString()
                        };
                        nonemployees.Add(nonemployee);
                    }
                }
                _connection.Close();
            }

            return nonemployees;
        }

        public string NewNonSLTEmployee(NonSLTEmployeeModel model)
        {
            // Validation to Check if any of the fields are empty or null
            if (string.IsNullOrWhiteSpace(model.Non_slt_name) || model.Role_id == 0 || string.IsNullOrWhiteSpace(model.NIC))
            {
                return "All fields must be filled out";
            }

            // Use the new Loc_id in your insert operation
            string sql = "INSERT INTO Non_SLT_Users (Role_id, Non_slt_name, NIC) VALUES (@Role_id, @Non_slt_name, @NIC)";

            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                _connection.Open();
                command.Parameters.AddWithValue("@Non_slt_name", model.Non_slt_name);
                command.Parameters.AddWithValue("@Role_id", model.Role_id);
                command.Parameters.AddWithValue("@NIC", model.NIC);

                command.ExecuteNonQuery();
                _connection.Close();
            }

            return "Non-SLT Employee added successfully.";
        }

        public string DeleteNonSLTEmployee(int Non_slt_id)
        {
            string deleteSql = "DELETE FROM Non_SLT_Users WHERE Non_slt_Id = @NonSLTId";

            using (SqlCommand command = new SqlCommand(deleteSql, _connection))
            {
                command.Parameters.AddWithValue("@NonSLTId", Non_slt_id);

                try
                {
                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    _connection.Close();

                    if (rowsAffected > 0)
                    {
                        // Deletion was successful
                        return "Employee deleted successfully.";
                    }
                    else
                    {
                        // Category with the specified ID not found
                        return "Employee with the specified ID not found.";
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the deletion process
                    return "An error occurred while deleting the employee: " + ex.Message;
                }
            }
        }

    }
}