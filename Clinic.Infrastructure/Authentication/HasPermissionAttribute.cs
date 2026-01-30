using Microsoft.AspNetCore.Authorization;

namespace Clinic.Infrastructure.Authentication;

public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission);
