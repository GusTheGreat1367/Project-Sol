using UnityEngine;
using System.Collections.Generic;
using Structual_piece;
using ResourceMiner_piece;
using JSONspacestation;
using DrawConnecter;

//if money >= 1000 and resources >= 500 then proceed and subtract the resources and money
namespace SpaceStation
{
    public class SpaceStation : MonoBehaviour // add to unity project
    {
        public int credits = 0;
        public int people = 50;
        var resources = new Dictionary<string, int>();
        resources.Add("O2", 0); //oxygen
        resources.Add("Ore", 0); //ore -> metals 
        resources.Add("H2O", 0); //water
        resources.Add("Carbon", 0); //carbon
        resources.Add("Food", 0); //food
        void createstation()
        { 
            //change script names and function names to actual names
            // DrawConnecter.cs had a function that draws the connecter (drawConnecter()) and JSONsaving.cs just saves the 
            //positions of each piece if it isn't directly attached
            JSONspacestation.loadstation();
            DrawConnecter.drawConnecter();
        }

        // Below is the canvas size, change null to agreed upon values
        public int canvas_max_X = null;
        public int canvas_min_X = null;
        public int canvas_max_Y = null;
        public int canvas_min_Y = null;
        public void Update()
        {
            foreach (var p in OtherSpaceStationPeices.AttPieces)
            {
                if (p.type == "functional")
                {
                    p.Activation();
                }
            }
            if (resources["Food"] == 100) // refines food into people
            {
                while (people < 100)
                {
                    resources["Food"] -= 100;
                    people += 5;
                }
            }
        }
    } 
}
