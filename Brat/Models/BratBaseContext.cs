using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Brat.Models;

public partial class BratBaseContext : DbContext
{
    public BratBaseContext()
    {
    }

    public BratBaseContext(DbContextOptions<BratBaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<FileAsset> FileAssets { get; set; }
    public virtual DbSet<MessageAttachment> MessageAttachments{ get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = $"server={WebSocketClient.GetLocalIPv4()};port=3306;database=BratBase;user=mysqladmin;password=mysqladmin";
        var serverVersion = new MySqlServerVersion(new Version(5, 7, 0));

        optionsBuilder.UseMySql(connectionString, serverVersion);
        //optionsBuilder.UseMySql($"server={WebSocketClient.GetLocalIPv4()};port=3306;database=BratBase;user=mysqladmin;password=mysqladmin", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.30-mysql"));
        /* => optionsBuilder.UseMySql($"server=31.31.197.33;port=3310;database=u3309507_BratBase;user=u3309507_admin;password=Qwerty2594!", Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7-mysql"));*/
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PRIMARY");

            entity.ToTable("chat");

            entity.HasIndex(e => e.UserId1, "user_id1");

            entity.HasIndex(e => e.UserId2, "user_id2");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.UserId1).HasColumnName("user_id1");
            entity.Property(e => e.UserId2).HasColumnName("user_id2");

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.ChatUserId1Navigations)
                .HasForeignKey(d => d.UserId1)
                .HasConstraintName("chat_ibfk_1");

            entity.HasOne(d => d.UserId2Navigation).WithMany(p => p.ChatUserId2Navigations)
                .HasForeignKey(d => d.UserId2)
                .HasConstraintName("chat_ibfk_2");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PRIMARY");

            entity.ToTable("message");

            entity.HasIndex(e => e.ChatId, "chat_id");

            entity.HasIndex(e => e.FromUserId, "from_user_id");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.FromUserId).HasColumnName("from_user_id");
            entity.Property(e => e.MessageText)
                .HasColumnType("text")
                .HasColumnName("message_text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("message_ibfk_1");

            entity.HasOne(d => d.FromUser).WithMany(p => p.MessageFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .HasConstraintName("message_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.MessageUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("message_ibfk_3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Birthday).HasColumnName("birthday");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
            entity.Property(e => e.SecondName)
                .HasMaxLength(50)
                .HasColumnName("second_name");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<FileAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("www_fileasset");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.File)
                .HasMaxLength(100)
                .HasColumnName("file");
            entity.Property(e => e.Kind)
                .HasMaxLength(10)
                .HasColumnName("kind");
            entity.Property(e => e.Mime)
                .HasMaxLength(100)
                .HasColumnName("mime");
            entity.Property(e => e.Size)
                .HasColumnType("bigint unsigned")
                .HasColumnName("size");
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            entity.Property(e => e.UploaderId)
                .HasColumnName("uploader_id");

            // Навигация на User
            entity.HasOne(d => d.Uploader)
                .WithMany(p => p.Files)
                .HasForeignKey(d => d.UploaderId);
        });

        modelBuilder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("www_messageattachment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.MessageId).HasColumnName("message_id");

            entity.HasOne(d => d.File)
                .WithMany(p => p.MessageFiles)
                .HasForeignKey(d => d.FileId);

            entity.HasOne(d => d.Message)
                .WithMany(p => p.MessageFiles)
                .HasForeignKey(d => d.MessageId);
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
