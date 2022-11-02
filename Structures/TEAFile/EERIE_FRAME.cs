using Arx_Model_Exporter.Helpers;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class EERIE_FRAME
    {
        public int num_frame { get; set; }
        public int flag { get; set; }
        public int master_key_frame { get; set; }
        public short f_translate { get; set; } //int
        public short f_rotate { get; set; } //int
        public float time { get; set; }
        public Vector3 translate { get; set; }
        public Quaternion quat { get; set; }
        public int sample { get; set; }

        public EERIE_FRAME(FileWrapper file)
        {
            num_frame = file.ReadInt();
            flag = file.ReadInt();
            master_key_frame = file.ReadInt();
            f_translate = file.ReadShort();
            f_rotate = file.ReadShort();
            time = file.ReadFloat();
            translate = file.ReadVector3();
            quat = file.ReadQuaternion();
            sample = file.ReadInt();
        }

        public EERIE_FRAME()
        {
        }
    }
}