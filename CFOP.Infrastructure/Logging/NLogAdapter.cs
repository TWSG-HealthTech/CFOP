using NLog;

namespace CFOP.Infrastructure.Logging
{
    public class NLogAdapter : ILogger
    {
        private static readonly Logger _logger = LogManager.GetLogger("CFOPLogger");

        public void Info(string message)
        {
            _logger.Info(message);
        }
    }
}
