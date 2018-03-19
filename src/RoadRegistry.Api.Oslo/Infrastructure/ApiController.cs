namespace RoadRegistry.Api.Oslo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.AspNetCore.Mvc.Middleware;
    using Aiv.Vbr.CommandHandling;
    using Aiv.Vbr.CommandHandling.Idempotency;
    using Aiv.Vbr.Utilities.HexByteConvertor;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public abstract class ApiController : ControllerBase
    {
        protected IDictionary<string, object> GetMetadata()
        {
            var ip = User.FindFirst(AddRemoteIpAddressMiddleware.UrnBeVlaanderenVlaamseBasisregistersIp)?.Value;
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userId = User.FindFirst("urn:be:vlaanderen:wegenregister:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBeVlaanderenVlaamseBasisregistersCorrelationId)?.Value;

            return new Dictionary<string, object>
            {
                { "FirstName", firstName },
                { "LastName", lastName },
                { "Ip", ip },
                { "UserId", userId },
                { "CorrelationId", correlationId }
            };
        }
    }

    public abstract class ApiBusController : ApiController
    {
        protected ICommandHandlerResolver Bus { get; }

        protected ApiBusController(ICommandHandlerResolver bus) => Bus = bus;

        protected async Task<IActionResult> IdempotentCommandHandlerDispatch(
            IdempotencyContext context,
            Guid? commandId,
            Func<dynamic> commandFunc,
            CancellationToken cancellationToken)
        {
            if (!commandId.HasValue)
                throw new ApiException("Ongeldig verzoek id.", StatusCodes.Status400BadRequest);

            // First check if the command id already has been processed
            var possibleProcessedCommand = await context.ProcessedCommands
                .Where(x => x.CommandId == commandId)
                .ToDictionaryAsync(x => x.CommandContentHash, x => x, cancellationToken);

            var mappedCommand = commandFunc();
            var contentHash = SHA512
                .Create()
                .ComputeHash(Encoding.UTF8.GetBytes((string) mappedCommand.ToString()))
                .ToHexString();

            // It is possible we have a GUID collision, check the SHA-512 hash as well to see if it is really the same one.
            // Do nothing if commandId with contenthash exists
            if (possibleProcessedCommand.Any() && possibleProcessedCommand.ContainsKey(contentHash))
                return Accepted();

            var processedCommand = new ProcessedCommand(commandId.Value, contentHash);

            try
            {
                // Store commandId in Command Store if it does not exist
                await context.ProcessedCommands.AddAsync(processedCommand, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                // Do work
                return Accepted(
                    await CommandHandlerResolverExtensions.Dispatch(
                        Bus,
                        commandId.Value,
                        mappedCommand,
                        GetMetadata(),
                        cancellationToken));
            }
            catch
            {
                // On exception, remove commandId from Command Store
                context.ProcessedCommands.Remove(processedCommand);
                context.SaveChanges();

                throw;
            }
        }
    }
}
