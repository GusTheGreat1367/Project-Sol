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
using OtherSpaceStationPieces; // change to actual

// SAVE/LOAD STATION PROCESS: Build the station -> save (Convert to JSON) -> load (Convert from JSON to station)

namespace JSON_SP
{
    class JSONSpaceStation
    {
        var Saves = new Dictionary<string, string>()
        {
            {"StarterStation", "StarterStation.json"}, 
            {"Station1", "spacestation1.json"}, //Rename "station1" to the actual name of the save file
            {"Station2", "spacestation2.json"}, //Rename "station2" to the actual name of the save file
            {"Station3", "spacestation3.json"}, //Rename "station3" to the actual name of the save file
            {"Station4", "spacestation4.json"}, //Rename "station4" to the actual name of the save file
            {"Station5", "spacestation5.json"} //Rename "station5" to the actual name of the save file
        };
        // MAX of 5 saved stations
        var station1 = new Dictionary<string, string>();
        var station2 = new Dictionary<string, string>();
        var station3 = new Dictionary<string, string>();
        var station4 = new Dictionary<string, string>();
        var station5 = new Dictionary<string, string>();

        
        void StarterStation()
        {
            var root = new Dictionary<string, object>();
            var SP = new Dictionary<string, string>();
            SP.Add("Left", "ResourceMiner");
            SP.Add("Right", "FoodModule");
            SP.Add("up", "Solar Panels");
            SP.Add("down", "Solar Panels");
            //change above's left/right to be the coordinates for the left/right areas and up/down to coordinates
            //Or just do l/r/u/d and in loadstation make them coordinates
            //station will look like ( the || are solar pannels):
            //      ||
            // [RM]:[]:[FM]
            //      ||
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
        void SaveStation(string stationNUM) // fix with coordinates 
        {
            var Root = new Dictionary<string, object>();
            var ST = new Dictionary<object, object>();
            //ST = OtherSpaceStationPieces.AttPieces; //change to actual name
            Root.Add("RootPiece", OtherSpaceStationPieces.AttPieces);//this might work and if so, yay! BC if the root is (0,0), the coordinates are relative to it
            Root.Add("Credits", 0);
            Root.Add("People", 50);
            // turn station into JSON file and write to <station_name>.json
            // save OtherSpaceStationPieces.AttPieces to stationNUM.json in json syntax

            var options = new JsonSerializerOptions { WriteIndented = true };
            var Station = JsonSerializer.Serialize(Root, options);
            string folderPath = @"./Assets/Gus/JSON";
            string fileName = $"{stationNUM}.json";
            string fullPath = Path.Combine(folderPath, fileName);        
            string content = Station;
            //copy content to stationNUM but the Dictionary for it
            File.WriteAllText(fullPath, content);
        }
        void LoadStation(string stationNUM)
        {
            foreach(var pie in stationNUM)// stationNUM is a copy of the JSON file ex. spacestation1 is the .json fila and a var
            { 
                string pieceXY = OtherSpaceStation.AttPieces[pie];
                char delimiter = ',';
                string[] coordinates = pieceXY.Split(delimiter);
                pieceX = coordinates(0);
                pieceY = coordinates(1);
                // save the piece with 
            }
            //Make the elements into a dictionary called APIECE with the piece then the coordinates
            foreach(var a in APIECE)
            {
                DrawConnector(APIECE);
            }
            
            // read <station_name>.json and turn into station

        }
    }
}
