# StackUp database

StackUp is an ASP.NET Core MVC app where players make accounts, form teams of up to 5, open lobbies, and play team-vs-team matches. All data access uses raw ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataAdapter` into `DataTable`). There is no Entity Framework and no repository layer.

The full DDL is in [`schema.sql`](schema.sql). Everything below was written on 2026-11-08 from reading the code and running read-only catalog queries against the live database.

## Connection

| Where | Value | Used? |
|---|---|---|
| `appsettings.json` → `ConnectionStrings:DefaultConnection` | `Server=(localdb)\MSSQLLocalDB;Database=StackUp;Trusted_Connection=True;` | **No.** Nothing reads it. |
| `GetConnectionString("StackUp.mdf")`, copied into `HomeController` and each ViewComponent | `Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={ContentRoot}\StackUp.mdf;Integrated Security=True` (falls back to `.\SQLEXPRESS` with `User Instance=true`) | Yes |

On LocalDB the database is attached under its file path, `C:\USERS\SHIVD\DESKTOP\STACKUP\STACKUP.MDF`, not under the name `StackUp`.

## Tables

Row counts are from 2026-11-08.

| Table | PK | Rows | Purpose in the app |
|---|---|---|---|
| **UserTable** | `UID` | 21 | Player accounts: `UserName` + `tagline` together act as the login identity (Riot-style `name#tag`), `Password` (plain text), profile `Picture`, `Desc`, `Rank` (image file name picked at sign-up), and `TID` for the player's current team. `TID = 0` means no team. |
| **TeamTable** | `TID` | 6 | Teams: `Name`, `Desc`, `Picture`, `Elo`, and `LID`. Here **`LID` means "leader ID"** and holds the leader's `UID`. It is not a lobby id. |
| **LobbyTable** | `LID` | 1 | Open or running matches. `TeamCreate` is the team that opened the lobby and `TeamJoin` is the opponent (NULL while waiting). `Active` is the string `'NO'` while waiting and `'YES'` once both teams are in. |
| **HistoryTable** | `HID` | 1 | Finished or abandoned matches: `TeamCreate`, `TeamJoin`, and the original lobby `LID`. Shown on the profile page (History component). |
| **MessageTable** | `MID` | 13 | Chat. A message has either `LID` (lobby chat) or `TID` (team chat), plus the author `UID`. Lobby messages are deleted when a lobby ends. |
| **RequestTable** | `RID` | 5 | **Invitations from a team to a user.** The user sees them in the Requests component and accepts or declines. |
| **JoinTable** | `JID` | 3 | **Applications from a user to a team.** The team leader sees them in the Joins component and accepts or declines. |
| **StatsTable** | `SID` | 0 | Per-player match stats (Kills, Deaths, `Assits`, Agent, KD, HS%, WIN%, ACS, DMG/R). **Not used anywhere in the code** and empty. It looks like a planned feature that goes with `Services/WebScraper.cs`, which scrapes tracker.gg and is also never called. |

## Relationships

**The database has no foreign key constraints.** It has primary keys only: no FKs, unique constraints, check constraints or defaults. Every relationship below exists only because of how the code joins the tables.

| From | To | Real FK in DB? | Where the code relies on it |
|---|---|---|---|
| `UserTable.TID` | `TeamTable.TID` | No (code only) | Team rosters, `Settings`, `Search`, `TeamCount`, `Message`. `0` is used as "no team" instead of NULL. |
| `TeamTable.LID` | `UserTable.UID` (team leader) | No (code only) | `Lobbies`, `Settings`, Check/CheckProfile/LobbyList/TeamSearch components, `Lobby`/`Lobby2` (`SELECT t.LID AS Leader`) |
| `LobbyTable.TeamCreate` | `TeamTable.TID` | No (code only) | `CreateLobby`, `StopLobby`, `DeleteTeam`, `Leave`, `Kick`, Active/Check/LobbyList components |
| `LobbyTable.TeamJoin` | `TeamTable.TID` | No (code only) | `JoinLobby`, `LeaveLobby`, Active/Check components |
| `HistoryTable.TeamCreate` / `TeamJoin` | `TeamTable.TID` | No (code only) | History component, `Lobby2` |
| `HistoryTable.LID` | `LobbyTable.LID` | No (code only) | Written by `StopLobby`, then the lobby row is deleted, so this always points to a missing row. Never read. |
| `MessageTable.UID` | `UserTable.UID` | No (code only) | Message component |
| `MessageTable.LID` | `LobbyTable.LID` | No (code only) | Lobby chat, `StopLobby` / `LeaveLobby` cleanup |
| `MessageTable.TID` | `TeamTable.TID` | No (code only) | Team chat on ViewTeam |
| `RequestTable.UID` / `TID` | `UserTable.UID` / `TeamTable.TID` | No (code only) | `CreateRequest`, `AcceptRequest`, `DeclineRequest`, Requests component |
| `JoinTable.UID` / `TID` | `UserTable.UID` / `TeamTable.TID` | No (code only) | `CreateJoin`, `AcceptJoin`, `DeclineJoin`, Joins component |
| `StatsTable.UID` / `MID` | probably `UserTable.UID` / a match id | No | Not used |

The code also joins `LobbyTable.TeamCreate`/`TeamJoin` directly to `UserTable.TID` to answer "is my team in a lobby?". That is just the two links above combined.

```
UserTable (UID) ◄──── TeamTable.LID (leader)
   │ TID ───────────► TeamTable (TID) ◄─── LobbyTable.TeamCreate / TeamJoin
   │                        ▲  ▲  ▲        HistoryTable.TeamCreate / TeamJoin
   ▼                        │  │  └─────── MessageTable.TID (team chat)
RequestTable / JoinTable ───┘  │
MessageTable.UID, .LID ────────┴──► LobbyTable (LID)
```

## Which files use which tables

| File | Tables | Operations |
|---|---|---|
| `Controllers/HomeController.cs` | UserTable, TeamTable, LobbyTable, HistoryTable, MessageTable, RequestTable, JoinTable (and a nonexistent `UserTbl`) | All writes happen here. Details below. |
| `ViewComponents/ActiveViewComponent.cs` | LobbyTable, TeamTable (×2) | SELECT active lobbies with both teams' names and logos |
| `ViewComponents/CheckViewComponent.cs` | UserTable, TeamTable, LobbyTable | SELECT: is the user a leader, is their team in a lobby |
| `ViewComponents/CheckProfileViewComponent.cs` | UserTable, TeamTable, LobbyTable | Same queries as CheckViewComponent |
| `ViewComponents/HistoryViewComponent.cs` | UserTable, HistoryTable, TeamTable (×2) | SELECT the user's team's match history |
| `ViewComponents/JoinsViewComponent.cs` | UserTable, JoinTable | SELECT applications to the session team |
| `ViewComponents/RequestsViewComponent.cs` | TeamTable, RequestTable | SELECT invitations to the session user |
| `ViewComponents/LobbyListViewComponent.cs` | LobbyTable, TeamTable, UserTable | SELECT waiting lobbies, leader check |
| `ViewComponents/MessageViewComponent.cs` | MessageTable, UserTable, TeamTable | SELECT lobby or team chat |
| `ViewComponents/SearchViewComponent.cs` | UserTable, TeamTable | SELECT users `LIKE @input` |
| `ViewComponents/TeamSearchViewComponent.cs` | TeamTable, UserTable | SELECT teams `LIKE @input` with their leader |
| `ViewComponents/TeamCountViewComponent.cs` | UserTable, TeamTable | SELECT team members (member count) |
| `ViewComponents/TeamsViewComponent.cs` | TeamTable | `ORDER BY Elo`. Never rendered. |
| `SQLQuery1.sql` (repo root) | UserTable, TeamTable | Scratch copy of the Settings query with a hard-coded `UID = 13` |
| `Services/WebScraper.cs`, `Models/*`, `Views/*` | none | Views reach the database only through the ViewComponents above. |

### HomeController actions by table

| Table | Actions |
|---|---|
| UserTable | `Index`, `Create2` (INSERT, UPDATE Picture), `Edit` (UPDATE), `ViewTeam`, `JoinTeam`, `CreateTeam`, `Team` (UPDATE TID), `DeleteTeam` (UPDATE TID=0), `Leave`, `Kick`, `CreateRequest`, `AcceptRequest`, `AcceptJoin`, `Profile`, `Lobbies`, `CreateLobby`, `JoinLobby`, `Lobby`, `Lobby2`, `LogIn_Check`, `Search2`, `Settings`, `Search` |
| TeamTable | `ViewTeam`, `CreateTeam`/`Team` (INSERT), `TeamEdit` (UPDATE), `DeleteTeam` (DELETE), `Leave`, `Kick`, `AcceptRequest`, `AcceptJoin`, `Lobbies`, `CreateLobby`, `JoinLobby`, `Lobby`, `Lobby2`, `Settings` |
| LobbyTable | `DeleteTeam`, `Leave`, `Kick`, `Lobbies`, `CreateLobby` (INSERT), `JoinLobby` (UPDATE), `Lobby`, `StopLobby` (DELETE), `LeaveLobby` (UPDATE) |
| HistoryTable | `Lobby2`, `StopLobby` (INSERT), `LeaveLobby` (INSERT) |
| MessageTable | `Message` (INSERT), `StopLobby` / `LeaveLobby` (DELETE by LID) |
| RequestTable | `CreateRequest` (SELECT/INSERT), `DeclineRequest`, `AcceptRequest` (DELETE) |
| JoinTable | `CreateJoin` (SELECT/INSERT), `AcceptRequest`, `DeclineJoin` (DELETE) |

## Inconsistencies and risks

### Bugs: queries that fail at runtime
1. **`DELETE FROM UserTbl`** (`HomeController.cs:344`, `Delete`). The table is named `UserTable`, so account deletion always throws "Invalid object name".
2. **Ambiguous column** (`HomeController.cs:965`, `AcceptRequest`): `... INNER JOIN TeamTable t ON u.TID = t.TID WHERE TID = @TID`. `TID` exists in both tables, so SQL Server raises "Ambiguous column name 'TID'". The same check in `AcceptJoin` (line 1027) correctly uses `t.TID`.
3. **Inconsistent history rows.** `StopLobby` inserts `LID` into HistoryTable, but `LeaveLobby` (line 1549) does not. `StopLobby` and `LeaveLobby` also read `Lobby.Rows[0]` without checking that a row exists, so they crash if there is no lobby.

### SQL injection / string concatenation
No query is built by concatenating user input. Every query I found uses `@parameters` via `AddWithValue`, including the `LIKE @input` searches. Two related points:
- `AddWithValue` infers types, for example `nvarchar(n)` against the `nchar(10)` `tagline`. This is a performance and implicit-conversion concern, not an injection risk.
- The `Active` values `'YES'`/`'NO'` are hard-coded literals, which is safe.

### Security
- **Passwords are stored and compared in plain text** (`Create2`, `LogIn_Check`, `Edit`, `DeleteTeam`).
- **Missing authorization checks against the session.** For example, `DeleteTeam(int tid)` checks the current user's password but never checks that the user leads team `tid`, so any logged-in user can empty and delete any team. `Delete(int UID)`, `Kick(int UID)` and `Edit(int UID, …)` also take the target id from the request.
- `appsettings.json` has a connection string that is never used, while the real one is duplicated in 14 files.

### Missing keys and constraints
- **No foreign keys at all.** Deleting a team (`DeleteTeam`) leaves orphaned RequestTable, JoinTable, MessageTable (team chat) and HistoryTable rows. Members are reset to `TID = 0` manually. One team already has a `LID` (leader) that matches no user.
- `UserTable.TID = 0` is a sentinel for "no team" (7 users currently). An FK would need this to be NULL instead.
- **No unique constraint on (`UserName`, `tagline`)** even though login and lookups treat it as unique. Uniqueness is only checked in code (`Create2`), which leaves a race window. Team `Name` has the same problem (checked in `Team`).
- No unique constraint on (`UID`, `TID`) in RequestTable/JoinTable. Duplicates are only prevented by a SELECT-then-INSERT.
- The "max 5 players per team" and "one lobby per team" rules exist only in code.
- Almost every column is nullable, including names, passwords and lobby team ids.

### Naming and type oddities
- **`LID` means two different things.** `TeamTable.LID` is the leader's UID, while `LobbyTable.LID`, `MessageTable.LID` and `HistoryTable.LID` are lobby ids. This is the most confusing part of the schema.
- **`MID` also means two things.** It is the message id in MessageTable, and in StatsTable it presumably means a match id.
- `tagline` is `nchar(10)`, so values come back padded with spaces. Comparisons still work because SQL Server ignores trailing spaces in `=`, but the C# side receives padded strings.
- `LobbyTable.Active` is `nvarchar(max)` holding `'YES'`/`'NO'` instead of a `bit`.
- Every text column is `nvarchar(max)`, including `Password`, `Rank` and `Active`.
- The code mixes `UserName` and `Username`. This works only because the collation is case-insensitive.
- StatsTable has the typo `Assits` and column names that need brackets (`HS%`, `WIN%`, `DMG/R`). Its `KD` is `int`, while `UserTable.KD` is `nvarchar`.
- Primary key names `PK__tmp_ms_x__…` are left over from Visual Studio table-designer rebuilds.

### Unused objects
- **StatsTable**: never queried and empty.
- **`UserTable.KD`**: never read or written.
- **`TeamTable.Elo`**: only read by `TeamsViewComponent`, which is never rendered, and never written.
- `TeamsViewComponent` is not used by any view. (The unused LeaderBoard component was removed on 2026-11-08.)
- `Services/WebScraper.cs` (Selenium) is never called.
- `userModel` (`Models/userModel.cs`) is never used by the data-access code.
