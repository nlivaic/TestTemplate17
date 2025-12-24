using System;
using System.Linq;
using System.Security.Claims;

namespace TestTemplate17.Api.Helpers;

public static class IdentityExtensions
{
    public static string Username(this ClaimsPrincipal user) =>
        user.Claims.SingleOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

    public static Guid? UserId(this ClaimsPrincipal user)
    {
        var subClaim = user.Claims.SingleOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");
        return subClaim == null
            ? null
            : new Guid(subClaim.Value);
    }
}
