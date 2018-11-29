using Celsus.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.DataLayer
{
    public class SqlDbContext: DbContext
    {
        public virtual DbSet<SourceDto> Sources { get; set; }
        public virtual DbSet<WorkflowDto> Workflows { get; set; }
        public virtual DbSet<FileTypeDto> FileTypes { get; set; }

        public virtual DbSet<FileSystemItemMetadataDto> FileSystemItemMetadatas { get; set; }
        public virtual DbSet<LicenseDto> Licenses { get; set; }
        public virtual DbSet<FileSystemItemDto> FileSystemItems { get; set; }
        public virtual DbSet<ServerRoleDto> ServerRoles { get; set; }
        public virtual DbSet<SourceLogDto> SourceLogs { get; set; }
        public virtual DbSet<SessionLogDto> SessionLogs { get; set; }
        public virtual DbSet<GeneralLogDto> GeneralLogs { get; set; }
        public virtual DbSet<ClearTextDto> ClearTexts { get; set; }
        public virtual DbSet<FileSystemItemLogDto> FileSystemItemLogs { get; set; }

        public SqlDbContext() : base("SqlDbContextConnection")
        {
            Database.SetInitializer<SqlDbContext>(null);
        }

        public SqlDbContext(string connString)
        {
            Database.SetInitializer<SqlDbContext>(null);
            this.Database.Connection.ConnectionString = connString;
        }

    }
}
