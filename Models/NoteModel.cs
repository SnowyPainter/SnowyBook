namespace SnowyBook.Models
{
    public class NoteModel
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public List<Participant> Participants = new List<Participant>();
    }
}
