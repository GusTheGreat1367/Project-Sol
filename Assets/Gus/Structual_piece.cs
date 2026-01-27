using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;

namespace Structual_Piece //This entire namespace works for all of the pieces, just change the Activation()
//me personally I would make a generic space station part class and then change stats in 
//that class, such as having a intake materials variable and then having a output materials
//variable, but thats just how I would do it. You don't need to do it that way as long as it works
{
    public class Structual_piece : MonoBehaviour
    {
        public string type = "connector"; // no function 
        int X = 0;
        int Y = 0;
        int R = 0;

        void update()
        {
            /* if (X > canvas_max_X || X < canvas_min_X || Y > canvas_max_Y || Y < canvas_min_Y)
            {
                X = 0;
                Y = 0;
            }
            */
            if(OtherSpaceStationPeices.SelectedPiece == "Structual_Piece")
            {
                X = OtherSpaceStationPeices.SelectedPiece.X;
                Y = OtherSpaceStationPeices.SelectedPiece.Y;
                R = OtherSpaceStationPeices.SelectedPiece.R;
            }
        }
    }
}
