using UnityEngine;

public class OtherSpaceStationPeices : MonoBehaviour
{
    class Pieces//each peice will be a struct in this class
    {
        struct Structual_piece
        { //make a struct like this for each peice
            public Script StructualPiece;
            public int X;
            public int Y;
            public int R; //rotation
        }
        struct ResourceMiner_piece
        {
            public Script ResourceMinerPiece;
            public int X;
            public int Y;
            public int R; //rotation
        }
    }
    public void PeiceSelection(Pieces piece){

        void OnMouseDown(Pieces.Structual_piece peice)
        {
            SelectedPiece = Pieces.Structual_piece;
            Piece = SelectedPiece;//makes it so Piece is a clone of selectedPeice. 
//ex: Pieces.Structual_piece -> SelectedPiece -> Piece so Pieces.Structual_piece is not directly affected.
        }
    }
    
}
