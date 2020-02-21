using System;
using System.Security.Claims;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;

namespace Litium.Accelerator.Auth0
{

    public class Auth0LoginService : IAuth0LoginService
    {
        private readonly IAuth0Configuration _configuration;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly PersonService _personService;
        private readonly SecurityContextService _securityContextService;

        public Auth0LoginService(PersonService personService, FieldTemplateService fieldTemplateService, SecurityContextService securityContextService, IAuth0Configuration configuration)
        {
            _personService = personService;
            _fieldTemplateService = fieldTemplateService;
            _securityContextService = securityContextService;
            _configuration = configuration;
        }

        public Person GetOrCreatePerson(ClaimsIdentity externalIdentity)
        {
            var email = externalIdentity.FindFirstValue(ClaimTypes.Email);
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

        public void SignOut(IOwinContext owinContext)
        {
            owinContext.Authentication.SignOut(_configuration.ProviderId);
        }

        private Person CreatePersonFromIdentity(ClaimsIdentity identity)
        {
            var email = identity.FindFirstValue(ClaimTypes.Email);

            var personTemplate = _configuration.PersonTemplate;
            var template = _fieldTemplateService.Get<PersonFieldTemplate>(typeof(CustomerArea), personTemplate);
            if (template == null)
                throw new Exception($"Person not created, person template '{personTemplate}' does not exist.");

            var person = new Person(template.SystemId) { Id = email, Email = email, LoginCredential = { Username = email } };

            var fullName = identity.FindFirstValue("name")?.Trim();

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
                person.FirstName = identity.FindFirstValue("nickname");
            }

            using (_securityContextService.ActAsSystem("Create person from Auth0 login"))
            {
                _personService.Create(person);
            }

            return person;
        }
    }
}