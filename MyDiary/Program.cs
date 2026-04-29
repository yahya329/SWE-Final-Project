using System;

using MyDiary.Models;
using MyDiary.Services;


namespace MyDiary
{
    internal static class Program
    {
        ///// <summary>
        ///// The main entry point for the application.
        ///// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1());
        //}

        static User currentUser = null; // session

        static void Main(string[] args)

        {

            //check database connection 
            try
            {
                var conn = MyDiary.Data.DbHelper.GetConnection();
                conn.Open();
                Console.WriteLine(" Connected to Oracle successfully!");
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Connection failed: " + ex.Message);
            }

            Console.WriteLine("=== Personal Diary System ===");

            while (currentUser == null)
            {
                Console.WriteLine("\n1. Login");
                Console.WriteLine("2. Register");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                if (choice == "1") Login();
                else if (choice == "2") Register();
                else Console.WriteLine("Invalid option.");
            }

            
            DiaryMenu();
        }

        static void Login()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            var auth = new AuthService();
            User user = auth.Login(username, password);

            if (user != null)
            {
                currentUser = user;
                Console.WriteLine($"\nLogin successful. Welcome, {currentUser.Username}!");
            }

            else
                Console.WriteLine("Invalid credentials. Try again.");
        }

        static void Register()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            var auth = new AuthService();
            bool success = auth.Register(username, password);

            if (success)
                Console.WriteLine("Registered successfully! Please login.");
            else
                Console.WriteLine("Username already taken. Try another.");
        }

        static void ShowAllEntries(DiaryService diaryService)
        {
            var entries = diaryService.GetAllEntries(currentUser.UserID);

            if (entries.Count == 0)
            {
                Console.WriteLine("\nNo entries found. Start writing!");
                return;
            }

            Console.WriteLine("\n=== Your Diary Entries ===");
            int i = 1;
            foreach (var entry in entries)
            {
                Console.WriteLine($"{i}. [{entry.EntryDate:dd-MMM-yyyy}] {entry.Title} | {entry.Mood}");
                i++;
            }
        }

        static void DiaryMenu()
        {
            var diaryService = new DiaryService();

            ShowAllEntries(diaryService);

            while (true)
            {
                Console.WriteLine($"\n=== Welcome, {currentUser.Username} ===");
                Console.WriteLine("1. Write new entry");
                //Console.WriteLine("2. View all entries");
                Console.WriteLine("2. Search entries");
                Console.WriteLine("3. Mood summary");
                Console.WriteLine("4. Update entry");  
                Console.WriteLine("5. Delete entry");  
                Console.WriteLine("0. Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                if (choice == "1") DoCreateEntry(diaryService);
                //else if (choice == "2") ShowAllEntries(diaryService);
                else if (choice == "2") DoSearch(diaryService);
                else if (choice == "3") ShowMoodSummary(diaryService);
                else if (choice == "4") DoUpdateEntry(diaryService);
                else if (choice == "5") DoDeleteEntry(diaryService);
                else if (choice == "0") break;
                else Console.WriteLine("Invalid option.");
            }
        }

        static void DoCreateEntry(DiaryService diaryService)
        {
            Console.Write("Title: ");
            string title = Console.ReadLine();

            Console.Write("Body: ");
            string body = Console.ReadLine();

            Console.WriteLine("Mood (Happy / Sad / Neutral / Anxious): ");
            string mood = Console.ReadLine();

            bool success = diaryService.CreateEntry(currentUser.UserID, title, body, mood);

            if (success)
                Console.WriteLine(" Entry saved successfully!");
            else
                Console.WriteLine(" Failed. Check your mood value and try again.");
        }

        static void DoUpdateEntry(DiaryService diaryService)
        {
            // first show all entries so user knows the IDs
            ShowAllEntries(diaryService);

            Console.Write("\nEnter Entry ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int entryID))
            {
                Console.WriteLine(" Invalid ID.");
                return;
            }

            Console.Write("New Title: ");
            string title = Console.ReadLine();

            Console.Write("New Body: ");
            string body = Console.ReadLine();

            Console.Write("New Mood (Happy / Sad / Neutral / Anxious): ");
            string mood = Console.ReadLine();

            bool success = diaryService.UpdateEntry(currentUser.UserID, entryID, title, body, mood);

            if (success)
                Console.WriteLine(" Entry updated successfully!");
            else
                Console.WriteLine(" Entry not found ");
        }

        static void DoDeleteEntry(DiaryService diaryService)
        {
            // first show all entries so user knows the IDs
            ShowAllEntries(diaryService);

            Console.Write("\nEnter Entry ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int entryID))
            {
                Console.WriteLine("❌ Invalid ID.");
                return;
            }

            Console.Write("Are you sure? (yes / no): ");
            string confirm = Console.ReadLine();

            if (confirm?.ToLower() != "yes")
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            bool success = diaryService.DeleteEntry(currentUser.UserID, entryID);

            if (success)
                Console.WriteLine(" Entry deleted successfully!");
            else
                Console.WriteLine(" Entry not found ");
        }

        static void DoSearch(DiaryService diaryService)
        {
            Console.WriteLine("\n=== Search Entries ===");

            Console.Write("Keyword (press Enter to skip): ");
            string keyword = Console.ReadLine();

            Console.Write("Start date dd-MM-yyyy (press Enter to skip): ");
            string startDate = Console.ReadLine();

            Console.Write("End date dd-MM-yyyy (press Enter to skip): ");
            string endDate = Console.ReadLine();

            var results = diaryService.Search(currentUser.UserID, keyword, startDate, endDate);

            if (results.Count == 0)
            {
                Console.WriteLine("No entries matched your search.");
                return;
            }

            Console.WriteLine($"\n{results.Count} result(s) found:");
            int i = 1;
            foreach (var entry in results)
            {
                Console.WriteLine($"{i}. [{entry.EntryDate:dd-MMM-yyyy}] {entry.Title} | {entry.Mood}");
                i++;
            }
        }

        static void ShowMoodSummary(DiaryService diaryService)
        {
            var summary = diaryService.GetMoodSummary(currentUser.UserID);

            Console.WriteLine($"\n=== Mood Summary ({DateTime.Now:MMMM yyyy}) ===");

            foreach (var mood in summary)
                Console.WriteLine($"{mood.Key,-10} : {mood.Value}");
        }
    }
}
