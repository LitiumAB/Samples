using System;
using Litium.Web.Administration.Filtering;
using Litium.Studio.Extenssions;
using Litium.Runtime.AutoMapper;
using AutoMapper;
using Litium.Web.Customers.TargetGroups;
using Litium.Runtime;
using Litium.Events;
using Litium.Security.Events;

namespace Litium.Accelerator.TargetGroupconditions.LoginStatus
{
    /// <summary>
    /// Setup class for Login status target group condition.
    /// Setup is triggered on application start and is used to register Automapper-mapping required by the condition
    /// and to register target group event to trigger every time a user
    /// </summary>
    [Autostart]
    public class LoginStatusSetup : IAutoMapperConfiguration
    {
        public LoginStatusSetup(TargetGroupEngine targetGroupEngine, EventBroker eventBroker)
        {
            // Setup a new targetgroup event to fire every time a user logs in or out, so that we can modify the 
            // target group membership that depend on login status.

            eventBroker.Subscribe<PersonSignedIn>(e =>
            {
                targetGroupEngine.Process(new LoginLogoutTargetGroupEvent(true));
            });
            eventBroker.Subscribe<PersonSignedOut>(e =>
            {
                targetGroupEngine.Process(new LoginLogoutTargetGroupEvent(false));
            });
        }

        private string GetTitle(string value)
        {
            var stringKey = value switch
            {
                LoginStatusFilterService.ValueAnonymous => LoginStatusFilterService.FieldTitleIncludeAnonymousResx,
                LoginStatusFilterService.ValueLoggedIn => LoginStatusFilterService.FieldTitleIncludeLoggedinResx,
                _ => throw new Exception("Value not known" + value),
            };

            return $"{LoginStatusFilterService.FieldTitleResx.AsAngularResourceString()}: {stringKey.AsAngularResourceString()}";
        }

        void IAutoMapperConfiguration.Configure(IMapperConfigurationExpression cfg)
        {
            // The mapper is called when a saved value is fetched from DB to create the items showing in the list of already selected items in the filter

            cfg.CreateMap<LoginStatusFilterCondition, TargetGroupFilterConditionModel>()
                .ForMember(x => x.Field, m => m.MapFrom(x => LoginStatusFilterService.FieldId))
                .ForMember(x => x.Operator, m => m.MapFrom(x => x.Operator))
                .ForMember(x => x.ValueTitle, m => m.MapFrom(x => GetTitle(x.Value)))
                .ForMember(x => x.Value, m => m.MapFrom(x => x.Value));
        }
    }
}

