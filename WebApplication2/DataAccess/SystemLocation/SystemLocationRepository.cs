using GatePass_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GatePass.DataAccess.SystemLocation
{
    public class SystemLocationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SystemLocationRepository> _logger;
        private readonly SqlConnection _connection;

        public SystemLocationRepository(IConfiguration configuration, ILogger<SystemLocationRepository> logger, SqlConnection connection)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
            _connection = connection;
        }

        public List<LocationsModel> GetSystemLocations()
        {
            List<LocationsModel> locations = new List<LocationsModel>();
            string fetchLocationsSql = "SELECT Loc_id, Loc_name FROM Locations WHERE IsDeleted IS NULL";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(fetchLocationsSql, connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LocationsModel location = new LocationsModel
                            {
                                Loc_id = Convert.ToInt32(reader["Loc_id"]),
                                Loc_name = reader["Loc_name"].ToString()
                            };
                            locations.Add(location);
                        }
                    }
                }
                connection.Close();
            }

            return locations;
        }

        public string UploadLocationCSV(IFormFile csvFile)
        {
            // Validation to check if csv file selected
            if (csvFile == null || csvFile.Length == 0)
            {
                return "No CSV file selected for upload.";
            }

            try
            {
                // Using the StreamReader class to read characters from the csv file
                using (var streamReader = new StreamReader(csvFile.OpenReadStream()))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var line = streamReader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            // Validation to check if the CSV file has more than one column
                            if (line.Split(',').Length != 1)
                            {
                                return "Invalid CSV file format. Location Data can only contain Location Name.";
                            }

                            // using trim to get rid of any whitespaces
                            string locName = line.Trim();

                            // Check if the location already exists
                            if (LocationExists(locName))
                            {
                                return $"Location '{locName}' already exists. Duplicate values are not allowed.";
                            }

                            string insertSql = "INSERT INTO Locations (Loc_name) VALUES (@LocName)";

                            using (var sqlCommand = new SqlCommand(insertSql, _connection))
                            {
                                sqlCommand.Parameters.AddWithValue("@LocName", locName);

                                _connection.Open();
                                sqlCommand.ExecuteNonQuery();
                                _connection.Close();
                            }
                        }
                    }
                }

                return "CSV data imported successfully into Locations.";
            }
            catch (Exception ex)
            {
                return "An error occurred while importing CSV data into Locations: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }
        }

        public bool LocationExists(string locName)
        {
            string query = "SELECT COUNT(*) FROM Locations WHERE Loc_name = @LocName";
            using (var sqlCommand = new SqlCommand(query, _connection))
            {
                sqlCommand.Parameters.AddWithValue("@LocName", locName);
                _connection.Open();
                int count = (int)sqlCommand.ExecuteScalar();
                _connection.Close();
                return count > 0;
            }
        }

        public string NewSystemLocation(LocationsModel model)
        {
            // Validation to Check if the Loc_name is empty or null
            if (string.IsNullOrWhiteSpace(model.Loc_name))
            {
                return "Location name cannot be empty.";
            }

            // Check if the location name already exists in the database
            string checkSql = "SELECT COUNT(*) FROM Locations WHERE Loc_name = @Loc_name";

            using (SqlCommand checkCommand = new SqlCommand(checkSql, _connection))
            {
                checkCommand.Parameters.AddWithValue("@Loc_name", model.Loc_name);
                _connection.Open();

                int existingCount = (int)checkCommand.ExecuteScalar();

                _connection.Close();

                if (existingCount > 0)
                {
                    return "Location with the same name already exists.";
                }
            }

            // Use the new Loc_id in your insert operation
            string sql = "INSERT INTO Locations (Loc_name) VALUES (@Loc_name)";

            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@Loc_name", model.Loc_name);

                _connection.Open();
                command.ExecuteNonQuery();
                _connection.Close();
            }

            return "Location created successfully.";
        }

        public string DeleteLocation(int locationId)
        {
            string updateSql = "UPDATE Locations SET IsDeleted = 1 WHERE Loc_id = @LocationId";

            using (SqlCommand command = new SqlCommand(updateSql, _connection))
            {
                command.Parameters.AddWithValue("@LocationId", locationId);

                try
                {
                    _connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    _connection.Close();

                    if (rowsAffected > 0)
                    {
                        // Soft deletion was successful
                        return "Location soft-deleted successfully.";
                    }
                    else
                    {
                        // Location with the specified ID not found
                        return "Location with the specified ID not found.";
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the soft deletion process
                    return "An error occurred while soft-deleting the location: " + ex.Message;
                }
            }

        }
    }
}