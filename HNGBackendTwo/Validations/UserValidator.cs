using FluentValidation;
using HNGBackendTwo.Models;

namespace HNGBackendTwo.Validations
{
    public class UserValidator : AbstractValidator<UserModel>
    {
        public UserValidator()
        {
            RuleFor(u => u.FirstName).NotEmpty();
            RuleFor(u => u.LastName).NotEmpty();
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(u => u.Password).NotEmpty();
        }
    }

    public class OrganisationValidator : AbstractValidator<OrganisationModel>
    {
        public OrganisationValidator()
        {
            RuleFor(o => o.Name).NotEmpty();
        }
    }

}
