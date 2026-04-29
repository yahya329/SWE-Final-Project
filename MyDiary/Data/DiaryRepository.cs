using System;
using System.Collections.Generic;
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

        public List<DiaryEntry> GetAllEntries(int userID)
        {
            try
            {
                var entries = new List<DiaryEntry>();

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT EntryID, Title, Mood, EntryDate 
                           FROM DiaryEntries 
                           WHERE UserID = :userid 
                           ORDER BY EntryDate DESC";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;

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

        // FR-04: Search entries by keyword and/or date range
        public List<DiaryEntry> Search(int userID, string keyword, DateTime? startDate, DateTime? endDate)
        {
            var entries = new List<DiaryEntry>();
            try
            {

                using (var conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT EntryID, Title, Mood, EntryDate 
                       FROM DiaryEntries 
                       WHERE UserID = :userid";

                    // add keyword filter only if user typed something
                    if (!string.IsNullOrEmpty(keyword))
                        sql += " AND (UPPER(Title) LIKE UPPER(:keyword) OR UPPER(Body) LIKE UPPER(:keyword))";

                    // add date filter only if user entered dates
                    if (startDate.HasValue && endDate.HasValue)
                        sql += " AND EntryDate BETWEEN :startdate AND :enddate";

                    sql += " ORDER BY EntryDate DESC";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        // always add userid
                        cmd.Parameters.Add(":userid", OracleDbType.Int32).Value = userID;

                        // only add keyword param if it was used
                        if (!string.IsNullOrEmpty(keyword))
                            cmd.Parameters.Add(":keyword", OracleDbType.Varchar2).Value = "%" + keyword + "%";

                        // only add date params if they were used
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            cmd.Parameters.Add(":startdate", OracleDbType.Date).Value = startDate.Value;
                            cmd.Parameters.Add(":enddate", OracleDbType.Date).Value = endDate.Value;
                        }

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
                Console.WriteLine(ex.Message);
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
                Console.WriteLine("❌ Database error: " + ex.Message);
            }
            return summary;
        }


    }
}
