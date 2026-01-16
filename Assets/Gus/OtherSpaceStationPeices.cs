using System;
using UnityEngine;
using System.Collections.Generic;
using Structual_Piece;
using ResourceMiner_piece;
using RootSpaceStationPeice;

namespace OtherSpaceStationPeices
{
    public class OtherSpaceStationPeices : MonoBehaviour
    {
        int i = 0; 
        public List<Item> pieces= new List<Item>() {Structual_Piece, ResourceMiner_piece};
        RootSpaceStationPeice rootPiece = new RootSpaceStationPeice();



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
            if(Input.GetKey(KeyCode.P))
            {
                //snap the piece to the root piece 
                // ALSO IN RootSpaceStationPeice.cs possible double unless changed
                rootPiece.pieces += 1;
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
