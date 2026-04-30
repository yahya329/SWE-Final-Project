using System;
using System.Collections.Generic;
using System.Data;
using MyDiary.Models;
using Oracle.ManagedDataAccess.Client;

namespace MyDiary.Data
{
    internal class DiaryRepository
    {
        public bool CreateEntry(DiaryEntry entry)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"INSERT INTO DiaryEntries (UserID, Title, Body, Mood, EntryDate)
                                   VALUES (:userid, :title, :body, :mood, :entrydate)";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = entry.UserID;
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = entry.Title;
                        cmd.Parameters.Add(":body", OracleDbType.Clob).Value = entry.Body;
                        cmd.Parameters.Add(":mood", OracleDbType.Varchar2).Value = entry.Mood;
                        cmd.Parameters.Add(":entrydate", OracleDbType.Date).Value = entry.EntryDate;

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating entry: " + ex.Message);
                return false;
            }
        }

        //Stored Procedure used multiple rows.
        public List<DiaryEntry> GetAllEntries(int userID)
        {
            try
            {
                var entries = new List<DiaryEntry>();

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("GetEntriesByUser", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_UserID", OracleDbType.Int32).Value = userID;

                        //Stored Procedure used multiple rows
                        cmd.Parameters.Add("p_Cursor", OracleDbType.RefCursor)
                                      .Direction = System.Data.ParameterDirection.Output;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                entries.Add(new DiaryEntry
                                {
                                    EntryID = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Mood = reader.GetString(2),
                                    EntryDate = reader.GetDateTime(3)
                                });
                            }
                        }
                    }
                }

                return entries;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving entries: " + ex.Message);
                return new List<DiaryEntry>();
            }

        }

        public bool UpdateEntry(DiaryEntry entry)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"UPDATE DiaryEntries 
                       SET Title = :title, 
                           Body  = :body, 
                           Mood  = :mood
                       WHERE EntryID = :entryid 
                       AND UserID = :userid";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":title", OracleDbType.Varchar2).Value = entry.Title;
                        cmd.Parameters.Add(":body", OracleDbType.Clob).Value = entry.Body;
                        cmd.Parameters.Add(":mood", OracleDbType.Varchar2).Value = entry.Mood;
                        cmd.Parameters.Add(":entryid", OracleDbType.Int32).Value = entry.EntryID;
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = entry.UserID;

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error updating entry: " + ex.Message);
                return false;
            }
        }

        public bool DeleteEntry(int entryID, int userID)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"DELETE FROM DiaryEntries 
                       WHERE EntryID = :entryid 
                       AND UserID = :userid";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":entryid", OracleDbType.Int32).Value = entryID;
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;

                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error deleting entry: " + ex.Message);
                return false;
            }
        }

        // FR-04: Search entries by keyword and/or date range (disconnected)
        public List<DiaryEntry> Search(int userID, string keyword, DateTime? startDate, DateTime? endDate)
        {
            var entries = new List<DiaryEntry>();
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    
                    string sql = @"SELECT EntryID, Title, Mood, EntryDate 
                           FROM DiaryEntries 
                           WHERE UserID = :userid";

                    if (!string.IsNullOrEmpty(keyword))
                        sql += " AND (UPPER(Title) LIKE UPPER(:keyword) OR UPPER(Body) LIKE UPPER(:keyword))";

                    if (startDate.HasValue && endDate.HasValue)
                        sql += " AND EntryDate BETWEEN :startdate AND :enddate";

                    sql += " ORDER BY EntryDate DESC";

                    
                    using (var adapter = new OracleDataAdapter(sql, conn))
                    {
                        
                        adapter.SelectCommand.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;

                        if (!string.IsNullOrEmpty(keyword))
                            adapter.SelectCommand.Parameters.Add(":keyword", OracleDbType.Varchar2).Value = "%" + keyword + "%";

                        if (startDate.HasValue && endDate.HasValue)
                        {
                            adapter.SelectCommand.Parameters.Add(":startdate", OracleDbType.Date).Value = startDate.Value;
                            adapter.SelectCommand.Parameters.Add(":enddate", OracleDbType.Date).Value = endDate.Value;
                        }

                        
                        DataSet ds = new DataSet();
                        adapter.Fill(ds, "Results");

                        
                        foreach (DataRow row in ds.Tables["Results"].Rows)
                        {
                            entries.Add(new DiaryEntry
                            {
                                EntryID = Convert.ToInt32(row["EntryID"]),
                                Title = row["Title"].ToString(),
                                Mood = row["Mood"].ToString(),
                                EntryDate = Convert.ToDateTime(row["EntryDate"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Disconnected Search Error: " + ex.Message);
            }
            return entries;
        }

        public Dictionary<string, int> GetMoodSummary(int userID)
        {
            
            var summary = new Dictionary<string, int>
            {
                { "Happy",   0 },
                { "Sad",     0 },
                { "Neutral", 0 },
                { "Anxious", 0 }
            };

            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT Mood, COUNT(*) 
                       FROM DiaryEntries
                       WHERE UserID = :userid
                       AND EXTRACT(MONTH FROM EntryDate) = EXTRACT(MONTH FROM SYSDATE)
                       AND EXTRACT(YEAR FROM EntryDate)  = EXTRACT(YEAR FROM SYSDATE)
                       GROUP BY Mood";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string mood = reader.GetString(0);
                                int count = reader.GetInt32(1);

                                // update the count for this mood
                                if (summary.ContainsKey(mood))
                                    summary[mood] = count;
                            }
                        }
                    }
                }
            }

            

            catch (Exception ex)
            {
                Console.WriteLine(" Database error: " + ex.Message);
            }
            return summary;
        }

        //Select multiple rows from DB using stored procedures.
        public DiaryEntry GetEntryByID(int entryID)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    using (var cmd = new OracleCommand("GetEntryByID", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        
                        cmd.Parameters.Add("p_EntryID", OracleDbType.Int32).Value = entryID;

                        
                        var pUserID = cmd.Parameters.Add("p_UserID", OracleDbType.Int32);
                        pUserID.Direction = System.Data.ParameterDirection.Output;

                        var pTitle = cmd.Parameters.Add("p_Title", OracleDbType.Varchar2, 200);
                        pTitle.Direction = System.Data.ParameterDirection.Output;

                        var pMood = cmd.Parameters.Add("p_Mood", OracleDbType.Varchar2, 20);
                        pMood.Direction = System.Data.ParameterDirection.Output;

                        var pEntryDate = cmd.Parameters.Add("p_EntryDate", OracleDbType.Date);
                        pEntryDate.Direction = System.Data.ParameterDirection.Output;

                        cmd.ExecuteNonQuery(); 

                        // if Oracle returned null -> entry not found
                        if (pTitle.Value is DBNull || pTitle.Value == null)
                            return null;

                        return new DiaryEntry
                        {
                            EntryID = entryID,
                            UserID = Convert.ToInt32(pUserID.Value.ToString()),
                            Title = pTitle.Value.ToString(),
                            Mood = pMood.Value.ToString(),
                            EntryDate = Convert.ToDateTime(pEntryDate.Value.ToString())
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(" GetEntryByID not found error: " + ex.Message);
                return null;
            }
        }

        //(Disconnected Update)
        public bool SaveChanges(int entryID, string newTitle, string newMood)
        {
            try
            {
                using (var conn = DbHelper.GetConnection())
                {
                    
                    string sql = "SELECT * FROM DiaryEntries WHERE EntryID = :id";
                    using (var adapter = new OracleDataAdapter(sql, conn))
                    {
                        adapter.SelectCommand.Parameters.Add(":id", OracleDbType.Int32).Value = entryID;

                        
                        using (var builder = new OracleCommandBuilder(adapter))
                        {
                            DataSet ds = new DataSet();
                            adapter.Fill(ds, "TargetRow");

                            if (ds.Tables["TargetRow"].Rows.Count > 0)
                            {
                                var row = ds.Tables["TargetRow"].Rows[0];
                                row["Title"] = newTitle;
                                row["Mood"] = newMood;

                                
                                adapter.Update(ds, "TargetRow");
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update Error: " + ex.Message);
            }
            return false;
        }
    }
}
