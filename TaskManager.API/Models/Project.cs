using System.Collections.ObjectModel;

namespace TaskManager.API.Models
{
    public class Project : Default
    {
        /// <summary>
        /// Уникальный идентификатор проекта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Унникальный идентификатор администратора данного проекта
        /// </summary>
        public int AdminId { get; set; }

        /// <summary>
        /// Модель администратора данного проекта
        /// </summary>
        public required ProjectAdmin Admin { get; set; }

        /// <summary>
        /// Список всех пользователей участвующих в данном проекте
        /// </summary>
        public required ObservableCollection<User> AllUsers { get; set; }

        /// <summary>
        /// Список всех задач привязанных к данному проекту
        /// </summary>
        public required ObservableCollection<Desk> AllDesks { get; set; }

        /// <summary>
        /// Статус проекта
        /// </summary>
        public ProjectStatus Status { get; set; }
    }
}
