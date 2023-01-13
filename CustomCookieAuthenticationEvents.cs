using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

namespace SingleUserCircuit;

public class CustomCookieAuthenticationEvents<TUser> : CookieAuthenticationEvents where TUser : class
{
    private readonly UserManager<TUser> _userManager;
    private readonly SignInManager<TUser> _signInManager;
    private readonly IdentityOptions _options;

    public CustomCookieAuthenticationEvents(SignInManager<TUser> signInManager, UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _options = optionsAccessor.Value;
    }

    public override Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!_userManager.SupportsUserSecurityStamp)
        {
            return base.ValidatePrincipal(context);
        }

        return ValidatePrincipalInternal(context);
    }

    async Task ValidatePrincipalInternal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;

        if (await _userManager.GetUserAsync(principal) is not TUser user)
        {
            context.RejectPrincipal();
            return;
        }

        var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
        var userStamp = await _userManager.GetSecurityStampAsync(user);
        if (principalStamp != userStamp)
        {
            context.RejectPrincipal();
            await _signInManager.SignOutAsync();
            return;
        }
    }
}
