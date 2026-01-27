using UnityEngine;
using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;
using System;
using System.Threading;

namespace FoodModule
{
    public class FoodModule : MonoBehaviour
    {
        public string type = "functional";
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
            if(OtherSpaceStationPeices.SelectedPiece == "FoodModule")
            {
                X = OtherSpaceStationPeices.SelectedPiece.X;
                Y = OtherSpaceStationPeices.SelectedPiece.Y;
                R = OtherSpaceStationPeices.SelectedPiece.R;
            }
        }
        void Activation()//on build.cs or RootSpaceStation.cs call Structual_piece.Activation(); to start the function
        {
            //control for the piece
            if ((SpaceStation.resources["Carbon"] == 100) && (SpaceStation.resources["H2O"] == 100)) // refines O2 && Carbon into food 
            {                                                                                        // That will help create more people 
                SpaceStation.resources["Carbon"] -= 50;
                SpaceStation.resources["H2O"] -= 50;
                SpaceStation.resources["Food"] += 50;
            }
        }
    }
}
