using System;

namespace CFOP.Infrastructure.Logging
{
    public interface ILogger
    {
        void Info(string message);
        void Fatal(Exception ex, string message);
    }
}
