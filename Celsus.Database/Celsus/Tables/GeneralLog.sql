CREATE TABLE [Celsus].[GeneralLog] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [Message]            NVARCHAR (MAX) NULL,
    [LogDate]            DATETIME       NOT NULL,
    [Exception]          NVARCHAR (MAX) NULL,
    [GeneralLogTypeEnum] INT            NOT NULL,
    CONSTRAINT [PK_Celsus.GeneralLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

