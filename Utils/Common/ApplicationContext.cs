
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utils.Common
{
    public class ApplicationContext
    {
    public ApplicationContext()
    {
        
    }
    public IConfiguration ApplicationConfiguration { get; set; }
    internal ILoggerFactory LoggerFactory {get; set;}
    public string CurrentClientId {get; set;}
    
    public LogWriter ApplicationLogger {
        get
        {
          lock(_syncLock)
          {
              if(_logWriter == null)
              {
                  _logWriter = new LogWriter(ApplicationConfiguration, LoggerFactory);
              }
              return _logWriter;
          }
        }
    }

    private LogWriter _logWriter;
    private object _syncLock = new object();
    
    }
}