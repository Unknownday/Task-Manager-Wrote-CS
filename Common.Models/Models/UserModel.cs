using System;

namespace Common.Models
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
        public string Surname { get; set; }

        public string Nickname { get; set; }

        public string Description { get; set; }

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

        public byte[] Photo { get; set; } = new byte[0];

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

        public UserModel(string firstName, string surname, string nickname, string description, string password, string email, string phone, byte[] photo, DateTime registrationDate, DateTime lastLoginDate, UserStatus status)
        {
            FirstName = firstName;
            Surname = surname;
            Nickname = nickname;
            Description = description;
            Password = password;
            Email = email;
            Phone = phone;
            Photo = photo;
            RegistrationDate = registrationDate;
            LastLoginDate = lastLoginDate;
            Status = status;
        }
    }
}
