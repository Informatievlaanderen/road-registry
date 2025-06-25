 namespace RoadRegistry.BackOffice.Handlers.Tests.Extracts.WhenRequestingOverlappingTransactionZones;

 using Abstractions.Extracts;
 using Api.Extracts.Handlers;
 using AutoFixture;
 using Editor.Schema;
 using Editor.Schema.Extracts;
 using FluentAssertions;
 using Handlers.Extracts;
 using Microsoft.EntityFrameworkCore;
 using Microsoft.Extensions.Logging.Abstractions;
 using NetTopologySuite.IO;

 public class ByContour
 {
     [Fact]
     public async Task WhenOverlap_ThenReturnExpectedResult()
     {
         // Arrange
         var fixture = new Fixture();
         await using var editorContext = BuildEditorContext();

         var downloadId1 = Guid.NewGuid();

         editorContext.ExtractRequests.Add(new ExtractRequestRecord
         {
             DownloadId = downloadId1,
             Contour = new WKTReader().Read("MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)))"),
             IsInformative = false,
             ExternalRequestId = fixture.Create<string>(),
             Description = fixture.Create<string>()
         });
         await editorContext.SaveChangesAsync();

         var request = new GetOverlappingTransactionZonesByContourRequest
         {
             Contour = "MULTIPOLYGON (((5 0, 5 10, 15 10, 15 0, 5 0)))"
         };

         // Act
         var handler = new GetOverlappingTransactionZonesByContourRequestHandler(editorContext, null, new NullLogger<DownloadExtractByContourRequestHandler>());
         var result = await handler.Handle(request, CancellationToken.None);

         // Assert
         result.DownloadIds.Should().HaveCount(1);
         result.DownloadIds.Single().Should().Be(downloadId1);
     }

     [Fact]
     public async Task WhenOverlapWithInformativeExtract_ThenReturnExpectedResult()
     {
         // Arrange
         var fixture = new Fixture();
         await using var editorContext = BuildEditorContext();

         var downloadId1 = Guid.NewGuid();

         var extractContour = "MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)))";

         editorContext.ExtractRequests.Add(new ExtractRequestRecord
         {
             DownloadId = downloadId1,
             Contour = new WKTReader().Read(extractContour),
             IsInformative = false,
             ExternalRequestId = fixture.Create<string>(),
             Description = fixture.Create<string>()
         });
         editorContext.ExtractRequests.Add(new ExtractRequestRecord
         {
             DownloadId = fixture.Create<Guid>(),
             Contour = new WKTReader().Read(extractContour),
             IsInformative = true,
             ExternalRequestId = fixture.Create<string>(),
             Description = fixture.Create<string>()
         });
         await editorContext.SaveChangesAsync();

         var request = new GetOverlappingTransactionZonesByContourRequest
         {
             Contour = "MULTIPOLYGON (((5 0, 5 10, 15 10, 15 0, 5 0)))"
         };

         // Act
         var handler = new GetOverlappingTransactionZonesByContourRequestHandler(editorContext, null, new NullLogger<DownloadExtractByContourRequestHandler>());
         var result = await handler.Handle(request, CancellationToken.None);

         // Assert
         result.DownloadIds.Should().HaveCount(1);
         result.DownloadIds.Single().Should().Be(downloadId1);
     }

     [Fact]
     public async Task WhenNoOverlap_ThenNone()
     {
         // Arrange
         var fixture = new Fixture();
         await using var editorContext = BuildEditorContext();

         editorContext.ExtractRequests.Add(new ExtractRequestRecord
         {
             DownloadId = fixture.Create<Guid>(),
             Contour = new WKTReader().Read("MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)))"),
             IsInformative = false,
             ExternalRequestId = fixture.Create<string>(),
             Description = fixture.Create<string>()
         });
         await editorContext.SaveChangesAsync();

         var request = new GetOverlappingTransactionZonesByContourRequest
         {
             Contour = "MULTIPOLYGON (((50 0, 50 10, 55 10, 55 0, 50 0)))"
         };

         // Act
         var handler = new GetOverlappingTransactionZonesByContourRequestHandler(editorContext, null, new NullLogger<DownloadExtractByContourRequestHandler>());
         var result = await handler.Handle(request, CancellationToken.None);

         // Assert
         result.DownloadIds.Should().BeEmpty();
     }

     [Fact]
     public async Task WhenOverlapOnlyOnTheBorder_ThenNone()
     {
         // Arrange
         var fixture = new Fixture();
         await using var editorContext = BuildEditorContext();

         editorContext.ExtractRequests.Add(new ExtractRequestRecord
         {
             DownloadId = fixture.Create<Guid>(),
             Contour = new WKTReader().Read("MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)))"),
             IsInformative = false,
             ExternalRequestId = fixture.Create<string>(),
             Description = fixture.Create<string>()
         });
         await editorContext.SaveChangesAsync();

         var request = new GetOverlappingTransactionZonesByContourRequest
         {
             Contour = "MULTIPOLYGON (((10 0, 10 10, 20 10, 20 0, 10 0)))"
         };

         // Act
         var handler = new GetOverlappingTransactionZonesByContourRequestHandler(editorContext, null, new NullLogger<DownloadExtractByContourRequestHandler>());
         var result = await handler.Handle(request, CancellationToken.None);

         // Assert
         result.DownloadIds.Should().BeEmpty();
     }

     private EditorContext BuildEditorContext()
     {
         var options = new DbContextOptionsBuilder<EditorContext>()
             .UseInMemoryDatabase(Guid.NewGuid().ToString())
             .Options;
         return new EditorContext(options);
     }
 }
