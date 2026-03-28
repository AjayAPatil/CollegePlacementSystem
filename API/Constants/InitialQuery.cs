namespace API.Constants
{
    public class InitialQuery
    {
        public const string InsertAdmin = @"
INSERT INTO Users (UserName, PasswordHash, Role, Status, ProfileImagePath, Email, MobileNo, StreetAddress, City, District, State, Country, PinCode, IsDeleted, CreatedAt, UpdatedAt)
VALUES ('Admin', 'Pass@123', 'Admin', 'Active', '', 'Admin', '', '', '', '', '', '', '', 0, GETDATE(), GETDATE());
SELECT CAST(SCOPE_IDENTITY() as bigint)
";

        public const string DropAndCreateTables = @$"
/*------------------------------------------------
DROP FOREIGN KEYS FIRST
------------------------------------------------*/

IF OBJECT_ID('dbo.Jobs', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Jobs DROP CONSTRAINT FK_Jobs_Users;
END

IF OBJECT_ID('dbo.JobApplications', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.JobApplications DROP CONSTRAINT FK_JobApplications_Jobs;
    ALTER TABLE dbo.JobApplications DROP CONSTRAINT FK_JobApplications_Companies;
    ALTER TABLE dbo.JobApplications DROP CONSTRAINT FK_JobApplications_Students;
    ALTER TABLE dbo.JobApplications DROP CONSTRAINT FK_JobApplications_Users;
END

IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Students DROP CONSTRAINT FK_Students_Users;
END

IF OBJECT_ID('dbo.Companies', 'U') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Companies DROP CONSTRAINT FK_Companies_Users;
END


/*------------------------------------------------
DROP TABLES (Child first)
------------------------------------------------*/

DROP TABLE IF EXISTS dbo.JobApplications;
DROP TABLE IF EXISTS dbo.Jobs;
DROP TABLE IF EXISTS dbo.Students;
DROP TABLE IF EXISTS dbo.Companies;
DROP TABLE IF EXISTS dbo.Users;



/*------------------------------------------------
CREATE USERS TABLE
------------------------------------------------*/

CREATE TABLE [dbo].[Users] (
    
    UserId BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    ProfileImagePath NVARCHAR(500) NULL,

    Email NVARCHAR(255) NOT NULL UNIQUE,
    MobileNo NVARCHAR(20) NOT NULL,
    StreetAddress NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    District NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    PinCode INT NOT NULL,

    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);



/*------------------------------------------------
CREATE STUDENTS TABLE
------------------------------------------------*/

CREATE TABLE [dbo].[Students] (

    Id BIGINT IDENTITY(1,1) PRIMARY KEY,

    UserId BIGINT NOT NULL,

    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NULL,
    Nationality NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    BloodGroup NVARCHAR(10) NULL,

    EnrollmentNo NVARCHAR(50) NOT NULL,
    Department NVARCHAR(100) NOT NULL,
    PassingYear INT NOT NULL,
    CGPA DECIMAL(4,2) NOT NULL,

    ResumeFilePath NVARCHAR(500) NULL,
    Skills NVARCHAR(MAX) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Students_Users
    FOREIGN KEY (UserId)
    REFERENCES Users(UserId)
    ON DELETE CASCADE
);


CREATE UNIQUE INDEX IX_Students_UserId
ON Students(UserId);



/*------------------------------------------------
CREATE COMPANIES TABLE
------------------------------------------------*/

CREATE TABLE [dbo].[Companies] (

    Id BIGINT IDENTITY(1,1) PRIMARY KEY,

    UserId BIGINT NOT NULL,

    CompanyName NVARCHAR(200) NOT NULL,
    Website NVARCHAR(200) NULL,
    Description NVARCHAR(MAX) NULL,
    Industry NVARCHAR(150) NULL,
    Location NVARCHAR(200) NULL,

    HRName NVARCHAR(150) NULL,
    ContactEmail NVARCHAR(255) NULL,
    ContactPhone NVARCHAR(20) NULL,

    FoundedYear INT NULL,
    CompanySize INT NULL,

    LogoUrl NVARCHAR(500) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_Companies_Users
    FOREIGN KEY (UserId)
    REFERENCES Users(UserId)
    ON DELETE CASCADE
);



/*------------------------------------------------
CREATE JOBS TABLE
------------------------------------------------*/

CREATE TABLE [dbo].[Jobs] (
    JobId BIGINT IDENTITY(1,1) PRIMARY KEY,

    CompanyId BIGINT NOT NULL,

    JobTitle NVARCHAR(200) NOT NULL,
    Department NVARCHAR(100),

    JobType NVARCHAR(50) NOT NULL,   -- Full-time, Internship, etc.
    WorkMode NVARCHAR(50) NOT NULL,  -- Remote, Hybrid, On-site

    Location NVARCHAR(150) NOT NULL,

    ExperienceMin INT NOT NULL,
    ExperienceMax INT NOT NULL,

    SalaryMin DECIMAL(10,2),
    SalaryMax DECIMAL(10,2),

    Openings INT NOT NULL DEFAULT 1,

    Responsibilities NVARCHAR(MAX),
    RequiredSkills NVARCHAR(MAX),
    PreferredSkills NVARCHAR(MAX),
    Qualifications NVARCHAR(MAX),
    Benefits NVARCHAR(MAX),

    Status NVARCHAR(20) NOT NULL DEFAULT 'draft', -- draft, published, closed

    ExpiryDate DATE,

    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Jobs_Users
        FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
        ON DELETE NO ACTION,
    CONSTRAINT FK_Jobs_Company FOREIGN KEY (CompanyId)
        REFERENCES Companies(Id)
        ON DELETE CASCADE
);

/*------------------------------------------------
CREATE JOB APPLICATIONS TABLE
------------------------------------------------*/

CREATE TABLE [dbo].[JobApplications] (
    ApplicationId BIGINT IDENTITY(1,1) PRIMARY KEY,
    JobId BIGINT NOT NULL,
    CompanyId BIGINT NOT NULL,
    StudentId BIGINT NOT NULL,
    StudentUserId BIGINT NOT NULL,
    StudentName NVARCHAR(250) NOT NULL,
    StudentEmail NVARCHAR(255) NOT NULL,
    StudentPhone NVARCHAR(20) NULL,
    ResumeFilePath NVARCHAR(500) NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'applied',
    AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    InterviewScheduledAt DATETIME2 NULL,
    InterviewMode NVARCHAR(100) NULL,
    InterviewLocation NVARCHAR(300) NULL,
    InterviewNotes NVARCHAR(MAX) NULL,
    DecisionAt DATETIME2 NULL,
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_JobApplications_Jobs FOREIGN KEY (JobId) REFERENCES Jobs(JobId) ON DELETE CASCADE,
    CONSTRAINT FK_JobApplications_Companies FOREIGN KEY (CompanyId) REFERENCES Companies(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_JobApplications_Students FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_JobApplications_Users FOREIGN KEY (StudentUserId) REFERENCES Users(UserId) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IX_JobApplications_JobId_StudentId
ON JobApplications(JobId, StudentId);

{InsertAdmin}
";
    }
}
