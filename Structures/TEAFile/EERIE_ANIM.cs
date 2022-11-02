namespace Arx_Model_Exporter.Structures.TEAFile
{
    public class EERIE_ANIM
    {
        public EERIE_ANIM()
        {
        }

        public long nb_groups { get; internal set; }
        public long nb_key_frames { get; internal set; }
        public EERIE_FRAME[] frames { get; internal set; }
        public EERIE_GROUP[] groups { get; internal set; }
        public byte[] voidgroups { get; internal set; }
        public float anim_time { get; internal set; }
        public string anim_name { get; internal set; }
        public THEA_KEYFRAME_2015[] tkf2015s { get; internal set; }
    }
}