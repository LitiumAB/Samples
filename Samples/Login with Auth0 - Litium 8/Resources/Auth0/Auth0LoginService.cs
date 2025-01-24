using IdentityModel;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Security;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Auth0
{
    public class Auth0LoginService : IAuth0LoginService
    {
        private readonly IOptions<Auth0Configuration> _configuration;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly PersonService _personService;
        private readonly SecurityContextService _securityContextService;

        public Auth0LoginService(
            PersonService personService,
            FieldTemplateService fieldTemplateService,
            SecurityContextService securityContextService,
            IOptions<Auth0Configuration> configuration)
        {
            _personService = personService;
            _fieldTemplateService = fieldTemplateService;
            _securityContextService = securityContextService;
            _configuration = configuration;
        }

        public Person GetOrCreatePerson(ClaimsPrincipal externalIdentity)
        {
            var email = externalIdentity.FindFirst(x => x.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception($"Required claim '{ClaimTypes.Email}' missing from identity");

            var person = _personService.Get(email);
            if (person != null)
                return person;

            person = CreatePersonFromIdentity(externalIdentity);
            if (person == null)
                throw new Exception("Could not create person from identity");

            return person;
        }

        private Person CreatePersonFromIdentity(ClaimsPrincipal identity)
        {
            var email = identity.FindFirst(x => x.Type == ClaimTypes.Email)?.Value;

            var personTemplate = _configuration.Value.PersonTemplate;
            var template = _fieldTemplateService.Get<FieldTemplate>(typeof(CustomerArea), personTemplate);
            if (template == null)
                throw new Exception($"Person not created, person template '{personTemplate}' does not exist.");

            var person = new Person(template.SystemId) { Id = email, Email = email, LoginCredential = { Username = email } };

            var fullName = identity.FindFirst(x => x.Type == JwtClaimTypes.Name)?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                // Check if the full name has a space separator, if so assume it is separating first and last name
                var hasLastName = fullName.Contains(" ");
                // Get value up to first space as first name
                person.FirstName = hasLastName ? fullName.Substring(0, fullName.IndexOf(" ", StringComparison.Ordinal)) : fullName;
                // Get value after first space as last name
                person.LastName = hasLastName ? fullName.Substring(fullName.IndexOf(" ", StringComparison.Ordinal)).Trim() : null;
            }
            else
            {
                person.FirstName = identity.FindFirst(x => x.Type == JwtClaimTypes.NickName)?.Value;
            }

            using (_securityContextService.ActAsSystem("Create person from Auth0 login"))
            {
                _personService.Create(person);
            }

            return person;
        }
    }
}
