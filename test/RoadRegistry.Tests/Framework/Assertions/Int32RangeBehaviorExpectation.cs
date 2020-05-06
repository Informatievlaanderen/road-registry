namespace RoadRegistry.Framework.Assertions
{
    using System;
    using System.Linq;
    using AutoFixture.Idioms;

    /// <summary>
    /// Encapsulates the expected behavior when an <see cref="IGuardClauseCommand" /> (typically
    /// representing a method or constructor) is invoked with a <see cref="Int32" /> less than 0 argument.
    /// </summary>
    /// <seealso cref="Verify(IGuardClauseCommand)" />
    public class Int32RangeBehaviorExpectation : IBehaviorExpectation
    {
        private readonly int _lowerLimitInclusive;
        private readonly int _upperLimitInclusive;
        private readonly int[] _exceptions;

        public Int32RangeBehaviorExpectation(int lowerLimitInclusive, int upperLimitInclusive)
        {
            if (lowerLimitInclusive > upperLimitInclusive)
            {
                throw new ArgumentException("The inclusive lower limit needs to be less than or equal to the inclusive upper limit.", nameof(lowerLimitInclusive));
            }
            _lowerLimitInclusive = lowerLimitInclusive;
            _upperLimitInclusive = upperLimitInclusive;
            _exceptions = new int[0];
        }

        public Int32RangeBehaviorExpectation(int lowerLimitInclusive, int upperLimitInclusive, params int[] exceptions)
        {
            if (lowerLimitInclusive > upperLimitInclusive)
            {
                throw new ArgumentException("The inclusive lower limit needs to be less than or equal to the inclusive upper limit.", nameof(lowerLimitInclusive));
            }
            _lowerLimitInclusive = lowerLimitInclusive;
            _upperLimitInclusive = upperLimitInclusive;
            _exceptions = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
        }

        /// <summary>
        /// Verifies the behavior of the command when invoked with <see cref="Int32" /> less than 0.
        /// </summary>
        /// <param name="command">The command whose behavior must be examined.</param>
        /// <remarks>
        /// <para>
        /// This method encapsulates the behavior which is expected when a method or constructor is
        /// invoked with <see cref="Int32" /> less than 0 as one of the method arguments. In that case it's
        /// expected that invoking <paramref name="command" /> with Int32 less than 0 throws an
        /// <see cref="ArgumentException" />, causing the Verify method to succeed. If other
        /// exceptions are thrown, or no exception is thrown when invoking the command, the Verify
        /// method throws an exception.
        /// </para>
        /// </remarks>
        public void Verify(IGuardClauseCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.RequestedType != typeof(int))
                return;

            try
            {
                var random = new Random();
                if (_exceptions.Length != 0)
                {
                    if (random.Next() % 2 == 0)
                    {
                        var value = random.Next(int.MinValue, _lowerLimitInclusive + 1);
                        while (_exceptions.Contains(value))
                        {
                            value = random.Next(int.MinValue, _lowerLimitInclusive + 1);
                        }

                        command.Execute(value);
                    }
                    else
                    {
                        var value = random.Next(_upperLimitInclusive, int.MaxValue);
                        while (_exceptions.Contains(value))
                        {
                            value = random.Next(_upperLimitInclusive, int.MaxValue);
                        }

                        command.Execute(value);
                    }
                }
                else
                {
                    if (random.Next() % 2 == 0)
                    {
                        command.Execute(random.Next(int.MinValue, _lowerLimitInclusive + 1));
                    }
                    else
                    {
                        command.Execute(random.Next(_upperLimitInclusive, int.MaxValue));
                    }
                }

            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            catch (Exception e)
            {
                throw command.CreateException("\"Int32Range\"", e);
            }

            throw command.CreateException("\"Int32Range\"");
        }
    }
}
