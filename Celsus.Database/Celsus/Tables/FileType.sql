CREATE TABLE [Celsus].[FileType] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX)  NULL,
    [Regex]    NVARCHAR (MAX)  NULL,
    [Workflow] VARBINARY (MAX) NULL,
    [IsActive] BIT             NOT NULL,
    [SourceId] INT             NOT NULL,
    CONSTRAINT [PK_Celsus.FileType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

