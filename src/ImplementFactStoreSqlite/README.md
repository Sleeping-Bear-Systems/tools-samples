# ImplementFactStoreSqlite

---

The sample illustrates the use of the versioned SQLite databases using *SleepingBearSystems.Tools.Persistence.Sqlite*
library. The sample program creates an SQLite database *TemporaryDatabaseGuard* class, versions it,
and then uses the database as a simple fact store for event sourcing.

The state of the users is stored as a set of facts which are then used to rebuild the current users' state.

## Running the Sample Code

1. Open a PowerShell window.
2. Change the directory to the `tools-samples` root directory.
3. `dotnet run --project .\src\ImplementFactStoreSqlite\ImplementFactStoreSqlite.csproj`

### *References:*

* [Event Sourcing](https://www.eventstore.com/blog/what-is-event-sourcing)
