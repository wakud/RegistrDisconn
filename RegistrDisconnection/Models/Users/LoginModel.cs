using System.ComponentModel.DataAnnotations;

namespace RegistrDisconnection.Models.Users
{
    /// <summary>
    /// авторизація користувача
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "Не введений логін")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не введений пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
