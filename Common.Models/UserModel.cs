using System;

namespace TaskManager.Common.Models
{
    /// <summary>
    /// Небезопасная модель пользователя для использования на бэк-енд. Содержит пароль
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Номер телефона пользователя
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Дата регистрации пользователя
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Дата последней авторизации пользователя
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Статус пользователя
        /// </summary>
        public UserStatus Status { get; set; }

        public UserModel() { }

        public UserModel(string firstname, string lastname, string email, string password, string phone, UserStatus status, int id)
        {
            Id = id;
            FirstName = firstname;
            LastName = lastname;
            Email = email;
            Password = password;
            Phone = phone;
            RegistrationDate = DateTime.Now;
            Status = status;
        }
    }
}
