CREATE TABLE [Celsus].[FileSystemItemLog] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [SourceId]                  INT            NOT NULL,
    [FileSystemItemId]          INT            NOT NULL,
    [StartDate]                 DATETIME       NOT NULL,
    [FileSystemItemLogTypeEnum] INT            NOT NULL,
    [Exception]                 NVARCHAR (MAX) NULL,
    [Message]                   NVARCHAR (MAX) NULL,
    [SessionId]                 NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Celsus.FileSystemItemLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

