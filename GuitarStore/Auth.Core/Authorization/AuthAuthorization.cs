namespace Auth.Core.Authorization;

public static class AuthRoles
{
    public const string User = "user";
    public const string Support = "support";
    public const string Admin = "admin";

    public static readonly string[] All =
    [
        User,
        Support,
        Admin
    ];
}

public static class AuthPermissions
{
    public const string CatalogManage = "Catalog.Manage";
    public const string OrdersViewAny = "Orders.ViewAny";
    public const string OrdersCancelAny = "Orders.CancelAny";
    public const string CustomersViewAny = "Customers.ViewAny";
}

public static class AuthPolicies
{
    public const string CatalogManage = AuthPermissions.CatalogManage;
    public const string OrdersViewAny = AuthPermissions.OrdersViewAny;
    public const string OrdersCancelAny = AuthPermissions.OrdersCancelAny;
    public const string CustomersViewAny = AuthPermissions.CustomersViewAny;
}

public static class AuthClaimTypes
{
    public const string Permission = "permission";
}

public static class AuthAuthenticationSchemes
{
    public const string IdentityApplication = "Identity.Application";
}

public static class AuthRolePermissions
{
    private static readonly IReadOnlyDictionary<string, string[]> RolePermissions =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            [AuthRoles.User] = [],
            [AuthRoles.Support] =
            [
                AuthPermissions.OrdersViewAny,
                AuthPermissions.OrdersCancelAny,
                AuthPermissions.CustomersViewAny
            ],
            [AuthRoles.Admin] =
            [
                AuthPermissions.CatalogManage,
                AuthPermissions.OrdersViewAny,
                AuthPermissions.OrdersCancelAny,
                AuthPermissions.CustomersViewAny
            ]
        };

    public static IReadOnlyCollection<string> GetPermissions(string roleName)
    {
        if (!RolePermissions.TryGetValue(roleName, out var permissions))
        {
            throw new InvalidOperationException($"Unknown auth role '{roleName}'.");
        }

        return permissions;
    }
}
