using Core.Helpers;
using FluentAssertions;
using Xunit;

namespace gpconnect_analytics.Test
{
    public class DateTimeHelperTests
    {
        [Fact]
        public void EachDay_ShouldReturnAllDaysBetweenTwoDates_Inclusive()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 5);

            // Act
            var result = DateTimeHelper.EachDay(fromDate, toDate).ToList();

            // Assert
            result.Should().HaveCount(5);
            result.Should().ContainInOrder(
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                new DateTime(2025, 1, 3),
                new DateTime(2025, 1, 4),
                new DateTime(2025, 1, 5)
            );
        }

        [Fact]
        public void EachDay_ShouldReturnSingleDay_WhenFromAndToDatesAreSame()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 1);
            var toDate = new DateTime(2025, 1, 1);

            // Act
            var result = DateTimeHelper.EachDay(fromDate, toDate).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.First().Should().Be(fromDate);
        }

        [Fact]
        public void EachDay_ShouldReturnEmpty_WhenFromDateIsAfterToDate()
        {
            // Arrange
            var fromDate = new DateTime(2025, 1, 5);
            var toDate = new DateTime(2025, 1, 1);

            // Act
            var result = DateTimeHelper.EachDay(fromDate, toDate).ToList();

            // Assert
            result.Should().BeEmpty();
        }
    }
}