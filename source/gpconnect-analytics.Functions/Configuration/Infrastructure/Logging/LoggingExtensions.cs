using gpconnect_analytics.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using System.Data;

namespace gpconnect_analytics.Configuration.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static ILoggingBuilder ConfigureLoggingServices(ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            var nLogConfiguration = new NLog.Config.LoggingConfiguration();

            var consoleTarget = AddConsoleTarget();
            var databaseTarget = AddDatabaseTarget(configuration);

            nLogConfiguration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);
            nLogConfiguration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, databaseTarget);

            nLogConfiguration.AddTarget(consoleTarget);
            nLogConfiguration.AddTarget(databaseTarget);

            var nLogOptions = new NLogAspNetCoreOptions
            {
                RegisterHttpContextAccessor = true,
                IgnoreEmptyEventId = true,
                IncludeScopes = true,
                ShutdownOnDispose = true
            };

            var logFactory = NLogBuilder.ConfigureNLog(nLogConfiguration);
            logFactory.AutoShutdown = false;

            var nLogConfig = logFactory.Configuration;
            loggingBuilder.AddNLog(nLogConfig, nLogOptions);

            return loggingBuilder;
        }

        private static DatabaseTarget AddDatabaseTarget(IConfiguration configuration)
        {
            var databaseTarget = new DatabaseTarget
            {
                Name = "Database",
                ConnectionString = configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics),
                CommandType = CommandType.StoredProcedure,
                CommandText = "Logging.WriteLog",
                DBProvider = "System.Data.SqlClient"
            };

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Application",
                Layout = "${application}",
                DbType = DbType.String.ToString()
            });

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Logged",
                Layout = "${date}",
                DbType = DbType.DateTime.ToString()
            });

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Level",
                Layout = "${level:uppercase=true}",
                DbType = DbType.String.ToString()
            });

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Message",
                Layout = "${message}",
                DbType = DbType.String.ToString()
            });

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Logger",
                Layout = "${logger}",
                DbType = DbType.String.ToString()
            });

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Callsite",
                Layout = "${callsite:filename=true}",
                DbType = DbType.String.ToString()
            });

            var exceptionLayout = new JsonLayout();
            exceptionLayout.Attributes.Add(new JsonAttribute("type", "${exception:format=Type}"));
            exceptionLayout.Attributes.Add(new JsonAttribute("message", "${exception:format=Message}"));
            exceptionLayout.Attributes.Add(new JsonAttribute("stacktrace", "${exception:format=StackTrace}"));
            exceptionLayout.Attributes.Add(new JsonAttribute("innerException", new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("type", "${exception:format=:innerFormat=Type:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                    new JsonAttribute("message", "${exception:format=:innerFormat=Message:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}"),
                    new JsonAttribute("stacktrace", "${exception:format=:innerFormat=StackTrace:MaxInnerExceptionLevel=1:InnerExceptionSeparator=}")
                },
                RenderEmptyObject = false
            }, false));

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Exception",
                Layout = exceptionLayout,
                DbType = DbType.String.ToString()
            });

            return databaseTarget;
        }

        private static ConsoleTarget AddConsoleTarget()
        {
            var consoleTarget = new ConsoleTarget
            {
                Name = "Console",
                Layout = "${date}|${message}|${exception:format=stackTrace}"
            };
            return consoleTarget;
        }
    }
}
