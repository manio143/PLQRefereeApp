using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PLQRefereeApp
{
    public partial class MainContext : DbContext
    {
        public MainContext(DbContextOptions<MainContext> options) : base(options) { }

        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<QuestionsAnswer> QuestionsAnswers { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<TestQuestion> TestQuestions { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("answer")
                    .HasMaxLength(1000);

                entity.Property(e => e.Correct)
                    .HasColumnName("correct")
                    .HasColumnType("tinyint(1)");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Information)
                    .IsRequired()
                    .HasColumnName("information")
                    .HasMaxLength(120)
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("question")
                    .HasMaxLength(1000);

                entity.Property(e => e.TypeData)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(2);
            });

            modelBuilder.Entity<QuestionsAnswer>(entity =>
            {
                entity.HasKey(e => new { e.QuestionId, e.AnswerId });

                entity.HasIndex(e => e.AnswerId)
                    .HasName("answerId");

                entity.Property(e => e.QuestionId)
                    .HasColumnName("questionId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AnswerId)
                    .HasColumnName("answerId")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.QuestionsAnswer)
                    .HasForeignKey(d => d.AnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("QuestionsAnswer_ibfk_2");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.QuestionsAnswer)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("QuestionsAnswer_ibfk_1");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("userId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Created)
                    .HasColumnName("created")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("'CURRENT_TIMESTAMP'");

                entity.Property(e => e.Finished)
                    .HasColumnName("finished")
                    .HasColumnType("datetime");

                entity.Property(e => e.Started)
                    .HasColumnName("started")
                    .HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(2);

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Test)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Test_ibfk_1");
            });

            modelBuilder.Entity<TestQuestion>(entity =>
            {
                entity.HasKey(e => new { e.TestId, e.QuestionId });

                entity.HasIndex(e => e.AnswerId)
                    .HasName("TestQuestion_ibfk_3");

                entity.HasIndex(e => e.QuestionId)
                    .HasName("TestQuestion_ibfk_2");

                entity.Property(e => e.TestId)
                    .HasColumnName("testId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.QuestionId)
                    .HasColumnName("questionId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AnswerId)
                    .HasColumnName("answerId")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Answer)
                    .WithMany(p => p.TestQuestion)
                    .HasForeignKey(d => d.AnswerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("TestQuestion_ibfk_3");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.TestQuestion)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("TestQuestion_ibfk_2");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.TestQuestion)
                    .HasForeignKey(d => d.TestId)
                    .HasConstraintName("TestQuestion_ibfk_1");
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasIndex(e => e.Ar)
                    .HasName("ar");

                entity.HasIndex(e => e.Hr)
                    .HasName("hr");

                entity.HasIndex(e => e.Sr)
                    .HasName("sr");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ar)
                    .HasColumnName("ar")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ArIrdp)
                    .HasColumnName("arIRDP")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Arcooldown)
                    .HasColumnName("arcooldown")
                    .HasColumnType("datetime");

                entity.Property(e => e.Hr)
                    .HasColumnName("hr")
                    .HasColumnType("int(11)");

                entity.Property(e => e.HrIrdp)
                    .HasColumnName("hrIRDP")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.HrPayment)
                    .HasColumnName("hrPayment")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Hrcooldown)
                    .HasColumnName("hrcooldown")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(32);

                entity.Property(e => e.Sr)
                    .HasColumnName("sr")
                    .HasColumnType("int(11)");

                entity.Property(e => e.SrIrdp)
                    .HasColumnName("srIRDP")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Srcooldown)
                    .HasColumnName("srcooldown")
                    .HasColumnType("datetime");

                entity.Property(e => e.Surname)
                    .IsRequired()
                    .HasColumnName("surname")
                    .HasMaxLength(32);

                entity.Property(e => e.Team)
                    .IsRequired()
                    .HasColumnName("team")
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserData)
                    .HasForeignKey<UserData>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserData_ibfk_1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedNever();

                entity.Property(e => e.Administrator)
                    .HasColumnName("administrator")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasMaxLength(128);

                entity.Property(e => e.Passphrase)
                    .IsRequired()
                    .HasColumnName("passphrase")
                    .HasMaxLength(64);

                entity.Property(e => e.Reset)
                    .HasColumnName("reset")
                    .HasMaxLength(128);
            });
        }
    }
}
