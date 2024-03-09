using System;
using TaskManager.Common.Models;

namespace Common.Models
{
    /// <summary>
    /// "Безопасная" модель пользователя которая возращается на фронт-енд. Не содержит пароля
    /// </summary>
    public class ShortUserModel
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

        public ShortUserModel(string firstname, string lastname, string email, string phone, UserStatus status, int id)
        {
            Id = id;
            FirstName = firstname;
            LastName = lastname;
            Email = email;
            Phone = phone;
            RegistrationDate = DateTime.Now;
            Status = status;
        }
    }
}
