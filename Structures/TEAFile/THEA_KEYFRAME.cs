using Arx_Model_Exporter.Helpers;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    internal class THEA_KEYFRAME
    {
        public int num_frame;
        public int flag_frame;
        public bool master_key_frame;
        public bool key_frame; //image clef
        public bool key_move;
        public bool key_orient;
        public bool key_morph;
        public int time_frame;
        
        public THEA_KEYFRAME(FileWrapper file)
        {
            num_frame = file.ReadInt();
            flag_frame = file.ReadInt();
            master_key_frame = file.ReadBool();
            key_frame = file.ReadBool();
            key_move = file.ReadBool();
            key_orient = file.ReadBool();
            key_morph = file.ReadBool();
            time_frame = file.ReadInt();
        }
    }
}