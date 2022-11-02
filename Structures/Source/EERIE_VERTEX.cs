using Arx_Model_Exporter.Helpers;
using System.Drawing;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_VERTEX
    {
        public D3DTLVERTEX vert { get; set; } // 32 bytes
        public Vector3 v { get; set; }
        public Vector3 norm { get; }

        public EERIE_VERTEX(MemoryWrapper ftlFile)
        {
            vert = new D3DTLVERTEX(ftlFile);
            v = ftlFile.ReadVector3();
            norm = ftlFile.ReadVector3();
        }
    }

    public class D3DTLVERTEX
    {
        public Vector3 v { get; set; }
        public float rhw { get; private set; }
        public Color color { get; set; }

        public D3DTLVERTEX(MemoryWrapper ftlFile)
        {
            v = ftlFile.ReadVector3(); // 12
            rhw = ftlFile.ReadFloat(); // 4
            color = ftlFile.ReadColor(); // 16
        }
    }
}