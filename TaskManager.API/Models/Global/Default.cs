namespace TaskManager.API.Models
{
    public class Default
    {
        /// <summary>
        /// Имя проекта или пользователя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание проекта или пользователя
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата создания проекта или дата регистрации пользователя
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Аватарка проекта или пользователя
        /// </summary>
        public byte[] Avatar { get; set; }

        public Default() 
        { 
            this.Name = "default";
            this.Avatar = new byte[0];
            this.CreationDate = DateTime.Now;
            this.Description = "description";
        }
    }
}
