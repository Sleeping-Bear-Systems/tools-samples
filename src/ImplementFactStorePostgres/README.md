# ImplementFactStorePostgres

---

The sample illustrates the use of the versioned SQLite databases using *SleepingBearSystems.Tools.Persistence.Sqlite*
library. The sample program creates a Postgres database *TemporaryDatabaseGuard* class, versions it,
and then uses the database as a simple fact store for event sourcing.

The state of the users is stored as a set of facts which are then used to rebuild the current users' state.

## Setup

1. Set up a Postgres database server. Using Docker to host the database server is simple
   approach and instructions can be found at the Postgres Docker repository.  (https://hub.docker.com/_/postgres).
2. Get the connection string to the server. Make sure the credential used to connect to the database server are allowed
   to create and drop databases.
3. Create a environment variable called `SBS_TEST_SERVER_POSTGRES` using the connection string.

## Running the Sample Code

1. Open a PowerShell window.
2. Change the directory to the `tools-samples` root directory.
3. `dotnet run --project .\src\ImplementFactStorePostgres\ImplementFactStorePostgres.csproj`

### *References:*

* [Event Sourcing](https://www.eventstore.com/blog/what-is-event-sourcing)
