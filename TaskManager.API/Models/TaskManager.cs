
namespace TaskManager.API.Models
{
    public class TaskManager : Default
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte[] File { get; set; } = Array.Empty<byte>();
        public int DeskId { get; set; }
        public required Desk Desk { get; set; }
        public int Column { get; set; }
        public int? CreatorId { get; set; }
        public required User Creator { get; set; }
        public int? ExecutorId { get; set; }
    }
}
