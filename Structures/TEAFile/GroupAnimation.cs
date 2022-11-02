using Arx_Model_Exporter.Helpers;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class GroupAnimation
    {
        public bool key_group { get; set; }
        public char[] angle { get; set; }
        public Quaternion quaternion { get; set; }
        public Vector3 translate { get; set; }
        public Vector3 zoom { get; set; }

        public GroupAnimation(FileWrapper file)
        {
            key_group = file.ReadBool();
            angle = file.ReadChars(-1, 8);
            quaternion = file.ReadArxQuaternion();
            translate = file.ReadVector3();
            zoom = file.ReadVector3();
        }

        public GroupAnimation()
        {
        }
    }
}