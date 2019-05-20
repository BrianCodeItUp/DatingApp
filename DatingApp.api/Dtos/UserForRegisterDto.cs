using System.ComponentModel.DataAnnotations;

namespace DatingApp.api.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(8, ErrorMessage = "You must specify password between 4 and 8 charaters")]
        public string Password { get; set; }
    }
}