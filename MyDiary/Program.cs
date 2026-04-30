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

            // Option menu 
            Console.WriteLine("\n--- Actions ---");
            Console.WriteLine("1. Edit ");
            Console.WriteLine("2. Delete ");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Choose: ");
            string action = Console.ReadLine();

            if (action == "1")
            {
                DoUpdateEntry(diaryService); 
            }
            else if (action == "2")
            {
                DoDeleteEntry(diaryService); 
            }
        }

        static void DiaryMenu()
        {
            var diaryService = new DiaryService();

            while (true)
            {
                Console.WriteLine($"\n=== Welcome, {currentUser.Username} ===");
                Console.WriteLine("1. Add new entry");
                Console.WriteLine("2. View My entries");
                Console.WriteLine("3. Search ");
                Console.WriteLine("4. Mood summary");
                Console.WriteLine("0. Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                if (choice == "1") DoCreateEntry(diaryService);
                else if (choice == "2") ShowAllEntries(diaryService);
                else if (choice == "3") DoSearch(diaryService);
                else if (choice == "4") ShowMoodSummary(diaryService);
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


            Console.Write("\nEnter Entry ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int entryID))
            {
                Console.WriteLine(" Invalid ID.");
                return;
            }

            // USE GetEntryByID SP here — fetch existing data first
            var existing = diaryService.GetEntryByID(entryID);
            if (existing == null)
            {
                Console.WriteLine(" Entry not found.");
                return;
            }

            //data u want to update, if user press enter, keep old value
            Console.WriteLine($"\n--- Current Entry ---");
            Console.WriteLine($"Title : {existing.Title}");
            Console.WriteLine($"Mood  : {existing.Mood}");
            Console.WriteLine($"Date  : {existing.EntryDate:dd-MMM-yyyy}");
            Console.WriteLine($"---------------------");

            
            Console.Write("New Title (Enter to keep current): ");
            string title = Console.ReadLine();
            if (string.IsNullOrEmpty(title)) title = existing.Title;

            Console.Write("New Body (Enter to keep current): ");
            string body = Console.ReadLine();
            if (string.IsNullOrEmpty(body)) body = existing.Body;

            Console.Write("New Mood (Enter to keep current): ");
            string mood = Console.ReadLine();
            if (string.IsNullOrEmpty(mood)) mood = existing.Mood;

            bool success = diaryService.UpdateEntry(currentUser.UserID, entryID, title, body, mood);

            if (success)
                Console.WriteLine(" Entry updated successfully!");
            else
                Console.WriteLine(" Update failed.");
        }

        static void DoDeleteEntry(DiaryService diaryService)
        {

            Console.Write("\nEnter Entry ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int entryID))
            {
                Console.WriteLine(" Invalid ID.");
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

            
            Console.WriteLine("\n--- Actions ---");
            Console.WriteLine("1. Edit an entry");
            Console.WriteLine("2. Delete an entry");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Choose: ");
            string action = Console.ReadLine();

            if (action == "1") 
            {
                Console.Write("Enter ID to Edit: ");
                int id = int.Parse(Console.ReadLine());

                // 1. Get new data from user
                Console.Write("New Title: ");
                string title = Console.ReadLine();
                Console.Write("New Mood: ");
                string mood = Console.ReadLine();

                // 2. Call the Disconnected Save method
                bool success = diaryService.UpdateDisconnected(id, title, mood);

                if (success) Console.WriteLine("Updated using Disconnected CommandBuilder!");
            }
            else if (action == "2")
            {
                DoDeleteEntry(diaryService); 
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
