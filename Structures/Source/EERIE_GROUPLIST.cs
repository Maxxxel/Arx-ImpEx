using Arx_Model_Exporter.Helpers;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_GROUPLIST
    {
        public string name { get; }
        public long origin { get; }
        public long nb_index { get; }
        private long unknown { get; }
        public long[] indexes { get; set; }
        public float siz { get; }

        public EERIE_GROUPLIST(MemoryWrapper ftlFile)
        {
            char[] temp = ftlFile.ReadChars(-1, 256);
            name = new string("");

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '\0')
                {
                    name = new string(temp, 0, i);
                    break;
                }
            }

            origin = ftlFile.ReadInt64();
            nb_index = ftlFile.ReadInt64();
            unknown = ftlFile.ReadInt64();
            siz = ftlFile.ReadFloat();
        }
    }
}