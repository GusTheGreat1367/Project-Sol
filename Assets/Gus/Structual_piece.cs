using UnityEngine;
using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;

namespace Structual_Piece //The piece is a 90 degree connector - block straight-line and replace with straight-line-90 degree connector
//Make another piece that is just a strightline connecter.
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
