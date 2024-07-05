using System.ComponentModel.DataAnnotations;

namespace HNGBackendTwo.Models
{
    public class OrganisationModel
    {
        [Key]
        public string OrgId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<UserModel> Users { get; set; }

        public OrganisationModel()
        {
            OrgId = Guid.NewGuid().ToString();
        }

    }
}
