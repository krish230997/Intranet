using Microsoft.EntityFrameworkCore;
using Pulse360.Models;

namespace Pulse360.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}
		public DbSet<Chat> Chats { get; set; }
		public DbSet<EmployeeFamilyDetail> EmployeeFamilyDetails { get; set; }
        public DbSet<EmployeeBankDetails> EmployeeBankDetails { get; set; }
        public DbSet<EducationDetails> EducationDetails { get; set; }
        public DbSet<Experience> Experience { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
		public DbSet<MasterIndicators> MasterIndicators { get; set; }
		public DbSet<GoalTypeList> GoalTypeList { get; set; }
		public DbSet<GoalTrackingList> GoalTrackingList { get; set; }
		public DbSet<PerformanceIndicator> PerformanceIndicators { get; set; }
		public DbSet<PerformanceAppraisal> PerformanceAppriasal { get; set; }
		public DbSet<PerformanceReview> PerformanceReviews { get; set; }
		public DbSet<EventModel> Events { get; set; }
        public DbSet<EventTypes> EventTypes { get; set; }
        public DbSet<Projects> AllProjects { get; set; }
        public DbSet<AddAdminDocName> addAdminDocNames { get; set; }
		public DbSet<AddEmployeeDocName> addEmployeeDocNames { get; set; }
		public DbSet<Tickets> Tickets { get; set; }
        public DbSet<TicketReplies> TicketReplies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<DepartmentLeaves> DepartmentLeaves { get; set; }
        public DbSet<MasterLeaveType> MasterLeaveTypes { get; set; }
        public DbSet<User> User { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Designation> Designations { get; set; }
		public DbSet<Role> Role { get; set; }
		public DbSet<Organization> Organization { get; set; }

		public DbSet<TrainingType> TrainingType { get; set; }
		public DbSet<Training> Training { get; set; }

		public DbSet<Trainer> Trainer { get; set; }

		public DbSet<Timesheet> Timesheets { get; set; }

		public DbSet<Activitys> Activiti { get; set; }
		public DbSet<KnowledgeBaseTopic> KnowledgeBaseTopics { get; set; }
		public DbSet<SubTopic> subTopics { get; set; }

		public DbSet<Promotion> Promotion { get; set; }
		public DbSet<Resignation> Resignation { get; set; }
		public DbSet<Termination> Termination { get; set; }
		
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<AdminDocuments> AdminDocuments { get; set; }
        public DbSet<TaskBoards> TaskBoards { get; set; }
        public DbSet<Tasks> Task { get; set; }
        public DbSet<Deduction> Deduction { get; set; }
        public DbSet<DeductionType> DeductionType { get; set; }
        public DbSet<Earning> Earning { get; set; }
        public DbSet<EarningType> EarningType { get; set; }
        public DbSet<EmployeeSalaries> EmployeeSalaries { get; set; }
        public DbSet<EmployeeEarnings> EmployeeEarnings { get; set; }
        public DbSet<EmployeeDeductions> EmployeeDeductions { get; set; }
        public DbSet<Payslips> Payslips { get; set; }

        public DbSet<TaskMembers> Taskmember { get; set; }

        public DbSet<EmployeePerformance> EmployeePerformances { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>()
			.HasOne(u => u.Designation)
			.WithMany()
			.HasForeignKey(u => u.DesignationtId)
			.OnDelete(DeleteBehavior.Restrict); // Ensure no unintended cascading deletes

			base.OnModelCreating(modelBuilder);

			// Configure relationships
			modelBuilder.Entity<KnowledgeBaseTopic>()
				.HasMany(k => k.SubTopics)
				.WithOne(s => s.KnowledgeBaseTopic)
				.HasForeignKey(s => s.KnowledgeBaseTopicId);

            modelBuilder.Entity<User>()
				.HasOne(u => u.Role)
				.WithMany() // Assuming no navigation property in Role for Users
				.HasForeignKey(u => u.RoleId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<User>()
				 .HasOne(e => e.Department)
				 .WithMany()
				 .HasForeignKey(e => e.DepartmentId)
				 .OnDelete(DeleteBehavior.NoAction);  // Prevents cascading delete

			modelBuilder.Entity<Tickets>()
				.HasOne(t => t.AssignedByUser)
				.WithMany()
				.HasForeignKey(t => t.AssignedBy)  // This maps the AssignedBy property to the foreign key column
				.OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tickets>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedTo)  // This maps the AssignedTo property to the foreign key column
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DepartmentLeaves>()
				.HasOne(dl => dl.MasterLeaveType)
				.WithMany(mlt => mlt.DepartmentLeaves)
				.HasForeignKey(dl => dl.LeaveTypeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Designation>()
				.HasOne(d => d.Department)
				.WithMany(dep => dep.Designations)
				.HasForeignKey(d => d.DepartmentId)
					.OnDelete(DeleteBehavior.Restrict);

            // TaskBoards -> Task (Restrict delete to avoid multiple cascading paths)
            modelBuilder.Entity<TaskBoards>()
                .HasOne(t => t.Task)
                .WithMany()
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Restrict); // Fix: Avoid cascading delete

            // TaskBoards -> Projects (Restrict delete to avoid multiple cascading paths)
            modelBuilder.Entity<TaskBoards>()
                .HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict); // Fix: Avoid cascading delete

            // Tasks -> Projects (Ensure foreign key constraint is properly configured)
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict); // Fix: Avoid cascading delete
        }

    }
}
