using Litium.Web.Customers.TargetGroups.Events;
using System;

namespace Litium.Accelerator.TargetGroupconditions.LoginStatus
{
    /// <summary>
    /// Custom ITargetGroupEvent that is triggered every time the user logs in or out
    /// </summary>
    public class LoginLogoutTargetGroupEvent : ITargetGroupEvent
    {
        public bool IsLogin;

        public LoginLogoutTargetGroupEvent(bool isLogin)
        {
            IsLogin = isLogin;
        }
    }
}
