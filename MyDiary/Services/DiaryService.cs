using System;
using System.Collections.Generic;
using MyDiary.Data;
using MyDiary.Models;

namespace MyDiary.Services
{
    internal class DiaryService
    {
        private readonly DiaryRepository _repo = new DiaryRepository();

        public bool CreateEntry(int userID, string title, string body, string mood)
        {
            // validate mood input
            string[] validMoods = { "Happy", "Sad", "Neutral", "Anxious" };
            bool moodValid = Array.Exists(validMoods, m => m.Equals(mood, StringComparison.OrdinalIgnoreCase));

            if (!moodValid)
                return false;

            var entry = new DiaryEntry
            {
                UserID = userID,
                Title = title,
                Body = body,
                Mood = mood,
                EntryDate = DateTime.Today
            };

            return _repo.CreateEntry(entry);
        }

        public List<DiaryEntry> GetAllEntries(int userID)
        {
            return _repo.GetAllEntries(userID);
        }

        public bool UpdateEntry(int userID, int entryID, string newTitle, string newBody, string newMood)
        {
            string[] validMoods = { "Happy", "Sad", "Neutral", "Anxious" };
            bool moodValid = Array.Exists(validMoods, m => m.Equals(newMood, StringComparison.OrdinalIgnoreCase));

            if (!moodValid)
            {
                Console.WriteLine(" Invalid mood. Choose: Happy / Sad / Neutral / Anxious");
                return false;
            }

            var entry = new DiaryEntry
            {
                EntryID = entryID,
                UserID = userID,
                Title = newTitle,
                Body = newBody,
                Mood = newMood
            };

            return _repo.UpdateEntry(entry);
        }

        public bool DeleteEntry(int userID, int entryID)
        {
            return _repo.DeleteEntry(entryID, userID);
        }

        public List<DiaryEntry> Search(int userID, string keyword, string startDateStr, string endDateStr)
        {
            // if empty or invalid, treat as null
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(startDateStr))
                if (DateTime.TryParseExact(startDateStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime sd))
                    startDate = sd;

            if (!string.IsNullOrEmpty(endDateStr))
                if (DateTime.TryParseExact(endDateStr, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ed))
                    endDate = ed;

            return _repo.Search(userID, keyword, startDate, endDate);
        }

        public Dictionary<string, int> GetMoodSummary(int userID)
        {
            return _repo.GetMoodSummary(userID);
        }

        public DiaryEntry GetEntryByID(int entryID)
        {
            return _repo.GetEntryByID(entryID);
        }

        public bool UpdateDisconnected(int id, string title, string mood)
        {
            return _repo.SaveChanges(id, title, mood);
        }
    }
}
