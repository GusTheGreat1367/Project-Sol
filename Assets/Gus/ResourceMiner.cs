using UnityEngine;
using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;

namespace ResourceMiner_piece //This entire namespace works for all of the pieces, just change the Activation()
{
    public class ResourceMiner_piece : MonoBehaviour
    {
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
            if(OtherSpaceStationPeices.SelectedPiece == "ResourceMiner_piece")
            {
                X = OtherSpaceStationPeices.SelectedPiece.X;
                Y = OtherSpaceStationPeices.SelectedPiece.Y;
                R = OtherSpaceStationPeices.SelectedPiece.R;
            }
        }
        void Activation()//on build.cs or RootSpaceStation.cs call Structual_piece.Activation(); to start the function
        {
            //control for the piece
            
        }
    }
}