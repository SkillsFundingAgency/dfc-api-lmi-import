using DFC.Api.Lmi.Import.UnitTests.TestModels;
using DFC.Api.Lmi.Import.Utilities;
using System;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Utilities
{
    [Trait("Category", "Text sanitizer utility Unit Tests")]
    public class TextSanitizerUtilitiesTests
    {
        private const string ComplexHtmlCase =
@"<!DOCTYPE html>
<html>
<style>
p {
	color: red;
}
</style>
<body>
	<h1>This is <b>bold</b> heading</h1>
	<p>This is <u>underlined</u> paragraph</p>
	<h2>This is <i>italic</i> &amp; heading</h2>
	<p>sections: 233.9 & 517.3;</p>
	<button onClick=""alert('you clicked me');"">click me</button>
</body>
</html>";

        private const string ComplexHtmlResult = "     This is bold heading  This is underlined paragraph  This is italic & heading  sections: 233.9 & 517.3;  click me  ";

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData("", "", null, null)]
        [InlineData(" ", " ", null, null)]
        [InlineData("hello world", "hello cruel world", "hello world", "hello cruel world")]
        [InlineData("<p>hello world", "<p>hello <b>cruel</b> world", "hello world", "hello cruel world")]
        public void TextSanitizerUtilitiesSanitizeReturnsSuccess(string name, string description, string expectedName, string expectedDescription)
        {
            //arrange
            var apiTestModel = new ApiTestModel { Id = Guid.NewGuid(), Name = name, Description = description };

            //act
            TextSanitizerUtilities.Sanitize(apiTestModel);

            //assert
            Assert.Equal(expectedName, apiTestModel.Name);
            Assert.Equal(expectedDescription, apiTestModel.Description);
        }

        [Fact]
        public void TextSanitizerUtilitiesSanitizeReturnsArgumentNullException()
        {
            //arrange
            ApiTestModel? nullApiTestModel = null;

            //act
            var exceptionResult = Assert.Throws<ArgumentNullException>(() => TextSanitizerUtilities.Sanitize(nullApiTestModel));

            // assert
            Assert.Equal("Value cannot be null. (Parameter 'model')", exceptionResult.Message);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", null)]
        [InlineData("hello world", "hello world")]
        [InlineData("<p>hello <b>cruel</b> world", "hello cruel world")]
        [InlineData(ComplexHtmlCase, ComplexHtmlResult)]
        public void TextSanitizerUtilitiesSanitizeStringReturnsExpected(string? testValue, string? expectedResult)
        {
            //arrange

            //act
            var result = TextSanitizerUtilities.SanitizeString(testValue);

            //assert
            Assert.Equal(expectedResult, result);
        }
    }
}
