using System;
using System.Data;
using System.Data.Common;
using NLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pegasus.Configuration;

namespace Pegasus.Database.Model
{
    public partial class DatabaseContext : DbContext
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Dungeon> Dungeon { get; set; }
        public virtual DbSet<DungeonTile> DungeonTile { get; set; }
        public virtual DbSet<Friend> Friend { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = $"server={ConfigManager.Config.MySql.Host};port={ConfigManager.Config.MySql.Port};user={ConfigManager.Config.MySql.Username}" +
                    $";password={ConfigManager.Config.MySql.Password};database={ConfigManager.Config.MySql.Database}";
                // Use AutoDetect so Pomelo can generate compatible SQL for the server version
                optionsBuilder.UseMySql(conn, Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlServerVersion.AutoDetect(conn));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreateIp)
                    .IsRequired()
                    .HasColumnName("createIp")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.CreateTime)
                    .HasColumnName("createTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.LastIp)
                    .IsRequired()
                    .HasColumnName("lastIp")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.LastTime)
                    .HasColumnName("lastTime")
                    // use MySQL TIMESTAMP so server-side CURRENT_TIMESTAMP ON UPDATE works
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("varchar(100)")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Privileges)
                    .HasColumnName("privileges")
                    .HasColumnType("smallint(6)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<Dungeon>(entity =>
            {
                entity.HasKey(e => e.LandBlockId)
                    .HasName("PRIMARY");

                entity.ToTable("dungeon");

                entity.Property(e => e.LandBlockId)
                    .HasColumnName("landBlockId")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasDefaultValueSql("''");
            });

            modelBuilder.Entity<DungeonTile>(entity =>
            {
                entity.ToTable("dungeon_tile");

                entity.HasIndex(e => e.LandBlockId)
                    .HasDatabaseName("__FK_dungeon_tile_landBlockId__dungeon_landBlockId");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.LandBlockId)
                    .HasColumnName("landBlockId")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.TileId)
                    .HasColumnName("tileId")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.X)
                    .HasColumnName("x")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Y)
                    .HasColumnName("y")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Z)
                    .HasColumnName("z")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.LandBlock)
                    .WithMany(p => p.DungeonTile)
                    .HasForeignKey(d => d.LandBlockId)
                    .HasConstraintName("__FK_dungeon_tile_landBlockId__dungeon_landBlockId");
            });

            modelBuilder.Entity<Friend>(entity =>
            {
                entity.ToTable("friend");

                entity.HasIndex(e => e.Friend1)
                    .HasDatabaseName("__FK_friend_friend__account_id");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.AddTime)
                    .HasColumnName("addTime")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Friend1)
                    .HasColumnName("friend")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Friend1Navigation)
                    .WithMany(p => p.FriendFriend1Navigation)
                    .HasForeignKey(d => d.Friend1)
                    .HasConstraintName("__FK_friend_friend__account_id");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.FriendIdNavigation)
                    .HasForeignKey<Friend>(d => d.Id)
                    .HasConstraintName("__FK_friend_id__account_id");
            });
        }
        public void ApplyMigrations()
            {
                // Temporarily skip applying migrations at runtime so the server can
                // start while we prepare a proper migration for the `lastTime`
                // column (DATETIME -> TIMESTAMP) or move timestamping to the app.
                // Re-enable this after the DB schema has been updated.
                return;
            }

        /// <summary>
        /// Idempotent check that ensures `account.lastTime` is a TIMESTAMP with
        /// ON UPDATE CURRENT_TIMESTAMP. If not, attempts to ALTER the column.
        /// Errors are caught and logged; this method will not throw.
        /// </summary>
        public void EnsureLastTimeTimestamp()
        {
            try
            {
                using DbConnection conn = this.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT COLUMN_TYPE, EXTRA FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='account' AND COLUMN_NAME='lastTime'";
                    using var rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var columnType = rdr.IsDBNull(0) ? null : rdr.GetString(0);
                        var extra = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                        if (!string.IsNullOrEmpty(columnType) && columnType.ToLower().Contains("timestamp")
                            && !string.IsNullOrEmpty(extra) && extra.ToLower().Contains("on update current_timestamp"))
                        {
                            _log.Info("account.lastTime already TIMESTAMP with ON UPDATE CURRENT_TIMESTAMP");
                            return; // already good
                        }
                    }
                }

                _log.Warn("account.lastTime is not TIMESTAMP+ON UPDATE; attempting ALTER TABLE to convert it.");
                using (var alter = conn.CreateCommand())
                {
                    alter.CommandText =
                        "ALTER TABLE `account` MODIFY `lastTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;";
                    alter.ExecuteNonQuery();
                }

                _log.Info("Successfully altered account.lastTime to TIMESTAMP with ON UPDATE CURRENT_TIMESTAMP");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to ensure account.lastTime TIMESTAMP mapping. Manual intervention may be required.");
            }
        }
    }
}
