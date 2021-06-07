using Dapper;
using gpconnect_analytics.DAL;
using gpconnect_analytics.DTO.Response.Configuration;
using gpconnect_analytics.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace gpconnect_analytics.Configuration.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static ILoggingBuilder ConfigureLoggingServices(ILoggingBuilder loggingBuilder, IConfiguration configuration)
        {
            var nLogConfiguration = new NLog.Config.LoggingConfiguration();

            var consoleTarget = AddConsoleTarget();
            var databaseTarget = AddDatabaseTarget(configuration);
            var mailTarget = AddMailTarget(configuration);

            nLogConfiguration.Variables.Add("applicationVersion", ApplicationHelper.ApplicationVersion.GetAssemblyVersion());

            nLogConfiguration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);
            nLogConfiguration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, databaseTarget);
            nLogConfiguration.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, mailTarget);

            nLogConfiguration.AddTarget(consoleTarget);
            nLogConfiguration.AddTarget(databaseTarget);
            nLogConfiguration.AddTarget(mailTarget);

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

        private static MailTarget AddMailTarget(IConfiguration configuration)
        {
            var emailConfiguration = GetEmailConfiguration(configuration);
            var mailTarget = new MailTarget
            {
                Name = "Mail",
                Html = false,
                SmtpServer = emailConfiguration.Hostname,
                SmtpAuthentication = emailConfiguration.AuthenticationRequired ? SmtpAuthenticationMode.Basic : SmtpAuthenticationMode.None,
                SmtpUserName = emailConfiguration.Username,
                SmtpPort = emailConfiguration.Port,
                SmtpPassword = emailConfiguration.Password,
                To = emailConfiguration.RecipientAddress,
                From = emailConfiguration.SenderAddress,
                Body = GetExceptionLayout(),
                Subject = emailConfiguration.DefaultSubject,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
            };
            return mailTarget;
        }

        private static JsonLayout GetExceptionLayout()
        {
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
            return exceptionLayout;
        }

        private static Email GetEmailConfiguration(IConfiguration configuration)
        {
            using (var sqlConnection = new SqlConnection(configuration.GetConnectionString(ConnectionStrings.GpConnectAnalytics)))
            {
                var result = sqlConnection.Query<Email>("[Configuration].[GetEmailConfiguration]", commandType: CommandType.StoredProcedure);
                return result.FirstOrDefault();
            }
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
                Layout = "${var:applicationVersion}",
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

            databaseTarget.Parameters.Add(new DatabaseParameterInfo
            {
                Name = "@Exception",
                Layout = GetExceptionLayout(),
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
