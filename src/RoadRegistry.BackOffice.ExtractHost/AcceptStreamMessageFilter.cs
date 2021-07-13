namespace RoadRegistry.BackOffice.ExtractHost
{
    using SqlStreamStore.Streams;

    /// <summary>
    /// Indicates if a stream message is acceptable for processing by the event processor.
    /// </summary>
    /// <param name="message">The <c>StreamMessage</c> to filter on</param>
    /// <returns><c>True</c> if the message can be processed, otherwise <c>false</c>.</returns>
    public delegate bool AcceptStreamMessageFilter(StreamMessage message);
}
