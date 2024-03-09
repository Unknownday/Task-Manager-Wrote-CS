namespace TaskManager.API.Models
{
    public class Desk : Default
    {
        public int Id { get; set; }
        public bool IsPublic { get; set; }
        public string Columns {  get; set; }
        public User Administrator { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }

    }

}
