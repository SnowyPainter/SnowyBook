using Harbor.Cargo;
using Microsoft.AspNetCore.Mvc;
using SnowyBook.Models;

namespace SnowyBook.Controllers
{
    public class HomeController : Controller
    {
        private static int changes = 0;
        private static string currDoc = "";
        private static string currContent = "";
        [HttpGet]
        public void Pullaway() {
            Program.ReviseLog.Lock();
            Program.Localship.LoadPublic(Program.ReviseLog);
            Program.Localship.PullAwayPublicData();
            Program.ReviseLog.UnLock();
            Program.ReviseLog = new DataCargo();
        }
        [HttpPost]
        public int Save(string doc, [Bind("content")] string content) {
            var temp = changes;
            if(currDoc != doc) {
                Program.ReviseLog.Load(new Data {Content = $"Changed {currDoc} {doc}"});
                currDoc = doc;
                changes++;
            }
            if(currContent != content) {
                Program.ReviseLog.Load(new Data {Content = $"Changed {currContent} {content}"});
                currContent = content;
                changes++;
            }
            if(temp <= changes)
                System.IO.File.WriteAllText(@$"{Program.DefaultSavePath}/{doc}.md", content);
            return changes;
        }
        public IActionResult Edit(string doc)
        {
            string? str = "";
            if (doc == null)
            {
                doc = "New Document";
                ViewData["isNew"] = true;
            }
            else
            {
                var path = $@"{Program.DefaultSavePath}/{doc}";
                str = Program.Localship.OpenFile(path);
                if (str == null) {
                    str = "";
                    ViewData["isNew"] = true;
                }
                else
                    ViewData["isNew"] = false;
            }
            NoteModel note = new NoteModel();
            note.Title = doc;
            note.Content = str.Trim();
            currDoc = note.Title.Split('.')[0];
            currContent = note.Content;
            // Participants 는 실시간으로 저장됨. 실시간 유입이기 때문이다.

            //Test
            for (int i = 0; i < 4; i++)
                note.Participants.Add(new Participant { Id = $"User{i}" });

            return View(note);
        }
        private NoteModel? saveToNoteModel(string fullPath)
        {
            var content = Program.Localship.OpenFile(fullPath);
            if (content == null) return null;
            var filename = Path.GetFileName(fullPath);
            return new NoteModel
            {
                Content = content,
                Title = filename
            };
        }
        public IActionResult Index()
        {
            ViewData["Title"] = "Home";
            LatelyNotesModel latelyNotesModel = new LatelyNotesModel();

            foreach (var p in Directory.GetFiles(Program.DefaultSavePath, "*.txt"))
            {
                var n = saveToNoteModel(p);
                if (n == null) continue;
                latelyNotesModel.Notes.Add(n);
            }
            foreach (var p in Directory.GetFiles(Program.DefaultSavePath, "*.md"))
            {
                var n = saveToNoteModel(p);
                if (n == null) continue;
                latelyNotesModel.Notes.Add(n);
            }

            return View(latelyNotesModel);
        }
    }
}