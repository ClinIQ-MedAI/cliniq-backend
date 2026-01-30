using Microsoft.AspNetCore.Authorization;

namespace Clinic.Authentication.Authorization;

public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission);
