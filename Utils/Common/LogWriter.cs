using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Events;

namespace Utils.Common
{
    public class LogWriter
    {

        private Logger _appErrorLogger;
        private readonly IConfiguration _configuration;
        public string ClientNo { get; set; }
        public Guid ClientId { get; set; }

        public LogWriter(IConfiguration configuration, ILoggerFactory logFactory, string clientNo, Guid clientId) : this(configuration, logFactory)
        {
            ClientNo = clientNo ?? throw new ArgumentNullException(nameof(clientNo));
            ClientId = clientId;
        }

        public LogWriter(IConfiguration configuration, ILoggerFactory logFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (logFactory == null)
            {
                logFactory = new LoggerFactory();
            }
            Initialize(configuration, logFactory);
        }

        private void Initialize(IConfiguration configuration, ILoggerFactory logFactory)
        {
            string applicationErrorLogFile;
            string logDirectory;
            try
            {
                logDirectory = configuration["LumenLog:LogDirectory"];
                applicationErrorLogFile = Path.Combine(logDirectory,"error_log_{Date}.log");
            }
            catch (Exception)
            {
                throw;
            }

            try
            {
              if(_appErrorLogger ==null)
              {
                  _appErrorLogger = new LoggerConfiguration()
                  .Enrich.WithExceptionDetails()
                  .Enrich.FromLogContext()
                  .Enrich.WithProperty("ApplicationName",_configuration["ApplicationName"])
                  .WriteTo.RollingFile(pathFormat: applicationErrorLogFile, buffered: false, restrictedToMinimumLevel: LogEventLevel.Debug)
                  .CreateLogger();
              }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void LogInformation(string messageTemplate, params object[] args)
        {
            _appErrorLogger.Information(messageTemplate,args);
        }

        public void LogError(Exception exception, string messageTemplate)
        {
            _appErrorLogger.Error(exception, messageTemplate);
        }
    }
}