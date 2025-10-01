using UnityEngine;


public class RootSpaceStationPeice : MonoBehaviour
{
    public int X;
    public int Y;
    public Script OtherSpaceStationPeices; //have OtherSpaceStationPeices be a link to all peices
    //OtherSpaceStationPeice will be a function that does the bundling and returns the selected peice
    OtherSpaceStationPeices OtherSpaceStationPeice;
    public Sprite me;
    

    void Start() {
        X = 0;
        Y = 0;
    }
    public void OnCollision(){
        if (OtherSpaceStationPeice.X == X && OtherSpaceStationPeice.Y == Y)
        {
            me = me += OtherSpaceStationPeice; // selected object becomes OtherSpaceStationPeice
            //wait no this doesnt work, AHHHHHHHHHH. 
            //make it so if their touching, not on top of each other!!!!!
            
        }
    }

}