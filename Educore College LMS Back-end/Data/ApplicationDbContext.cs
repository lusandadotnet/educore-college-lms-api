using Educore_College_LMS_Back_end.Controllers;
using Educore_College_LMS_Back_end.Domain.Courses;
using Educore_College_LMS_Back_end.Domain.Identity;
using Educore_College_LMS_Back_end.Domain.Joins;
using Educore_College_LMS_Back_end.Domain.Lecturers;
using Educore_College_LMS_Back_end.Domain.Modules;
using Educore_College_LMS_Back_end.Domain.Students;
using Educore_College_LMS_Back_end.Domain.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace Educore_College_LMS_Back_end.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Lecturer> Lecturers => Set<Lecturer>();
        public DbSet<LecturerModule> LecturerModules { get; set; }
        public DbSet<StudentModule> StudentModules { get; set; }
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Domain.Modules.Module> Modules => Set<Domain.Modules.Module>();
         
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();
        public DbSet<StudentTask> StudentTasks => Set<StudentTask>();

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            builder.Entity<Student>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            builder.Entity<Lecturer>()
                .HasIndex(l => l.LecturerNumber)
                .IsUnique();

            builder.Entity<Course>()
                .HasOne(c => c.Lecturer)
                .WithMany(l => l.Courses)
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Domain.Modules.Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
                .HasOne(t => t.Module)
                .WithMany(m => m.Tasks)
                .HasForeignKey(t => t.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.StudentId, sc.CourseId });

            builder.Entity<StudentCourse>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.StudentCourses)
                .HasForeignKey(sc => sc.StudentId);

            builder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.StudentCourses)
                .HasForeignKey(sc => sc.CourseId);

            builder.Entity<StudentTask>()
                .HasKey(st => new { st.StudentId, st.TaskItemId });

            builder.Entity<StudentTask>()
                .HasOne(st => st.Student)
                .WithMany(s => s.StudentTasks)
                .HasForeignKey(st => st.StudentId);

            builder.Entity<StudentTask>()
                .HasOne(st => st.TaskItem)
                .WithMany(t => t.StudentTasks)
                .HasForeignKey(st => st.TaskItemId);

            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Lecturer>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<LecturerModule>()
            .HasKey(lm => new { lm.LecturerId, lm.ModuleId });

          
            builder.Entity<LecturerModule>()
                .HasOne(lm => lm.Lecturer)
                .WithMany(l => l.LecturerModules)
                .HasForeignKey(lm => lm.LecturerId);
                

            builder.Entity<LecturerModule>()
                .HasOne(lm => lm.Module)
                .WithMany(m => m.LecturerModules)
                .HasForeignKey(lm => lm.ModuleId);

            builder.Entity<StudentModule>()
                .HasKey(sm => new { sm.StudentId, sm.ModuleId });

            builder.Entity<StudentModule>()
                .HasOne(sm => sm.Student)
                .WithMany(s => s.StudentModules)
                .HasForeignKey(sm => sm.StudentId);

             builder.Entity<StudentModule>()
                .HasOne(sm => sm.Module)
                .WithMany(m => m.StudentModules)
                .HasForeignKey(sm => sm.ModuleId);





        }
    }
}
