using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using OtherSpaceStationPeices;

namespace RootSpaceStationPeice
{
    public class Root //the only reason we'd need this is to link the pieces together
    {
        public int X;
        public int Y;
        public int R;
    }


    public class RootSpaceStationPeice : MonoBehaviour
    {
        private int pieces = 0;
        
        // Added required fields that were missing
        private int X;
        private int Y;
        private int Z;
        private int R;

        private class FadingMessage
        {
            public string text;
            public int size;
            public Rect rect;
            public float timer;
        }

        private List<FadingMessage> activeMessages = new List<FadingMessage>();

        void Start()
        {
            X = 0;
            Y = 0;
            Z = 0;
            R = 0;
        }

        void Disp(string msg, int txtsize, int x, int y)//display a message the the screen
        {
            
            // Prevent duplicate message spamming
            if (activeMessages.Count > 0 && activeMessages[activeMessages.Count - 1].text == msg)
            {
                return;
            }

            activeMessages.Add(new FadingMessage
            {
                text = msg,
                size = txtsize,
                rect = new Rect(x + 10, y + 10, 500, 100), // Offset slightly and give width
                timer = 2.5f
            });
        }

        void OnGUI()
        {
            for (int i = activeMessages.Count - 1; i >= 0; i--)
            {
                var msg = activeMessages[i];
                msg.timer -= Time.deltaTime;

                if (msg.timer <= 0)
                {
                    activeMessages.RemoveAt(i);
                    continue;
                }

                GUIStyle style = new GUIStyle();
                style.fontSize = msg.size;
                // Fade out in the last 1 second
                float alpha = Mathf.Clamp01(msg.timer);
                style.normal.textColor = new Color(1, 1, 1, alpha);
                
                GUI.Label(msg.rect, msg.text, style);
            }
        }

        void Update()
        {
            if (X != 0 || Y != 0 || R != 0)
            {
                X = 0;
                Y = 0;
                Z = 0;
            }
            if(Input.GetKey(KeyCode.P))
            {
                //snap the piece
                pieces += 1;
            }
            
            // Commenting out this check as OtherSpaceStationPeices.SelectedPiece is not accessible staticly
            // if (OtherSpaceStationPeices.OtherSpaceStationPeices.SelectedPiece /*Is touching root*/)
            // {
            //    pieces += 1;
                //detect where the piece is touching and append it to that area.
            // }

            if (pieces == 0)
            {
                Disp("Attach a piece to the root piece to get started!", 15, 0, 5);
            }
            if (pieces == 64) // to prevent crashing the game and messing up storage later update to a better #
            {
                Disp("Error, you cannot have more than 64 pieces on the space station at a time", 20, 0, 0);
                enabled = false; // Stop the script instead of 'break'
            }
        }
    }
}
