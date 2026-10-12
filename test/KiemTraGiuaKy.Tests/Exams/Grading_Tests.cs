using KiemTraGiuaKy.Exams.Services;
using Shouldly;
using Xunit;

namespace KiemTraGiuaKy.Tests.Exams;

public class Grading_Tests
{
    private readonly IToeicScoringService _scoringService;

    public Grading_Tests()
    {
        _scoringService = new ToeicScoringService();
    }

    [Fact]
    public void CalculateReadingScore_ShouldReturnMinimumScore_WhenZeroCorrect()
    {
        // Arrange & Act
        int score = _scoringService.CalculateReadingScore(0);

        // Assert
        score.ShouldBe(5);
    }

    [Fact]
    public void CalculateReadingScore_ShouldReturnMaximumScore_When100Correct()
    {
        // Arrange & Act
        int score = _scoringService.CalculateReadingScore(100);

        // Assert
        score.ShouldBe(495);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(25, 85)]
    [InlineData(50, 210)]
    [InlineData(75, 335)]
    [InlineData(90, 415)]
    [InlineData(99, 495)]
    public void CalculateReadingScore_ShouldReturnExpectedScaledScores(int correctCount, int expectedScore)
    {
        // Act
        int score = _scoringService.CalculateReadingScore(correctCount);

        // Assert
        score.ShouldBe(expectedScore);
    }

    [Fact]
    public void CalculateReadingScore_ShouldHandleNegativeInputsGracefully()
    {
        // Act
        int score = _scoringService.CalculateReadingScore(-5);

        // Assert
        score.ShouldBe(5);
    }

    [Fact]
    public void CalculateReadingScore_ShouldCapAtMaximumScore_WhenExceeding100()
    {
        // Act
        int score = _scoringService.CalculateReadingScore(105);

        // Assert
        score.ShouldBe(495);
    }
}
