/*
Reclaim - database-first training fixture
Training Day 10, 10 September 2026

Purpose: supply an existing database for an EF Core scaffolding exercise.
These are Reclaim practice requirements, not official competition rules.
All sample people are fictional display names.

Open in SQL Server Management Studio, connect to your local SQL Server,
and execute the complete file. The script targets ReclaimPracticeDb only.
It creates the database if missing and refuses to seed a database that
already contains user tables. It does not delete or replace existing data.

Expected rows: ItemCategories 6, FoundItems 6, Claimants 3, ItemClaims 4.
Later API lessons will implement validation and coordinated status changes.
*/

USE [master];
GO

IF DB_ID(N'ReclaimPracticeDb') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [ReclaimPracticeDb];');
END;
GO

USE [ReclaimPracticeDb];

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'ReclaimPracticeDb'
BEGIN
    THROW 50001, 'This script must run in ReclaimPracticeDb.', 1;
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
BEGIN
    THROW 50002, 'ReclaimPracticeDb already contains tables. Nothing was changed. Inspect the existing database instead of running setup again.', 1;
END;

BEGIN TRANSACTION;

-- Categories are database rows in this exercise, rather than a C# enum.
CREATE TABLE dbo.ItemCategories
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemCategories PRIMARY KEY,
    Name NVARCHAR(40) NOT NULL CONSTRAINT UQ_ItemCategories_Name UNIQUE
);

-- Each found item belongs to one category.
CREATE TABLE dbo.FoundItems
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FoundItems PRIMARY KEY,
    Name NVARCHAR(80) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    CategoryId INT NOT NULL,
    FoundLocation NVARCHAR(80) NOT NULL,
    FoundDate DATE NOT NULL,
    Status NVARCHAR(12) NOT NULL
        CONSTRAINT DF_FoundItems_Status DEFAULT N'Available',
    CONSTRAINT FK_FoundItems_ItemCategories FOREIGN KEY (CategoryId)
        REFERENCES dbo.ItemCategories(Id),
    CONSTRAINT CK_FoundItems_Status
        CHECK (Status IN (N'Available', N'Reserved', N'Returned'))
);

-- A claimant is a person submitting an ownership claim. No login system.
CREATE TABLE dbo.Claimants
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Claimants PRIMARY KEY,
    DisplayName NVARCHAR(40) NOT NULL
);

-- Each claim refers to one item and one claimant.
-- Foreign keys preserve history: referenced parent rows cannot be deleted.
CREATE TABLE dbo.ItemClaims
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemClaims PRIMARY KEY,
    FoundItemId INT NOT NULL,
    ClaimantId INT NOT NULL,
    OwnershipDescription NVARCHAR(500) NOT NULL,
    ClaimedOn DATE NOT NULL,
    Status NVARCHAR(12) NOT NULL
        CONSTRAINT DF_ItemClaims_Status DEFAULT N'Pending',
    CONSTRAINT FK_ItemClaims_FoundItems FOREIGN KEY (FoundItemId)
        REFERENCES dbo.FoundItems(Id),
    CONSTRAINT FK_ItemClaims_Claimants FOREIGN KEY (ClaimantId)
        REFERENCES dbo.Claimants(Id),
    CONSTRAINT CK_ItemClaims_Status
        CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Completed'))
);

INSERT INTO dbo.ItemCategories (Name)
VALUES
    (N'Electronics'),
    (N'Stationery'),
    (N'Clothing'),
    (N'Accessories'),
    (N'Books'),
    (N'Other');

INSERT INTO dbo.FoundItems
    (Name, Description, CategoryId, FoundLocation, FoundDate, Status)
VALUES
    (N'Blue calculator', N'Blue scientific calculator with a small star sticker.',
     1, N'Science lab', '20260907', N'Available'),
    (N'Black pencil case', N'Zip pencil case containing two pencils and a ruler.',
     2, N'Library', '20260908', N'Available'),
    (N'Grey hoodie', N'Plain grey hoodie with a green label inside.',
     3, N'Gym', '20260908', N'Reserved'),
    (N'Green water bottle', N'Metal bottle with a green lid.',
     6, N'Courtyard', '20260909', N'Available'),
    (N'Silver keyring', N'Two keys on a ring with a blue tag.',
     4, N'Reception', '20260907', N'Returned'),
    (N'Physics workbook', N'Workbook with an orange cover.',
     5, N'Classroom 10B', '20260909', N'Available');

INSERT INTO dbo.Claimants (DisplayName)
VALUES (N'Ali'), (N'Mariam'), (N'Omar');

INSERT INTO dbo.ItemClaims
    (FoundItemId, ClaimantId, OwnershipDescription, ClaimedOn, Status)
VALUES
    (1, 1, N'My calculator has a star sticker near the top.', '20260908', N'Pending'),
    (3, 2, N'There is a green label inside my hoodie.', '20260909', N'Approved'),
    (5, 3, N'My keyring has a blue tag and two keys.', '20260908', N'Completed'),
    (1, 2, N'I lost a calculator, but mine has a different case.', '20260909', N'Rejected');

COMMIT TRANSACTION;

-- These counts are the completion check for the database setup step.
SELECT N'ItemCategories' AS TableName, COUNT(*) AS [RowCount] FROM dbo.ItemCategories
UNION ALL
SELECT N'FoundItems', COUNT(*) FROM dbo.FoundItems
UNION ALL
SELECT N'Claimants', COUNT(*) FROM dbo.Claimants
UNION ALL
SELECT N'ItemClaims', COUNT(*) FROM dbo.ItemClaims;
