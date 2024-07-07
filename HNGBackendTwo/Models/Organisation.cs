using System.ComponentModel.DataAnnotations;

namespace HNGBackendTwo.Models
{
    public class Organisation
    {
        [Key]
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<OrganisationUser> OrganisationUsers { get; set; }

        public Organisation()
        {
            OrgId = Guid.NewGuid().ToString();
        }

    }
}
