using Harbor.Cargo;

namespace SnowyBook.Models
{
    public class Participant
    {
        public string Id { get; set; } = "";
    }
    
    public class LatelyNotesModel
    {
        public List<NoteModel> Notes = new List<NoteModel>();
    }
}
