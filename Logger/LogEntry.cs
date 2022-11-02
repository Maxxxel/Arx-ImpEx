using System.Collections.Generic;

namespace Arx_Model_Exporter.Logger
{
    public class LogEntry : PropertyChangedBase
    {
        public string DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }
}
