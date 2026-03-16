namespace Parking.Api.Auth;

public static class ParkingPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrTechnician = "AdminOrTechnician";
    public const string AdminOrFinance = "AdminOrFinance";
    public const string AnyStaff = "AnyStaff";
}
