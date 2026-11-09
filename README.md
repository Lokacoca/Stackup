# StackUp

<table>
  <tr>
    <td width="50%"><img src="https://github.com/user-attachments/assets/a7e6e1ee-8a9a-467c-867b-1d1425c90f60" alt="StackUp home page" width="100%"><br><sub><b>Home page</b> with live platform stats</sub></td>
    <td width="50%"><img src="https://github.com/user-attachments/assets/206257e3-1925-454d-97bc-c433f8e063cc" alt="Matchmaking page" width="100%"><br><sub><b>Matchmaking</b>: teams searching for matches and ongoing matches</sub></td>
  </tr>
  <tr>
    <td width="50%"><img src="https://github.com/user-attachments/assets/5cbe9ffb-08c9-478e-9a9a-a5404f7d1ee5" alt="Team page" width="100%"><br><sub><b>Team page</b> with roster, ranks and team chat</sub></td>
    <td width="50%"><img src="https://github.com/user-attachments/assets/507bfa71-8a12-4833-8ca7-377306fa7f05" alt="Match lobby" width="100%"><br><sub><b>Match lobby</b>: both teams face to face with a shared lobby chat</sub></td>
  </tr>
</table>

**An online esports hub where Valorant players form teams and set up matches against other teams.** Inspired by platforms like Faceit, StackUp lets a team leader open a lobby, another team join it, and both teams meet in a shared match lobby to chat and play.

## Features

- **Accounts and profiles:** sign up with a Riot-style name and tag, then add a profile picture, a description and your rank
- **Teams of up to five:** create a team, become its leader, edit its name, picture and description, and manage the roster
- **Invitations and applications:** teams can invite players, and players can apply to teams. Both sides accept or decline
- **Matchmaking:** team leaders open a lobby, and other teams can browse open lobbies and join one. Lobbies are shown as searching or ongoing
- **Match lobby:** once two teams meet, both rosters appear side by side with a shared lobby chat
- **Team chat:** every team has its own chat on the team page
- **Match history:** finished matches show up on the player's profile
- **Search:** find players and teams by name
- **Platform stats:** the home page shows matches hosted, registered accounts, hours played and teams created

## How it works

StackUp is built with **ASP.NET Core MVC** and a **SQL Server** database.

- **Controller:** `HomeController` handles each request, reads the logged-in user from the session and performs all database writes, such as creating teams, accepting invitations and starting or ending lobbies.
- **View components:** the reusable parts of each page, such as the lobby list, active matches, team search, invitations, applications, chat and match history, each have their own component with its own query and view. The same component can be placed on several pages without duplicating code.
- **Razor views:** `.cshtml` pages turn the data into HTML.
- **Data access:** all queries are written in SQL with ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataAdapter`). Every query uses parameters instead of building SQL from user input, which protects against SQL injection.

### Database

The database has eight tables: users, teams, lobbies, match history, messages, team invitations, team applications and player stats. The full schema and every relationship between the tables are documented in [`docs/database.md`](docs/database.md) and [`docs/schema.sql`](docs/schema.sql).

## Tech stack

| | |
|---|---|
| Language | C# |
| Framework | ASP.NET Core MVC |
| Database | SQL Server (LocalDB), ADO.NET |
| Front end | Razor views, HTML, CSS, JavaScript |
| Tools | Visual Studio, Git/GitHub |

## Development

I built the first version of StackUp by hand in high school. The goal was to learn about structuring databases, user interface,front and back-end communication, SQL and C# all together in one project. I came back to the project in 2026 and realized that alot of my features where missing, a result of my poor version control :( So i re-implemented what was missing and did alot of testing and bug hunting with the assistance of claude code on a seperate branch. That branch is now merged into this one and the website is fully functional.

## Known limitations and next steps

- Passwords are stored in plain text and should be hashed
- The database has no foreign key constraints, so the relationships between tables are only enforced in the code

## Getting started

1. Clone the repository.
2. Open `Stackup.sln` in Visual Studio 2022 with the **ASP.NET and web development** workload installed. SQL Server LocalDB is included.
3. Press **F5**. The app attaches `StackUp.mdf` from the project folder automatically.
