CREATE TABLE [Celsus].[ClearText] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [FileSystemItemId] INT            NOT NULL,
    [TextInFile]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Celsus.ClearText] PRIMARY KEY CLUSTERED ([Id] ASC)
);





