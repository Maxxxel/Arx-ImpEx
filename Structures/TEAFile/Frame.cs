using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class Frame
    {
        public int master_key_frame { get; internal set; }
        public long num_frame { get; internal set; }
        public bool f_rotate { get; internal set; }
        public bool f_translate { get; internal set; }
        public long flag { get; internal set; }
        public float time { get; internal set; }
        public Vector3 translate { get; set; }
        public Quaternion quat { get; set; }
        public long sample { get; set; }

        public Frame()
        {
        }
    }
}
