using Arx_Model_Exporter.Helpers;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class THEO_GROUPANIM
    {
        public bool key_group;
        public byte[] angle;
        public Quaternion Quaternion; // ArxQuat
        public Vector3 translate;
        public Vector3 zoom;
        
        public THEO_GROUPANIM(FileWrapper file)
        {
            key_group = file.ReadBool();
            angle = file.ReadBytes(-1, 8);
            Quaternion = file.ReadArxQuaternion();
            translate = file.ReadVector3();
            zoom = file.ReadVector3();
        }
    }
}