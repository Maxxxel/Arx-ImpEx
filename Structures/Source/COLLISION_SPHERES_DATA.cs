namespace Arx_Model_Exporter.Structures.EERIE
{
    public class COLLISION_SPHERES_DATA
    {
        public long nb_spheres { get; set; }
        public COLLISION_SPHERE[] spheres { get; set; }

        public COLLISION_SPHERES_DATA()
        {
        }
    }

    public class COLLISION_SPHERE
    {
        public short idx { get; set; }
        public short flags { get; set; }
        public float radius { get; set; }

        public COLLISION_SPHERE(Helpers.MemoryWrapper ftlFile)
        {
            idx = ftlFile.ReadShort();
            flags = ftlFile.ReadShort();
            radius = ftlFile.ReadFloat();
        }
    }
}