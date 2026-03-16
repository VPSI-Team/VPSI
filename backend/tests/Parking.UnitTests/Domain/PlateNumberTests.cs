using FluentAssertions;
using Parking.Domain.ValueObjects;

namespace Parking.UnitTests.Domain;

public sealed class PlateNumberTests
{
    [Theory]
    [InlineData("1AB2345", "1AB2345")]
    [InlineData(" 1ab2345 ", "1AB2345")]
    [InlineData("1 AB 2345", "1AB2345")]
    public void Create_WithValidInput_NormalizesValue(string input, string expected)
    {
        var plate = PlateNumber.Create(input);
        plate.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyInput_Throws(string input)
    {
        var act = () => PlateNumber.Create(input);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithTooLongInput_Throws()
    {
        var act = () => PlateNumber.Create("TOOLONGPLATE123");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoPlatesWithSameValue_AreEqual()
    {
        var a = PlateNumber.Create("1AB2345");
        var b = PlateNumber.Create("1ab2345");
        a.Should().Be(b);
    }
}
