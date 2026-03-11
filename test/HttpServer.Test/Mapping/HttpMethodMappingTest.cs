using Xunit;

namespace Transport.Test.Mapping
{
    public class HttpMethodMappingTest
    {
        [Fact]
        public void Sanity_Test_Framowork_Works()
        {
            // Arrange
            var expected = 2 + 2;

            // Actual
            var actual = 4;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
