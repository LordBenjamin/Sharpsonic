using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Auricular.DataAccess.Entities;

namespace Auricular.DataAccess.Sqlite
{
    public class SqliteDbContextSettings {
        public string ConnectionString { get; set; }
    };

    public class SqliteDbContext : DbContext
    {
        private readonly SqliteDbContextSettings settings;
        public SqliteDbContext(SqliteDbContextSettings settings) {
            this.settings = settings;
        }

        public DbSet<MediaLibraryEntry> LibraryEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(settings.ConnectionString);
    }
}
