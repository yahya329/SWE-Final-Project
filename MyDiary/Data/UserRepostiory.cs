using System;
using Oracle.ManagedDataAccess.Client;
using MyDiary.Models;

namespace MyDiary.Data
{
    internal class UserRepostiory
    {
        // Find a user by username — login
        public User FindByUsername(string username)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT UserID, Username, Password FROM Users WHERE Username = :uname";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":uname", OracleDbType.Varchar2).Value = username;

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    Password = reader.GetString(2)
                                };
                            }
                        }
                    }
                }
                
            }


            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                Console.WriteLine("Error in FindByUsername: " + ex.Message);
            }

            return null; // user not found

        }

        // Insert a new user — registration 
        public bool Register(string username, string hashedPassword)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Users (Username, Password)
                               VALUES (:uname, :pwd)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":uname", OracleDbType.Varchar2).Value = username;
                        cmd.Parameters.Add(":pwd", OracleDbType.Varchar2).Value = hashedPassword;

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }

            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                Console.WriteLine("Error in Register: " + ex.Message);
                return false;

            }
        }
    }
}
