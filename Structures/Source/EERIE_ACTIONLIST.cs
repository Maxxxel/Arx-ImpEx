using Arx_Model_Exporter.Helpers;
using System.Text;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_ACTIONLIST
    {
        public string name { get; }
        public long idx { get; }
        public int act { get; }
        public int sfx { get; }


        public EERIE_ACTIONLIST(MemoryWrapper ftlFile)
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

            idx = ftlFile.ReadInt64();
            act = ftlFile.ReadInt();
            sfx = ftlFile.ReadInt();
        }
    }
}