using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests.FeatureCompare
{
    using Fixtures;

    public class WhenMessageReceivedWithKnownType : IClassFixture<WhenMessageReceivedWithKnownTypeFixture>
    {
        private readonly WhenMessageReceivedWithKnownTypeFixture _fixture;

        public WhenMessageReceivedWithKnownType(WhenMessageReceivedWithKnownTypeFixture fixture) => _fixture = fixture;

        [Fact]
        public void ItShouldSucceed()
        {
            Assert.True(_fixture.Result);
        }
    }
}
