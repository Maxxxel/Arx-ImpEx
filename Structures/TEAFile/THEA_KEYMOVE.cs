using Arx_Model_Exporter.Helpers;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    internal class THEA_KEYMOVE
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        
        public THEA_KEYMOVE(FileWrapper file)
        {
            x = file.ReadFloat();
            y = file.ReadFloat();
            z = file.ReadFloat();
        }
    }
}