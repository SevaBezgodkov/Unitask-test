using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using EmployeeService.Models;
using Newtonsoft.Json;

namespace EmployeeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IEmployeeService
    {
        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EmployeeDB;Integrated Security=True;";
        public bool GetEmployeeById(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    if(connection == null)
                    {
                        return false;
                    }
                    connection.Open();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.CommandText = $"SELECT * FROM Employee WHERE Id=@Id";
                        command.Connection = connection;
                        

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var employee = new EmployeeModel
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : string.Empty,
                                    ManagerId = reader["ManagerId"] != DBNull.Value ? Convert.ToInt32(reader["ManagerId"]) : 0,
                                    Enable = reader["Enable"] != DBNull.Value && (bool)reader["Enable"]
                                };

                                string employeeJson = JsonConvert.SerializeObject(employee, Formatting.Indented);

                                HttpContext.Current.Response.ContentType = "application/json";
                                HttpContext.Current.Response.Write(employeeJson + '\n');
                                return true;
                            }
                            else
                            {
                                HttpContext.Current.Response.StatusCode = 404;
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

      

        public void EnableEmployee(int id, int enable)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    object lockObject = new object();
                    // Проверка наличия сотрудника 
                    using (SqlCommand checkCommand = new SqlCommand())
                    {
                        checkCommand.Parameters.AddWithValue("@Id", id);
                        checkCommand.CommandText = "SELECT COUNT(*) FROM Employee WHERE Id=@Id";
                        checkCommand.Connection = connection;

                        var result = checkCommand.ExecuteScalar();
                        int employeeCount = result != null ? Convert.ToInt32(result) : 0;

                        if (employeeCount == 0)
                        {
                            HttpContext.Current.Response.ContentType = "application/json";
                            HttpContext.Current.Response.Write($"Employee with ID:{id} not found.");
                            return;
                        }

                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@Enable", enable);
                        command.CommandText = "UPDATE Employee SET Enable=@Enable WHERE Id=@Id";
                        command.Connection = connection;

                        HttpContext.Current.Response.ContentType = "application/json";
                        HttpContext.Current.Response.Write($"The 'enable' column for the employee with ID:{id} has been changed successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new NullReferenceException(ex.Message);
            }
        }
    }
}