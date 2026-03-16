using Parking.Domain.Enums;

namespace Parking.Domain.Exceptions;

public class InvalidSessionStateException : DomainException
{
    public InvalidSessionStateException(ParkingSessionStatus current, ParkingSessionStatus attempted)
        : base($"Cannot transition parking session from '{current}' to '{attempted}'.") { }
}
