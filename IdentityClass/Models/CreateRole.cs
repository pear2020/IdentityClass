using System.ComponentModel.DataAnnotations;

namespace IdentityClass.Models
{
    public class CreateRole
    {
        [Required]
        public string RoleName { get; set; }
    }
}
