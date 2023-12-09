using System.ComponentModel.DataAnnotations;

namespace VVS_biblioteka.Models
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Type of user is required")]
        [EnumDataType(typeof(UserType), ErrorMessage = "Invalid user type")]
        public UserType userType { get; set; }
    }

    public enum UserType
    {
        student,
        ucenik,
        penzioner,
        dijete
    }
}
}