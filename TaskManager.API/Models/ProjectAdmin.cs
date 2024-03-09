namespace TaskManager.API.Models
{
    public class ProjectAdmin
    {
        /// <summary>
        /// Уникальный идентификатор админстратора
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Уникальный идентификатор пользователя который является администратором
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Модель пользователя
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Проекты администрируемые данным пользователем
        /// </summary>
        public List<Project> Projects { get; set; } = new List<Project>();
        public ProjectAdmin(User user) 
        { 
            this.User = user;
        }
    }
}
