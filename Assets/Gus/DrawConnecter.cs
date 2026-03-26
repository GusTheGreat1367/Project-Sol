/*
using UnityEngine;
using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;
using RootSpaceStationPeice;

namespace DrawConnecter
{
    public class DrawConnecter : MonoBehaviour
    {
        public string conncecterSidePiece = "./Assets/Gus/ConnecterSidePath.obj"; // Might be useless
        public string connecterLengthPiece = "./Assets/Gus/ConnecterLengthPath.obj"; 
        public RootPiece rootPiece = OtherSpaceStationPeices.AttPieces.RootPiece;
        public OtherSpaceStationPeices.AttPieces.AttachedPiece placedPiece = OtherSpaceStationPeices.AttPieces.AttachedPiece;

 // Ex: "[" is the connecter side piece then you have to rotate it 180 degrees to get from the root piece to the other piece that's being connected
// Ex: the amount of ":" pieces to make the connection length - connecter length piece. 
//The connecter piece kinda has a end piece if we add like 1 more length then needed so it's hidden in the piece
//Total example: [::::::::] then rotated to connect root to the attached piece


        void DrawConnector()
        {
            
        }

        
    }
}
*/
using UnityEngine;
using System.Collections.Generic;
using SpaceStation;
using OtherSpaceStationPeices;
using RootSpaceStationPeice;

namespace DrawConnecter
{
    public class DrawConnecter : MonoBehaviour
    {
        public string connecterLengthPiece = "./Assets/Gus/ConnecterLengthPath.obj";
        public RootPiece rootPiece = OtherSpaceStationPeices.AttPieces.RootPiece;
        public OtherSpaceStationPeices.AttPieces.AttachedPiece placedPiece = OtherSpaceStationPeices.AttPieces.AttachedPiece;

        public GameObject connectorLengthPrefab; // The ":" filler pieces
        public float pieceLengthUnits = 1.0f;    // How long each ":" piece is in world units

        private List<GameObject> spawnedPieces = new List<GameObject>();

        // Ex: the amount of ":" pieces to make the connection length - connecter length piece.
        // The connecter piece has 1 more length than needed so it's hidden inside the attached piece
        // Total example: :::::::::

        void DrawConnector()
        {
            // --- 1. Get world positions of the root and attached pieces ---
            Vector3 startPos = rootPiece.transform.position;
            Vector3 endPos = placedPiece.transform.position;

            // --- 2. Calculate direction and total distance ---
            Vector3 direction = endPos - startPos;
            float totalDistance = direction.magnitude;
            Vector3 normalizedDir = direction.normalized;

            // --- 3. Figure out how many ":" length pieces are needed (+1 extra so the end is hidden inside the attached piece) ---
            int lengthPieceCount = Mathf.RoundToInt(totalDistance / pieceLengthUnits) + 1;

            // --- 4. Calculate rotation so pieces face from root to attached ---
            Quaternion connectorRotation = Quaternion.LookRotation(normalizedDir);

            // --- 5. Clear any previously drawn connector pieces ---
            ClearConnector();

            // --- 6. Spawn each ":" length piece along the path ---
            for (int i = 0; i < lengthPieceCount; i++)
            {
                float offset = (i * pieceLengthUnits) + (pieceLengthUnits / 2f);
                Vector3 piecePos = startPos + normalizedDir * offset;
                SpawnPiece(connectorLengthPrefab, piecePos, connectorRotation);
            }
        }

        // Instantiates a piece, tracks it, and parents it to this GameObject for easy cleanup
        private void SpawnPiece(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogWarning("DrawConnecter: connectorLengthPrefab is not assigned in the Inspector.");
                return;
            }

            GameObject piece = Instantiate(prefab, position, rotation);
            piece.transform.SetParent(this.transform);
            spawnedPieces.Add(piece);
        }

        // Destroys all previously spawned connector pieces so we can redraw cleanly
        public void ClearConnector()
        {
            foreach (GameObject piece in spawnedPieces)
            {
                if (piece != null)
                    Destroy(piece);
            }
            spawnedPieces.Clear();
        }
    }
}