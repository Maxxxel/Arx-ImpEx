using Arx_Model_Exporter.Helpers;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_SPRINGS
    {
        public short startidx { get; set; }
        public short endidx { get; set; }
        public float restlength { get; set; }
        public float constant { get; set; } // spring constant
        public float damping { get; set; }  // spring damping
        public long type { get; set; }

        public EERIE_SPRINGS(MemoryWrapper ftlFile)
        {
            startidx = ftlFile.ReadShort();
            endidx = ftlFile.ReadShort();
            restlength = ftlFile.ReadFloat();
            constant = ftlFile.ReadFloat();
            damping = ftlFile.ReadFloat();
            type = ftlFile.ReadInt64();
        }
    }
}