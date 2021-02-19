using DFC.Api.Lmi.Import.Models.JobProfileApi;
using DFC.Api.Lmi.Import.Models.SocJobProfileMapping;
using DFC.Api.Lmi.Import.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DFC.Api.Lmi.Import.UnitTests.Services
{
    [Trait("Category", "job profile to SOC mapping service Unit Tests")]
    public class JobProfilesToSocMappingServiceTests
    {
        private readonly JobProfilesToSocMappingService jobProfilesToSocMappingService;

        public JobProfilesToSocMappingServiceTests()
        {
            jobProfilesToSocMappingService = new JobProfilesToSocMappingService();
        }

        [Fact]
        public void JobProfilesToSocMappingServiceMapReturnsSucess()
        {
            // arrange
            var jobProfileDetails = BuildJobProfileDetails();
            var expectedResult = BuildExpectedMappingResults();

            // act
            var results = jobProfilesToSocMappingService.Map(jobProfileDetails);

            // assert
            expectedResult.Should().BeEquivalentTo(results);
        }

        private IList<JobProfileDetailModel> BuildJobProfileDetails()
        {
            return new List<JobProfileDetailModel>
            {
                new JobProfileDetailModel
                {
                    Soc = 1234,
                    CanonicalName = "canonical-name-1",
                    Title = "A title 1",
                    Url = new Uri("https://somewhere.com", UriKind.Absolute),
                },
                new JobProfileDetailModel
                {
                    Soc = 4321,
                    CanonicalName = "canonical-name-2",
                    Title = "A title 2",
                    Url = new Uri("https://somewhere.com", UriKind.Absolute),
                },
                new JobProfileDetailModel
                {
                    Soc = 4321,
                    CanonicalName = "canonical-name-3",
                    Title = "A title 3",
                    Url = new Uri("https://somewhere.com", UriKind.Absolute),
                },
                new JobProfileDetailModel
                {
                    Soc = 1234,
                    CanonicalName = "canonical-name-4",
                    Title = "A title 4",
                    Url = new Uri("https://somewhere.com", UriKind.Absolute),
                },
            };
        }

        private IList<SocJobProfileMappingModel> BuildExpectedMappingResults()
        {
            return new List<SocJobProfileMappingModel>
            {
                new SocJobProfileMappingModel
                {
                    Soc = 1234,
                    JobProfiles = new List<SocJobProfileItemModel>
                    {
                        new SocJobProfileItemModel
                        {
                            CanonicalName = "canonical-name-1",
                            Title = "A title 1",
                        },
                        new SocJobProfileItemModel
                        {
                            CanonicalName = "canonical-name-4",
                            Title = "A title 4",
                        },
                    },
                },
                new SocJobProfileMappingModel
                {
                    Soc = 4321,
                    JobProfiles = new List<SocJobProfileItemModel>
                    {
                        new SocJobProfileItemModel
                        {
                            CanonicalName = "canonical-name-2",
                            Title = "A title 2",
                        },
                        new SocJobProfileItemModel
                        {
                            CanonicalName = "canonical-name-3",
                            Title = "A title 3",
                        },
                    },
                },
            };
        }
    }
}
