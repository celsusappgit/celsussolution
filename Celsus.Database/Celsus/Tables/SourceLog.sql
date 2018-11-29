CREATE TABLE [Celsus].[SourceLog] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [SourceId]          INT            NOT NULL,
    [StartDate]         DATETIME       NOT NULL,
    [SourceLogTypeEnum] INT            NOT NULL,
    [Exception]         NVARCHAR (MAX) NULL,
    [Message]           NVARCHAR (MAX) NULL,
    [SessionId]         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Celsus.SourceLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

