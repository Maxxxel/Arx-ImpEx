using Arx_Model_Exporter.Helpers;
using System.Text;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_SELECTIONS
    {
        public char[] name { get; }
        public long nb_selected { get; }
        private long unknown { get; }
        public long[] selected { get; set; }

        public EERIE_SELECTIONS(MemoryWrapper ftlFile)
        {
            name = ftlFile.ReadChars(-1, 64);
            nb_selected = ftlFile.ReadInt64();
            unknown = ftlFile.ReadInt64();
        }
    }
}