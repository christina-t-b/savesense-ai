using SaveSenseAI.Application.Health.Queries.GetPing;

namespace SaveSenseAI.Application.UnitTests.Health.Queries.GetPing;

public class GetPingQueryValidatorTests
{
    private readonly GetPingQueryValidator _validator = new();

    [Fact]
    public void Validate_NameWithinLimit_IsValid()
    {
        var result = _validator.Validate(new GetPingQuery("Christina"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NameOver100Characters_IsInvalid()
    {
        var tooLong = new string('a', 101);

        var result = _validator.Validate(new GetPingQuery(tooLong));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetPingQuery.Name));
    }
}
