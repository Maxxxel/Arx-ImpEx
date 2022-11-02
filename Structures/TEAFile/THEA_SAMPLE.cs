using Arx_Model_Exporter.Helpers;
using System.Xml.Linq;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class THEA_SAMPLE
    {
        public string sample_name { get; set; }
        public int sample_size { get; set; }
        private byte[] sample_data { get; set; }
        public THEA_SAMPLE(FileWrapper file)
        {
            char[] temp = file.ReadChars(-1, 256);
            sample_name = new string("");

            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == '\0')
                {
                    sample_name = new string(temp, 0, i);
                    break;
                }
            }

            sample_size = file.ReadInt();
            sample_data = file.ReadBytes(-1, sample_size);
        }
    }
}