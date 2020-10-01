USE [streetname-registry]
GO
CREATE SCHEMA StreetNameRegistryLegacy
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [StreetNameRegistryLegacy].[StreetNameSyndication](
	[Position] [bigint] NOT NULL,
	[StreetNameId] [uniqueidentifier] NOT NULL,
	[PersistentLocalId] [int] NULL,
	[NisCode] [nvarchar](max) NULL,
	[ChangeType] [nvarchar](max) NULL,
	[NameDutch] [nvarchar](max) NULL,
	[NameFrench] [nvarchar](max) NULL,
	[NameGerman] [nvarchar](max) NULL,
	[NameEnglish] [nvarchar](max) NULL,
	[HomonymAdditionDutch] [nvarchar](max) NULL,
	[HomonymAdditionFrench] [nvarchar](max) NULL,
	[HomonymAdditionGerman] [nvarchar](max) NULL,
	[HomonymAdditionEnglish] [nvarchar](max) NULL,
	[Status] [int] NULL,
	[IsComplete] [bit] NOT NULL,
	[RecordCreatedAt] [datetimeoffset](7) NOT NULL,
	[LastChangedOn] [datetimeoffset](7) NOT NULL,
	[Application] [int] NULL,
	[Modification] [int] NULL,
	[Operator] [nvarchar](max) NULL,
	[Organisation] [int] NULL,
	[EventDataAsXml] [nvarchar](max) NULL,
	[Reason] [nvarchar](max) NULL,
	[SyndicationItemCreatedAt] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_StreetNameSyndication] PRIMARY KEY CLUSTERED 
(
	[Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_StreetNameSyndication_StreetNameId] ON [StreetNameRegistryLegacy].[StreetNameSyndication]
(
	[StreetNameId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED COLUMNSTORE INDEX [CI_StreetNameSyndication_Position] ON [StreetNameRegistryLegacy].[StreetNameSyndication]
(
	[Position]
)WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0) ON [PRIMARY]
GO
