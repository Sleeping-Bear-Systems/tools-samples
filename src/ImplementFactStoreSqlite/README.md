# SleepingBearSystems.ToolsSample

## Implement FactStore SQLite

The sample illustrates the use of the versioned SQLite databases using *SleepingBearSystems.Tools.Persistence.Sqlite*
library. The sample program creates an SQLite database *TemporaryDatabaseGuard* class, versions it,
and then uses the database as a simple fact store for event sourcing.

The repository implementation rebuilds the users collection from the
facts stored in the database.

*References:*
* [Event Sourcing](https://www.eventstore.com/blog/what-is-event-sourcing)
