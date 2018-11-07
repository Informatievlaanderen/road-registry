namespace RoadRegistry.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Messages;

    internal class RejectionReasons : IEnumerable<Reason>
    {
        public static readonly RejectionReasons None = new RejectionReasons(ImmutableArray<Reason>.Empty);

        private readonly ImmutableArray<Reason> _reasons;

        private RejectionReasons(ImmutableArray<Reason> reasons)
        {
            _reasons = reasons;
        }

        public RejectionReasons BecauseRoadNodeIdTaken()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadNodeIdTaken",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadNodeGeometryTaken(RoadNodeId byOtherNode)
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadNodeGeometryTaken",
                        Parameters = new []
                        {
                            new ReasonParameter
                            {
                                Name = "ByOtherNode",
                                Value = byOtherNode.ToInt32().ToString()
                            }
                        }
                    }
                )
            );
        }

        public IEnumerator<Reason> GetEnumerator() => ((IEnumerable<Reason>)_reasons).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
