namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using AutoFixture;
    using Events;
    using Commands;
    using FluentValidation;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Testing;
    using Xunit;

    public class RoadNodeScenarios : RoadRegistryFixture
    {

        [Fact]
        public Task when_adding_a_non_existent_node()
        {
            return Run(scenario => scenario
                .GivenNone()
                .When(new Message(new Dictionary<string, object>(), new ChangeRoadNetwork
                {
                    Changeset = new[]
                    {
                        new Commands.RoadNetworkChange
                        {
                            AddRoadNode = new AddRoadNode
                            {
                                Id = 1,
                                Type = Events.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                }))
                .Then(RoadNetworks.Stream, new RoadNetworkChanged
                {
                    Changeset = new[]
                    {
                        new Events.RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                Type = Events.RoadNodeType.FakeNode,
                                Geometry = Fixture.CreateMany<byte>().ToArray()
                            }
                        }
                    }
                }));
        }
    }
}
