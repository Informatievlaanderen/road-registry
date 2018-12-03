// ReSharper disable ImpureMethodCallOnReadonlyValueField
namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
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

        public RejectionReasons BecauseRoadSegmentIdTaken()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentIdTaken",
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

        public RejectionReasons BecauseRoadNodeTooClose(RoadNodeId toOtherNode)
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadNodeTooClose",
                        Parameters = new []
                        {
                            new ReasonParameter
                            {
                                Name = "ToOtherNode",
                                Value = toOtherNode.ToInt32().ToString()
                            }
                        }
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentGeometryTaken",
                        Parameters = new []
                        {
                            new ReasonParameter
                            {
                                Name = "ByOtherSegment",
                                Value = byOtherSegment.ToInt32().ToString()
                            }
                        }
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadNodeNotConnectedToAnySegment()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadNodeNotConnectedToAnySegment",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentStartNodeMissing()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentStartNodeMissing",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentEndNodeMissing()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentEndNodeMissing",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentGeometryLengthIsZero()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentGeometryLengthIsZero",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentStartPointDoesNotMatchNodeGeometry()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentStartPointDoesNotMatchNodeGeometry",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public RejectionReasons BecauseRoadSegmentEndPointDoesNotMatchNodeGeometry()
        {
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadSegmentEndPointDoesNotMatchNodeGeometry",
                        Parameters = new ReasonParameter[0]
                    }
                )
            );
        }

        public IEnumerator<Reason> GetEnumerator() => ((IEnumerable<Reason>)_reasons).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RejectionReasons BecauseRoadNodeTypeMismatch(params RoadNodeType[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (types.Length == 0)
                throw new ArgumentException("The expected road node types must contain at least one.", nameof(types));
            return new RejectionReasons(
                _reasons.Add(
                    new Reason
                    {
                        Because = "RoadNodeTypeMismatch",
                        Parameters = types.Select(type => new ReasonParameter
                        {
                            Name = "Expected", Value = type.ToString()
                        }).ToArray()
                    }
                )
            );
        }
    }
}
