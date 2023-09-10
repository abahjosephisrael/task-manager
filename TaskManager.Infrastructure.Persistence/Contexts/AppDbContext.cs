﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Commons;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Contexts
{
    public class AppDbContext : IdentityDbContext<User>
    {
        private readonly IDateTimeService dateTime;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IDateTimeService dateTime,
            IAuthenticatedUserService authenticatedUserService
            ) : base(options)
        {
            Database.Migrate();
            this.dateTime = dateTime;
            this.authenticatedUserService = authenticatedUserService;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "Users");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            builder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable(name: "RefreshTokens");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");

            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
            });

            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
            });

            builder.Entity<Domain.Entities.Task>(entity =>
            {
                entity.ToTable("Tasks");
            });

            builder.Entity<UserTask>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("UserTasks");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = dateTime.CurrentUtc;
                        entry.Entity.CreatedBy = authenticatedUserService.UserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = dateTime.CurrentUtc;
                        entry.Entity.LastModifiedBy = authenticatedUserService.UserId;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
