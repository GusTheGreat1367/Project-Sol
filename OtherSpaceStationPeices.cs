using UnityEngine;

public class OtherSpaceStationPeices : MonoBehaviour
{
    class Peices//each peice will be a struct in this class
    {
        struct Structual_peice
        { //make a struct like this for each peice
            public Script StructualPeice;
            public int X;
            public int Y;
            public int R; //rotation
        }
        struct ResourceMiner_peice
        {
            public Script ResourceMinerPeice;
            public int X;
            public int Y;
            public int R; //rotation
        }
    }
    public void PeiceSelection(Peices peice){
        
        void OnMouseDown()
        {
            //make it select a peice and clone it and communicate to RootSpaceStationPeice
            //make it do it once a time
        }
    }
    
}
