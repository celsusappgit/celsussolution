CREATE TABLE [Celsus].[Source] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (MAX) NULL,
    [Path]     NVARCHAR (MAX) NULL,
    [Cron]     NVARCHAR (MAX) NULL,
    [IsActive] BIT            NOT NULL,
    CONSTRAINT [PK_Celsus.Source] PRIMARY KEY CLUSTERED ([Id] ASC)
);

