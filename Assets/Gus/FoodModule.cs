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
        public int UpCost = 2500;
        public int level = 1;
        public var time = new Dictionary<int, int> ();
        time.Add(1, 5000); // you could easily automate this just by using a for loop
        time.Add(2, 4500);
        time.Add(3, 4000);
        time.Add(4, 3500);
        time.Add(5, 3000);
        time.Add(6, 2500);
        time.Add(7, 2000);
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
            // The level influences the number of resources needed to refine into 50 food
            if ((SpaceStation.resources["Carbon"] >= 100) && (SpaceStation.resources["H2O"] >= 100)) // refines Carbon and H2O into Food
            {
                SpaceStation.resources["Carbon"] -= 100;
                SpaceStation.resources["H2O"] -= 100;
                SpaceStation.resources["Food"] += 50;
            }
        }
        void upgrade() // in space station.cs 
        {
            if (time[level] < 7)
            {
                if (SpaceStation.credits >= UpCost)
                {
                    SpaceStation.credits -= UpCost;
                    level += 1;
                    UpCost += 2500;
                }
            }
        }        
    }
}
