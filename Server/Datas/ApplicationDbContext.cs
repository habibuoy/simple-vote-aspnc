using Microsoft.EntityFrameworkCore;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Datas;

public class ApplicationDbContext : DbContext
{
    private readonly ILogger<ApplicationDbContext> logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        ILogger<ApplicationDbContext> logger)
        : base(options) { this.logger = logger; }

    public DbSet<Vote> Votes { get; set; }
    public DbSet<VoteSubject> VoteSubjects { get; set; }
    public DbSet<VoteSubjectInput> VoteSubjectInputs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vote>()
            .HasMany(v => v.Subjects)
            .WithOne(s => s.Vote)
            .HasForeignKey(s => s.VoteId);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.CreatorId);

        modelBuilder.Entity<VoteSubject>()
            .HasMany(v => v.Voters)
            .WithOne(i => i.VoteSubject)
            .HasForeignKey(i => i.SubjectId);

        modelBuilder.Entity<VoteSubject>()
            .Property(v => v.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<VoteSubject>().ToTable("VoteSubjects");
        modelBuilder.Entity<VoteSubjectInput>().ToTable("VoteInputs");

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(entity => entity.GetProperties()))
        {
            var propInfo = property.PropertyInfo;

            if (property.ClrType != typeof(DateTime) && property.ClrType != typeof(DateTime?)
                || propInfo == null
                || propInfo.GetCustomAttributes(true).Any(attr => attr is NotUtcAttribute))
            {
                continue;
            }

            if (property.ClrType == typeof(DateTime))
            {
                property.SetValueConverter(typeof(DateTimeUtcConverter));
            }
            else if (property.ClrType == typeof(DateTime?))
            {
                property.SetValueConverter(typeof(NullableDateTimeUtcConverter));
            }
        }
    }
}