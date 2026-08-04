namespace SaveSenseAI.Application.Common.Interfaces;

public interface ICurrentUserService
{
    /// <summary>Null when there is no authenticated user (anonymous request).</summary>
    Guid? UserId { get; }
}
