using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Models
{
    internal class DiaryEntry
    {
        public int EntryID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Mood { get; set; }   // Happy / Sad / Neutral / Anxious
        public DateTime EntryDate { get; set; }
    }
}
