namespace Duotify.Membership.Web.Domain.ValueObjects;

public enum ChallengeStatus
{
    Active = 0,
    Locked = 1,
    Verified = 2,
    Expired = 3,
    Invalidated = 4
}