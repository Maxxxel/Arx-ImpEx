namespace Arx_Model_Exporter.Structures.EERIE
{
    public class CLOTHES_DATA
    {
        public short nb_cvert { get; internal set; }
        public short nb_springs { get; internal set; }
        public CLOTHESVERTEX[] cvert { get; set; }
        public CLOTHESVERTEX[] backup { get; set; }
        public EERIE_SPRINGS[] springs { get; set; }

        public CLOTHES_DATA()
        {
        }
    }
}