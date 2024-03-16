using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskManager.API.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "Unknownday Task Manager";

        public const string AUDIENCE = "Unknownday Task Manager";

        const string KEY = "SHA256 Mein super geheimer Schlüssel";

        /// <summary>
        /// Получение симетричного ключа
        /// </summary>
        /// <returns>Новый симетричный ключ</returns>
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
