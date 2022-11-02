using Arx_Model_Exporter.Helpers;
using System;
using System.Numerics;

namespace Arx_Model_Exporter.Structures.EERIE
{
    public class CLOTHESVERTEX
    {
        public short idx { get; set; }
        public char flags { get; set; }
        public char coll { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 velocity { get; set; }
        public Vector3 force { get; set; }
        public float mass { get; set; } // 1.f/mass
        public Vector3 t_pos { get; set; }
        public Vector3 t_velocity { get; set; }
        public Vector3 t_force { get; set; }
        public Vector3 lastpos { get; set; }

        public CLOTHESVERTEX(MemoryWrapper ftlFile)
        {
            idx = ftlFile.ReadShort();
            flags = ftlFile.ReadChar();
            coll = ftlFile.ReadChar();
            pos = ftlFile.ReadVector3();
            velocity = ftlFile.ReadVector3();
            force = ftlFile.ReadVector3();
            mass = ftlFile.ReadFloat();
            t_pos = ftlFile.ReadVector3();
            t_velocity = ftlFile.ReadVector3();
            t_force = ftlFile.ReadVector3();
            lastpos = ftlFile.ReadVector3();
        }
    }
}