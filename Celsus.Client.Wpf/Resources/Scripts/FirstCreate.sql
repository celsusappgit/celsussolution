USE [Celsus]
GO
/****** Object:  Schema [Celsus]    Script Date: 11/21/2018 5:12:04 PM ******/
CREATE SCHEMA [Celsus]
GO
/****** Object:  FullTextCatalog [FTC01]    Script Date: 11/21/2018 5:12:04 PM ******/
CREATE FULLTEXT CATALOG [FTC01] WITH ACCENT_SENSITIVITY = ON
GO
/****** Object:  Table [Celsus].[ClearText]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[ClearText](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileSystemItemId] [int] NOT NULL,
	[TextInFile] [nvarchar](max) NULL,
 CONSTRAINT [PK_Celsus.ClearText] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[FileSystemItem]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[FileSystemItem](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[FileTypeId] [int] NULL,
	[Name] [nvarchar](max) NULL,
	[FullPath] [nvarchar](max) NULL,
	[OperationDate] [datetime] NOT NULL,
	[FileSystemItemStatusEnum] [int] NOT NULL,
	[IsDirectory] [bit] NOT NULL,
	[ParentId] [int] NULL,
 CONSTRAINT [PK_Celsus.FileSystemItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[FileSystemItemLog]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[FileSystemItemLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[FileSystemItemId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[FileSystemItemLogTypeEnum] [int] NOT NULL,
	[Exception] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[SessionId] [nvarchar](max) NULL,
 CONSTRAINT [PK_Celsus.FileSystemItemLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[FileSystemItemMetadata]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[FileSystemItemMetadata](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileSystemItemId] [int] NOT NULL,
	[Key] [nvarchar](max) NULL,
	[StringValue] [nvarchar](max) NULL,
	[BoolValue] [bit] NULL,
	[IntValue] [int] NULL,
	[DateTimeValue] [datetime] NULL,
	[LongValue] [bigint] NULL,
	[ValueType] [nvarchar](max) NULL,
 CONSTRAINT [PK_Celsus.FileSystemItemMetadata] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[FileType]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[FileType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Regex] [nvarchar](max) NULL,
	[Workflow] [varbinary](max) NULL,
	[IsActive] [bit] NOT NULL,
	[SourceId] [int] NOT NULL,
 CONSTRAINT [PK_Celsus.FileType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[GeneralLog]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[GeneralLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[LogDate] [datetime] NOT NULL,
	[Exception] [nvarchar](max) NULL,
	[GeneralLogTypeEnum] [int] NOT NULL,
 CONSTRAINT [PK_Celsus.GeneralLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[License]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[License](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ServerId] [nvarchar](max) NULL,
	[LicenseKey] [nvarchar](max) NULL,
 CONSTRAINT [PK_Celsus.License] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[ServerRole]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[ServerRole](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ServerName] [nvarchar](max) NULL,
	[ServerRoleEnum] [int] NOT NULL,
	[ServerId] [nvarchar](max) NULL,
	[ServerIP] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Celsus.ServerRole] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[SessionLog]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[SessionLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[LogDate] [datetime] NOT NULL,
	[Exception] [nvarchar](max) NULL,
	[SessionLogTypeEnum] [int] NOT NULL,
 CONSTRAINT [PK_Celsus.SessionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[Source]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[Source](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Path] [nvarchar](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Celsus.Source] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[SourceLog]    Script Date: 11/21/2018 5:12:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[SourceLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[SourceLogTypeEnum] [int] NOT NULL,
	[Exception] [nvarchar](max) NULL,
	[Message] [nvarchar](max) NULL,
	[SessionId] [nvarchar](max) NULL,
 CONSTRAINT [PK_Celsus.SourceLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [Celsus].[Workflow]    Script Date: 11/21/2018 5:12:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [Celsus].[Workflow](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SourceId] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[FileType] [nvarchar](max) NULL,
	[OrderNo] [int] NOT NULL,
	[InternalTypeName] [nvarchar](max) NULL,
	[InternalTypeParameters] [nvarchar](max) NULL,
	[WfWorkflow] [varbinary](max) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Celsus.Workflow] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 11/21/2018 5:12:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE FULLTEXT INDEX ON [Celsus].[ClearText] KEY INDEX [PK_Celsus.ClearText] ON ([FTC01]) WITH (CHANGE_TRACKING AUTO)
GO
USE [Celsus]
GO
ALTER FULLTEXT INDEX ON [Celsus].[ClearText] ADD ([TextInFile] LANGUAGE [Turkish])
GO
ALTER FULLTEXT INDEX ON [Celsus].[ClearText] ENABLE
GO