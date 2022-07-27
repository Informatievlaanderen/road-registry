using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using RoadRegistry.BackOffice.Api.Uploads;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;

namespace RoadRegistry.BackOffice.Api
{

    public class UploadControllerFixture
    {
        public Mock<IMediator> MediatorMock { get; }
        public FormFile FormFile { get; }
        public UploadController Controller { get; }

        public UploadControllerFixture()
        {
            MediatorMock = ConfigureMediatorMock();
            Controller = new UploadController(MediatorMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            FormFile = ConfigureFormFile();
        }

        protected virtual Mock<IMediator> ConfigureMediatorMock() => new();

        protected virtual FormFile ConfigureFormFile() => new(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream")}
            })
        };
    }
}
