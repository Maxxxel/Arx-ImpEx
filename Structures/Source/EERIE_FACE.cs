using System.Numerics;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_FACE
    {
        public long facetype { get; internal set; }
        public short texid { get; internal set; }
        public float transval { get; internal set; }
        public float temp { get; internal set; }
        public Vector3 norm { get; internal set; }
        public Vector3[] nrmls { get; internal set; }
        public ushort[] vid { get; internal set; }
        public float[] u { get; internal set; }
        public float[] v { get; internal set; }
        public short[] ou { get; internal set; }
        public short[] ov { get; internal set; }

        public EERIE_FACE()
        {
            nrmls = new Vector3[3];
            vid = new ushort[3];
            u = new float[3];
            v = new float[3];
            ou = new short[3];
            ov = new short[3];
        }
    }
}