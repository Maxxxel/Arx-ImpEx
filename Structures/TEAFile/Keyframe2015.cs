using Arx_Model_Exporter.Helpers;
using System.Numerics;
using System.Windows.Media.Animation;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class Keyframe2015
    {
        public int num_frame { get; private set; }
        public int flag_frame { get; private set; }
        public char[] info_frame { get; private set; }
        public bool master_key_frame { get; private set; }
        public bool key_frame { get; private set; }
        public bool key_move { get; private set; }
        public bool key_orient { get; private set; }
        public bool key_morph { get; private set; }
        public int time_frame { get; set; }
        public Quaternion quat { get; private set; }
        public Vector3 tkm { get; private set; }
        public GroupAnimation[] group_animations { get; private set; }
        public int num_samples { get; private set; }
        public int num_sfx { get; private set; }

        public Keyframe2015(FileWrapper file, int nb_groups)
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

            if (key_move)
            {
                tkm = file.ReadVector3();
            }

            if (key_orient)
            {
                // Ignored
                byte[] Useless = file.ReadBytes(-1, 8);

                quat = file.ReadArxQuaternion();
            }

            if (key_morph)
            {
                // Ignored
                byte[] Useless = file.ReadBytes(-1, 16);
            }

            group_animations = new GroupAnimation[nb_groups];

            for (int i = 0; i < nb_groups; i++)
            {
                group_animations[i] = new GroupAnimation(file);
            }

            num_samples = file.ReadInt();
            num_sfx = file.ReadInt();
        }
    }
}