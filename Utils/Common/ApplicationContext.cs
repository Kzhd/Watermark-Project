
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
    
    }
}