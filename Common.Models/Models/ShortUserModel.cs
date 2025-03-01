﻿using System;

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
        public string Surname { get; set; }

        public string Nickname { get; set; }

        public string Description { get; set; }

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

        public ShortUserModel(string firstname, string lastname, string email, string phone, UserStatus status, int id)
        {
            Id = id;
            FirstName = firstname;
            Surname = lastname;
            Email = email;
            Phone = phone;
            RegistrationDate = DateTime.Now;
            Status = status;
        }

        public ShortUserModel(UserModel model) 
        {
            Id = model.Id;
            FirstName = model.FirstName;
            Surname = model.Surname;
            Nickname = model.Nickname;
            Description = model.Description;
            Photo = model.Photo;
            Email = model.Email;
            Phone = model.Phone;
            RegistrationDate = model.RegistrationDate;
            Status = model.Status;
        }
    }
}
