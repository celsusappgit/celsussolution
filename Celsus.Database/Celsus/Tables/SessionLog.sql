CREATE TABLE [Celsus].[SessionLog] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [SessionId]          NVARCHAR (MAX) NULL,
    [Message]            NVARCHAR (MAX) NULL,
    [LogDate]            DATETIME       NOT NULL,
    [Exception]          NVARCHAR (MAX) NULL,
    [SessionLogTypeEnum] INT            NOT NULL,
    CONSTRAINT [PK_Celsus.SessionLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

