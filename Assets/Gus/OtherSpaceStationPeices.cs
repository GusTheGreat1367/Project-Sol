using System;
using UnityEngine;
using System.Collections.Generic;
using Structual_Piece;
using ResourceMiner_piece;
using FoodModule;

namespace OtherSpaceStationPeices
{
    public class OtherSpaceStationPeices : MonoBehaviour
    {
        int i = 0; 
        public List<Type> pieces = new List<Type>() {Structual_Piece.Structual_piece, ResourceMiner_piece.ResourceMiner_piece, FoodModule.FoodModule};
        public var AttPieces = new Dictionary<Item, Item>(); 
        //Ex: AttPieces.Add(Right, (selectedPiece.side == "Right") ? selectedPiece : null); <- valid syntax


        void Update()
        {
            while (i < pieces.Count)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    var SelectedPiece = pieces[i];
                    i += 1;
                    if (i > pieces.Count) // might not work bc the for loop might have already exited
                    {
                        i = 0;
                    }
                }
            }
            //use mouse to move pieces
            //use button to end build
            if(Input.GetKey(KeyCode.P))
            {
                //place the piece
                // get the side of the root piece that the piece is touching
                // THIS DOESN'T WORK BECAUSE NOW YOU CAN PLACE THE PIECE WHERE EVER!
                rootPiece.pieces += 1;
                AttPieces.Add(Right, (selectedPiece.side == "Right") ? selectedPiece : null);
                AttPieces.Add(Left, (selectedPiece.side == "Left") ? selectedPiece : null);
                AttPieces.Add(Top, (selectedPiece.side == "Top") ? selectedPiece : null);
                AttPieces.Add(Bottom, (selectedPiece.side == "Bottom") ? selectedPiece : null);
            }
            if (Input.GetKey(KeyCode.R))
            {
                SelectedPiece.R += 90; 

            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                SelectedPiece.X += 1;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                SelectedPiece.X -= 1;
            }
            if(Input.GetKey(KeyCode.UpArrow))
            {
                SelectedPiece.Y += 1;
            }
            if(Input.GetKey(KeyCode.DownArrow))
            {
                SelectedPiece.Y -= 1;   
            }
        }
    }
}
