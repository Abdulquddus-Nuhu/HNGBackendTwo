namespace HNGBackendTwo.Models
{
    public class OrganisationUser
    {
        public string OrganisationId { get; set; }
        public Organisation Organisation { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
