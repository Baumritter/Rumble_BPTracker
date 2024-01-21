using MelonLoader;
using RUMBLE.Players.Subsystems;
using RUMBLE.Social.Phone;
using System.Linq;
using UnityEngine;
//Plan:
//Get UserData
//  From Friend List (6 entries)
//  From Recent List (No Info)
//  From Other Players (No Info)
//Write Name/BP to file
//  Check if Name exists
//      Check if BP is same
namespace BPTracker
{
    public class BPTrackerClass : MelonMod
    {
        //constants
        private const string SettingsFile = @"UserData\BPTracker\Settings\Settings.txt";
        private const string LogFilePath = @"UserData\BPTracker\Logs\";
        private const string LogFileName = "BPList";
        private const string LogFileSuffix = ".csv";

        private const string PTag_1 = "Player Tag 2.0";
        private const string PTag_2 = "Player Tag 2.0 (1)";
        private const string PTag_3 = "Player Tag 2.0 (2)";
        private const string PTag_4 = "Player Tag 2.0 (3)";
        private const string PTag_5 = "Player Tag 2.0 (4)";
        private const string PTag_6 = "Player Tag 2.0 (5)";

        //variables

        //objects
        private PlayerTag playertag;

        //initializes things
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
        }

        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();


            playertag = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Player Tags/").GetComponent<PlayerTag>();
            MelonLogger.Msg(playertag.name);
        }


    }
}

