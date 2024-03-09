using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskManager.API.Models
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        const string KEY = "SHA256 Mein super geheimer Schlüssel";
        public const int LIFETIME = 2;

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
