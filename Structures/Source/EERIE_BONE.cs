using System.Numerics;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class EERIE_BONE
    {
        internal int bindID;

        public long nb_idxvertices { get; set; }
        public long[] idxvertices { get; set; }
        public EERIE_GROUPLIST original_group { get; set; }
        public long father { get; set; }
        public Quaternion quatanim { get; set; }
        public Vector3 transanim { get; set; }
        public Vector3 scaleanim { get; set; }
        public Quaternion quatlast { get; set; }
        public Vector3 translast { get; set; }
        public Vector3 scalelast { get; set; }
        public Quaternion quatinit { get; set; }
        public Vector3 transinit { get; set; }
        public Vector3 scaleinit { get; set; }
        public Vector3 transinit_global { get; set; }

        public EERIE_BONE()
        {
        }
    }
}