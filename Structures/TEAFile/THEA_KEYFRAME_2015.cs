using Arx_Model_Exporter.Helpers;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class THEA_KEYFRAME_2015
    {
        public int num_frame;
        public int flag_frame;
        public char[] info_frame;
        public bool master_key_frame;
        public bool key_frame; //image clef
        public bool key_move;
        public bool key_orient;
        public bool key_morph;
        public int time_frame;
        internal Quaternion quat;

        public THEA_KEYFRAME_2015(FileWrapper file)
        {
            num_frame = file.ReadInt();
            flag_frame = file.ReadInt();
            info_frame = file.ReadChars(-1, 256);
            master_key_frame = file.ReadBool();
            key_frame = file.ReadBool();
            key_move = file.ReadBool();
            key_orient = file.ReadBool();
            key_morph = file.ReadBool();
            time_frame = file.ReadInt();
        }

        public Vector3 translate { get; internal set; }
        public THEO_GROUPANIM[] groups { get; internal set; }
    }
}