CREATE TABLE [Celsus].[FileSystemItem] (
    [Id]                       INT            IDENTITY (1, 1) NOT NULL,
    [SourceId]                 INT            NOT NULL,
    [FileTypeId]               INT            NULL,
    [Name]                     NVARCHAR (MAX) NULL,
    [FullPath]                 NVARCHAR (MAX) NULL,
    [OperationDate]            DATETIME       NOT NULL,
    [FileSystemItemStatusEnum] INT            NOT NULL,
    [ParentId]                 INT            NULL,
    CONSTRAINT [PK_Celsus.FileSystemItem] PRIMARY KEY CLUSTERED ([Id] ASC)
);

