USE [municipality-registry]
GO
CREATE SCHEMA MunicipalityRegistryLegacy
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [MunicipalityRegistryLegacy].[MunicipalitySyndication](
	[Position] [bigint] NOT NULL,
	[MunicipalityId] [uniqueidentifier] NOT NULL,
	[NisCode] [nvarchar](max) NULL,
	[ChangeType] [nvarchar](max) NULL,
	[DefaultName] [nvarchar](max) NULL,
	[NameDutch] [nvarchar](max) NULL,
	[NameFrench] [nvarchar](max) NULL,
	[NameGerman] [nvarchar](max) NULL,
	[NameEnglish] [nvarchar](max) NULL,
	[Status] [int] NULL,
	[RecordCreatedAt] [datetimeoffset](7) NOT NULL,
	[LastChangedOn] [datetimeoffset](7) NOT NULL,
	[Application] [int] NULL,
	[Modification] [int] NULL,
	[Operator] [nvarchar](max) NULL,
	[Organisation] [int] NULL,
	[FacilitiesLanguages] [nvarchar](max) NULL,
	[OfficialLanguages] [nvarchar](max) NULL,
	[EventDataAsXml] [nvarchar](max) NULL,
	[Reason] [nvarchar](max) NULL,
 CONSTRAINT [PK_MunicipalitySyndication] PRIMARY KEY CLUSTERED 
(
	[Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MunicipalitySyndication_MunicipalityId] ON [MunicipalityRegistryLegacy].[MunicipalitySyndication]
(
	[MunicipalityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED COLUMNSTORE INDEX [CI_MunicipalitySyndication_Position] ON [MunicipalityRegistryLegacy].[MunicipalitySyndication]
(
	[Position]
)WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0) ON [PRIMARY]
GO
