using Arx_Model_Exporter.Helpers;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class EERIE_GROUP
    {
        public int key { get; set; }
        public Vector3 translate { get; set; }
        public Quaternion quat { get; set; }
        public Vector3 zoom { get; set; }
    }
}