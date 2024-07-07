using System.ComponentModel.DataAnnotations;

namespace HNGBackendTwo.Dtos
{
    public class OrganisationDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
