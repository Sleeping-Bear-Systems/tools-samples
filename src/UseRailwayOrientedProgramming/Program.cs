using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Serilog;
using SleepingBearSystems.Tools.Common;
using SleepingBearSystems.Tools.Railway;

namespace SleepingBearSystems.ToolsSamples.UseRailwayOrientedProgramming;

internal static class Program
{
    public static int Main()
    {
        var logger = default(Serilog.Core.Logger);
        try
        {
            // create logger
            logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            var indentationMap = IndentationMap.Create();

            // in this example, the program will validate a string
            // value as not null or empty.  The input string is converted to
            // a result and value if checked for validity.
            logger.Information("simple string validation: success");
            {
                const string value = "The quick brown fox...";
                var result = value
                    .ToResult("valid_tag")
                    .Check(v => !string.IsNullOrEmpty(v), "String cannot be null or empty.");
                result.LogResult(logger, indentationMap);
                logger.Information("valid value: {Value}", result.Unwrap());
            }

            // in this sample, the program will validate a second string as
            // not null or empty but in this case the validation fails.
            logger.Information("simple string validation: failure");
            {
                const string value = "";
                var result = value
                    .ToResult("invalid_tag")
                    .Check(v => !string.IsNullOrEmpty(v), "String cannot be null or empty.");
                result.LogResult(logger, indentationMap);
            }

            // in this sample, the program will do a more complex validation
            // of a user record.  A valid user will have a non-empty user name and
            // password with the user name with a length less than 12 characters and
            // a password with a length greater than 5 characters.
            logger.Information("validate user: success");
            {
                var user = new User("jack_white", "password1234");
                var result = user
                    .ToResult("valid_user")
                    .Check(u => !string.IsNullOrWhiteSpace(u.Name), "User name cannot be null, empty, or whitespace.")
                    .Check(u => u.Name.Length < 12, "User name length must be less than 12 characters.")
                    .Check(
                        u => !string.IsNullOrWhiteSpace(u.Password),
                        "Password cannot be null, empty, or whitespace.")
                    .Check(u => u.Password.Length > 5, "Password length must be greater than 5 characters.");
                result.LogResult(logger, indentationMap);
                logger.Information("valid user: {User}", result.Unwrap());
            }

            // in this sample, the user validation is expanded even further
            // by identifying all validation errors
            logger.Information("validate user: multiple failures");
            {
                var user = new User("cynthia_magenta", "pass");

                var failures = ImmutableList<Result>.Empty;

                // ReSharper disable once UnusedVariable
                var validName = user.Name
                    .ToResult(nameof(user.Name))
                    .Check(v => !string.IsNullOrWhiteSpace(v), "User name cannot be null, empty, or whitespace.")
                    .Check(v => v.Length < 12, "User name length must be less than 12 characters.")
                    .UnwrapOrAddToFailuresImmutable(ref failures);

                // ReSharper disable once UnusedVariable
                var validPassword = user.Password
                    .ToResult(nameof(user.Password))
                    .Check(v => !string.IsNullOrWhiteSpace(v), "Password cannot be null, empty, or whitespace.")
                    .Check(v => v.Length > 5, "Password length must be greater than 5 characters.")
                    .UnwrapOrAddToFailuresImmutable(ref failures);

                if (failures.IsEmpty)
                {
                    // not called
                    logger.Information("valid user: {User}", user);
                }
                else
                {
                    Result<User>
                        .Failure(failures.ToResultError("Invalid user."))
                        .LogResult(logger, indentationMap);
                }
            }

            // in this sample, the user validation is combined into a simple factory
            // method that creates a User instance from parameters
            logger.Information("using a factory method");
            {
                // factory method for creating User instances
                static Result<User> FromParameters(string name, string password)
                {
                    var failures = ImmutableList<Result>.Empty;

                    var validName = name
                        .ToResult(nameof(name))
                        .Check(v => !string.IsNullOrWhiteSpace(v), "User name cannot be null, empty, or whitespace.")
                        .Check(v => v.Length < 12, "User name length must be less than 12 characters.")
                        .UnwrapOrAddToFailuresImmutable(ref failures);

                    var validPassword = password
                        .ToResult(nameof(password))
                        .Check(v => !string.IsNullOrWhiteSpace(v), "Password cannot be null, empty, or whitespace.")
                        .Check(v => v.Length > 5, "Password length must be greater than 5 characters.")
                        .UnwrapOrAddToFailuresImmutable(ref failures);

                    return failures.IsEmpty
                        ? Result<User>.Success(new User(validName, validPassword))
                        : Result<User>.Failure(failures.ToResultError("Unable to create user."));
                }

                // use case: valid user
                FromParameters("jack_white", "password1234")
                    .LogResult(logger, indentationMap,
                        (localLogger, localUser) => { localLogger.Information("valid user: {User}", localUser); });

                // use case: invalid user
                FromParameters("cynthia_magenta", "pass")
                    .LogResult(logger, indentationMap);
            }

            // in this sample, a local method is use to load text from a file.
            // the method takes the file name, add the parent path, loads the
            // lines of text from the file, and returns the concatenated lines
            logger.Information("file handling workflow");
            {
                Result<string> GetFileContent(string filename)
                {
                    return filename
                        .ToResultIsNotNullOrEmpty(tag: nameof(filename))
                        .Transform(validFileName =>
                        {
                            var executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            return Path.Combine(executingPath!, validFileName);
                        })
                        .OnSuccess(path =>
                        {
                            try
                            {
                                var lines = File.ReadLines(path);
                                var content = string.Join(", ", lines);
                                return Result<string>.Success(content);
                            }
                            catch (Exception ex)
                            {
                                ex.FailFastIfCritical(
                                    "SleepingBearSystems.ToolsSamples.UseRailwayOrientedProgramming.Program");
                                return Result<string>.Failure(ex.ToResultError("Cannot read from file."));
                            }
                        });
                }

                // use case: success
                logger.Information("reads content from existing file");
                GetFileContent("railway.txt")
                    .LogResult(logger, indentationMap,
                        (localLogger, localContent) =>
                        {
                            localLogger.Information("content: \"{Content}\"", localContent);
                        });

                // use case: invalid file name
                logger.Information("fails if filename is null or empty");
                GetFileContent(string.Empty)
                    .LogResult(logger, indentationMap);

                // use case: file does not exist
                logger.Information("fails if file does not exist");
                GetFileContent("does-not-exist.txt")
                    .LogResult(logger, indentationMap);
            }

            logger.Information("Exiting...");

            return 0;
        }
        catch (Exception ex)
        {
            ex.FailFastIfCritical("SleepingBearSystems.ToolsSamples.UseRailwayOrientedProgramming.Program");
            logger?.Error(ex, "An error occurred");
            return 1;
        }
    }

    private sealed record User(string Name, string Password);
}
