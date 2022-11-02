using Arx_Model_Exporter.Helpers;
using System.Drawing;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_FACE_FTL
    {
        public long facetype { get; }  // 0 = flat  1 = text  2 = Double-Side
        public Color rgb { get; }
        public ushort[] vid { get; }
        public short texid { get; }
        public float[] u { get; }
        public float[] v { get; }
        public short[] ou { get; }
        public short[] ov { get; }
        public float transval { get; }
        public Vector3 norm { get; }
        public Vector3[] nrmls { get; }
        public float temp { get; }

        public EERIE_FACE_FTL(MemoryWrapper ftlFile)
        {
            vid = new ushort[3];
            u = new float[3];
            v = new float[3];
            ou = new short[3];
            ov = new short[3];
            nrmls = new Vector3[3];

            facetype = ftlFile.ReadInt64();
            rgb = ftlFile.ReadColor(true);
            for (int i = 0; i < 3; i++) vid[i] = ftlFile.ReadUShort();
            texid = ftlFile.ReadShort();
            for (int i = 0; i < 3; i++) u[i] = ftlFile.ReadFloat();
            for (int i = 0; i < 3; i++) v[i] = ftlFile.ReadFloat();
            for (int i = 0; i < 3; i++) ou[i] = ftlFile.ReadShort();
            for (int i = 0; i < 3; i++) ov[i] = ftlFile.ReadShort();
            transval = ftlFile.ReadFloat();
            norm = ftlFile.ReadVector3();
            for (int i = 0; i < 3; i++) nrmls[i] = ftlFile.ReadVector3();
            temp = ftlFile.ReadFloat();
        }
    }
}