using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Sharpsonic.DataAccess.Entities;

namespace Sharpsonic.DataAccess.Sqlite
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<MediaLibraryEntry> LibraryEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=/config/auricular.db");
    }
}
