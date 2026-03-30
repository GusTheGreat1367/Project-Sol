/*
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using Station;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using SpaceStationPeices;
using RootSpaceStationPeice;

// SAVE/LOAD STATION PROCESS: Build the station -> save (Convert to JSON) -> load (Convert from JSON to station)
// I might be cooked, I need to find a method for drawing the piece connecter, requiring coordinates and another
// save method. 
//A LOT OF ERRORS

namespace JSON_SP
{
    class JSONSpaceStation
    {
        var Saves = new Dictionary<string, string>();
        Saves.Add("StarterStation", "StarterStation.json");
        Saves.Add("Station1", "spacestation1.json"); //Rename "station1" to the actual name of the save file
        Saves.Add("Station2", "spacestation2.json"); //Rename "station2" to the actual name of the save file
        Saves.Add("Station3", "spacestation3.json"); //Rename "station3" to the actual name of the save file
        Saves.Add("Station4", "spacestation4.json"); //Rename "station4" to the actual name of the save file
        Saves.Add("Station5", "spacestation5.json"); //Rename "station5" to the actual name of the save file
        // MAX of 5 saved stations

        
        void StarterStation()
        {
            var root = new Dictionary<string, object>();
            var SP = new Dictionary<string, string>();
            SP.Add("Left", "ResourceMiner");
            SP.Add("Right", "FoodModule");
            
            root.Add("RootPiece", SP);
            root.Add("Credits", 0);
            root.Add("People", 50);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            var StarterStation = JsonSerializer.Serialize(root, options);

        
            string folderPath = @"./Assets/Gus/JSON";
            string fileName = "StarterStation.json";
            string fullPath = Path.Combine(folderPath, fileName);
            
            string content = StarterStation;
            File.WriteAllText(fullPath, content);
        }
        void SaveStation()
        {
            var Root = new Dictionary<string, object>();
            // turn station into JSON file and write to <station_name>.json
            var station = new Dictionary<string, string>();
            station.Add("Right", OtherSpaceStationPeices.AttPieces.Right);
            station.Add("Left", OtherSpaceStationPeices.AttPieces.Left);
            station.Add("Top", OtherSpaceStationPeices.AttPieces.Top);
            station.Add("Bottom", OtherSpaceStationPeices.AttPieces.Bottom);
            Root.Add("Root", station);
            //Instead of sides, record coordanites for the connecter
            // FOR THE COORDINATES FOR SAVING:
            // Make a string of the coordinates as a variable ex: string pieceXY = "014,098";
            // THEN you do a: char delimiter = ',';
            // and then split it by a comma: string[] coordinates = input.Split(delimiter);
            // then a simple pieceX = coordinates(0) and pieceY = coordinates(1)
            // This can be in the decoding area (LoadStation) 
            // then we just draw the line from the root to the final piece position

            // save to a station# file

            //Then find if the top/bottom/left/right piece has anything attached to it and if so make that a 
            // new dictionary and in the end add the attached piece the that piece's l/r/t/b side
            // then make that the OtherSpaceStationPeices.AttPieces.[side] (a new var like OSSPAP.[side])
            // and add the actual station.add()s to the bottom, after the steps above so you can do 
            // station.add("Right", OSSPAP.Right);
            // if OSSAP.Right is null, do a check for it.
            //final code for the process:
 // station.add("Right", (OSSPAP.Right == null) ? OtherSpaceStationPeices.AttPieces.Right : OSSPAP.Right);
            
            /*
            Example if top is not null and it is a fictional piece named  "Laser piece":
            "Root" = [
                "left" = "ResourceMiner",
                "right" = "FoodModule",
                "top" = "Laser piece" = [
                    "left" = "DockingPort",
                    "right" = "TradingPost",
                ]
                "bottom" = "AnotherPiece"
            ]
            
            
        }
        void LoadStation(string stationNUM)
        {
            //Make the elements into a list called APIECE
            foreach(var a in APIECE)
            {
                DrawConnector(APIECE);
            }
            
            // read <station_name>.json and turn into station
        }
    }
}
*/
