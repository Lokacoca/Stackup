-- StackUp database schema
-- Generated 2026-11-08 from the LocalDB database attached from StackUp.mdf
-- (read-only queries against INFORMATION_SCHEMA and sys.* catalog views).
--
-- Notes:
--  * The database has NO foreign key constraints, unique constraints, check
--    constraints, defaults, views, procedures or triggers. Only primary keys.
--  * Primary key names are the auto-generated ones in the database (several
--    still carry the "tmp_ms_x" prefix left behind by the Visual Studio table
--    designer when the table was rebuilt).
--  * Collation: SQL_Latin1_General_CP1_CI_AS (case-insensitive).
--  * Relationships that exist only in application code are listed as comments
--    under each table. See docs/database.md for details.

CREATE TABLE [dbo].[UserTable] (
    [UID]      INT            IDENTITY (1, 1) NOT NULL,
    [UserName] NVARCHAR (MAX) NULL,
    [Password] NVARCHAR (MAX) NULL,   -- stored in plain text by the app
    [Picture]  NVARCHAR (MAX) NULL,   -- file name under wwwroot/Bilder
    [Desc]     NVARCHAR (MAX) NULL,
    [Rank]     NVARCHAR (MAX) NULL,   -- rank image file name, e.g. "12.png"
    [TID]      INT            NULL,   -- current team; 0 = no team
    [tagline]  NCHAR (10)     NULL,   -- fixed-width, right-padded with spaces
    [KD]       NVARCHAR (MAX) NULL,   -- not used by the code
    CONSTRAINT [PK__tmp_ms_x__C5B19602359AC804] PRIMARY KEY CLUSTERED ([UID] ASC)
);
-- Code-only link: UserTable.TID -> TeamTable.TID (0 used as "no team", so a real FK would need NULL instead)

CREATE TABLE [dbo].[TeamTable] (
    [TID]     INT            IDENTITY (1, 1) NOT NULL,
    [Elo]     INT            NULL,
    [Picture] NVARCHAR (MAX) NULL,
    [Name]    NVARCHAR (MAX) NULL,
    [Desc]    NVARCHAR (MAX) NULL,
    [LID]     INT            NULL,   -- "Leader ID": the UID of the team leader (NOT a LobbyTable.LID)
    CONSTRAINT [PK__tmp_ms_x__C456D72962EB0FE4] PRIMARY KEY CLUSTERED ([TID] ASC)
);
-- Code-only link: TeamTable.LID -> UserTable.UID

CREATE TABLE [dbo].[LobbyTable] (
    [LID]        INT            IDENTITY (1, 1) NOT NULL,
    [TeamCreate] INT            NULL,   -- team that opened the lobby
    [TeamJoin]   INT            NULL,   -- team that joined; NULL while waiting
    [Active]     NVARCHAR (MAX) NULL,   -- 'YES' (two teams) / 'NO' (waiting)
    CONSTRAINT [PK__tmp_ms_x__C65557212AE1C06E] PRIMARY KEY CLUSTERED ([LID] ASC)
);
-- Code-only links: LobbyTable.TeamCreate -> TeamTable.TID, LobbyTable.TeamJoin -> TeamTable.TID

CREATE TABLE [dbo].[HistoryTable] (
    [HID]        INT IDENTITY (1, 1) NOT NULL,
    [LID]        INT NULL,   -- LID of the lobby the match came from (lobby row is deleted afterwards)
    [TeamCreate] INT NULL,
    [TeamJoin]   INT NULL,
    CONSTRAINT [PK__tmp_ms_x__C755152704689A2D] PRIMARY KEY CLUSTERED ([HID] ASC)
);
-- Code-only links: HistoryTable.TeamCreate / TeamJoin -> TeamTable.TID
-- HistoryTable.LID points at a LobbyTable row that no longer exists once history is written.

CREATE TABLE [dbo].[MessageTable] (
    [MID]     INT            IDENTITY (1, 1) NOT NULL,
    [Content] NVARCHAR (MAX) NULL,
    [UID]     INT            NULL,   -- author
    [LID]     INT            NULL,   -- set for lobby chat
    [TID]     INT            NULL,   -- set for team chat
    CONSTRAINT [PK__MessageT__C797348A293D5A5A] PRIMARY KEY CLUSTERED ([MID] ASC)
);
-- Code-only links: MessageTable.UID -> UserTable.UID, .LID -> LobbyTable.LID, .TID -> TeamTable.TID

CREATE TABLE [dbo].[RequestTable] (
    [RID] INT IDENTITY (1, 1) NOT NULL,
    [UID] INT NOT NULL,   -- invited user
    [TID] INT NOT NULL,   -- inviting team
    CONSTRAINT [PK__tmp_ms_x__CAFF41322FFC32FC] PRIMARY KEY CLUSTERED ([RID] ASC)
);
-- Code-only links: RequestTable.UID -> UserTable.UID, RequestTable.TID -> TeamTable.TID

CREATE TABLE [dbo].[JoinTable] (
    [JID] INT IDENTITY (1, 1) NOT NULL,
    [UID] INT NOT NULL,   -- user asking to join
    [TID] INT NOT NULL,   -- team being asked
    CONSTRAINT [PK__JoinTabl__C4DD21B58FEC424B] PRIMARY KEY CLUSTERED ([JID] ASC)
);
-- Code-only links: JoinTable.UID -> UserTable.UID, JoinTable.TID -> TeamTable.TID

CREATE TABLE [dbo].[StatsTable] (
    [SID]    INT        IDENTITY (1, 1) NOT NULL,
    [UID]    INT        NULL,
    [MID]    INT        NULL,
    [Kills]  INT        NULL,
    [Deaths] INT        NULL,
    [Assits] INT        NULL,   -- (sic) "Assists"
    [Agent]  NCHAR (10) NULL,
    [KD]     INT        NULL,
    [HS%]    INT        NULL,
    [WIN%]   INT        NULL,
    [ACS]    INT        NULL,
    [DMG/R]  INT        NULL,
    CONSTRAINT [PK__StatsTab__CA195970B27EF62E] PRIMARY KEY CLUSTERED ([SID] ASC)
);
-- Not referenced anywhere in the code. Intended links unclear:
-- UID -> UserTable.UID; MID is probably a match id (it does not match MessageTable.MID semantically).
