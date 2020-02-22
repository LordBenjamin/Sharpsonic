using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Auricular.DataAccess.Entities;

namespace Auricular.DataAccess.Sqlite
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<MediaLibraryEntry> LibraryEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=/config/auricular.db");
    }
}
