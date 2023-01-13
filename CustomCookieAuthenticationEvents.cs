using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace SingleUserCircuit;

public class CustomCookieAuthenticationEvents<TUser> : CookieAuthenticationEvents where TUser : class
{
    private readonly UserManager<TUser> _userManager;

    public CustomCookieAuthenticationEvents(UserManager<TUser> userManager)
    {
        _userManager = userManager;
    }

    public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!_userManager.SupportsUserSecurityStamp)
        {
            return base.ValidatePrincipal(context);
        }

        return SecurityStampValidator.ValidatePrincipalAsync(context);
    }
}
