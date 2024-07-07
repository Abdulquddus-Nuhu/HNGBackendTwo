using System.ComponentModel.DataAnnotations;

namespace HNGBackendTwo.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public ICollection<OrganisationUser> OrganisationUsers { get; set; }


        public User()
        {
            UserId = Guid.NewGuid().ToString();
        }
    }
}
