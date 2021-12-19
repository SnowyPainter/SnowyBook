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
        public string Save(string doc, [Bind("content")] string content, [Bind("id")] string id) {
            var temp = changes;
            var changeMsg = "";
            if(currDoc != doc) {
                Program.ReviseLog.Load(new Data {Content = $"Changed {currDoc} {doc}"});
                changeMsg += $"[{id}] {currDoc} To {doc}\n";
                currDoc = doc;
                changes++;
            }
            if(currContent != content) {
                Program.ReviseLog.Load(new Data {Content = $"Changed {currContent} {content}"});
                changeMsg += $"[{id}] Contents {currContent.Length} to {content.Length}\n";
                currContent = content;
                changes++;
            }
            if(temp <= changes)
                System.IO.File.WriteAllText(@$"{Program.DefaultSavePath}/{doc}.md", content);
            return changeMsg;
        }
        public IActionResult Edit(string doc, string id)
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

            note.Participants.Add(new Participant { Id = id });
            ViewData["me"] = id;
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