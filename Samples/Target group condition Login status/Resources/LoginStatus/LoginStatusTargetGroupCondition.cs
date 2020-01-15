using Litium.Foundation.Security;
using Litium.Web.Customers.TargetGroups;
using Litium.Web.Customers.TargetGroups.Events;
using System.Collections.Concurrent;

namespace Litium.Accelerator.TargetGroupconditions.LoginStatus
{
    /// <summary>
    /// The TargetGroupCondition is responsible for parsing the condition when a user visits the page.
    /// It contains the logic to check if the current user matches the defined target group condition.
    /// </summary>
    public class LoginStatusTargetGroupCondition : ITargetGroupCondition<LoginStatusFilterCondition>
    {
        private LoginStatusFilterCondition _data;

        /// <summary>
        /// If set to true the condition support both Add and Remove as actions so that a user can be removed 
        /// from a condition where previously matched.
        /// </summary>
        public bool AllowsMultipleActions => true;

        /// <summary>
        /// Called to check which action that should be executed if the process action returns True.
        /// This method is called AFTER the call to Process and is only called if a change is to be made (if Proces() returns true)
        /// </summary>
        public TargetGroupProcessAction GetProcessAction(ConcurrentDictionary<string, object> context, ITargetGroupEvent targetGroupEvent)
        {
            var result = TargetGroupProcessAction.Add;
            bool isLoggedIn = IsUserLoggedIn(targetGroupEvent);
            if (_data.Value == LoginStatusFilterService.ValueAnonymous)
            {
                // User should be member of anonymous condition on only if not logged in, otherwise removed
                result = isLoggedIn ? TargetGroupProcessAction.Remove : TargetGroupProcessAction.Add;
            }
            else if (_data.Value == LoginStatusFilterService.ValueLoggedIn)
            {
                // User should be member of logged in condition only if logged in, otherwise removed
                result = isLoggedIn ? TargetGroupProcessAction.Add : TargetGroupProcessAction.Remove;
            }
            this.Log().Debug($"LoginStatusTargetGroupCondition.GetProcessAction {_data.Value}={result}");
            return result;
        }

        /// <summary>
        /// Called on application startup
        /// </summary>
        public void Initialize(LoginStatusFilterCondition data)
        {
            _data = data;
        }

        /// <summary>
        /// Called once for each ITargetGroupEvent that is fired
        /// Cast the parameter and check its type to only listen to specific events 
        /// 
        /// If Process() returns true then membership of the target group is updated with Add or Remove 
        /// depending on what is returned from the GetProcessAction-method.
        /// </summary>
        public bool Process(ConcurrentDictionary<string, object> context, ITargetGroupEvent targetGroupEvent)
        {
            return true;
        }

        private bool IsUserLoggedIn(ITargetGroupEvent tgEvent)
        {
            // Check if the user is currently logged in or not
            var loginLogoutEvent = tgEvent as LoginLogoutTargetGroupEvent;
            bool isLoggedIn = loginLogoutEvent != null ? loginLogoutEvent.IsLogin : !SecurityToken.CurrentSecurityToken.IsAnonymousUser;
            return isLoggedIn;
        }
    }
}

