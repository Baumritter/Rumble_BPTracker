using Il2CppSystem;
using MelonLoader;
using RUMBLE.Interactions.InteractionBase;
using RUMBLE.Social.Phone;
using System.IO;
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
        //--------------------------------------------------
        //constants
        private const int SceneDelay = 450;

        private const string LogFilePath = @"UserData\BPTracker\Logs\BPList.csv";
        private const string PTag_1 = "Player Tag 2.0";
        private const string PTag_2 = "Player Tag 2.0 (1)";
        private const string PTag_3 = "Player Tag 2.0 (2)";
        private const string PTag_4 = "Player Tag 2.0 (3)";
        private const string PTag_5 = "Player Tag 2.0 (4)";
        private const string PTag_6 = "Player Tag 2.0 (5)";

        //friendlist
        private const string FriendList = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Player Tags/";
        private const string PageDown = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageDownButton/Button";
        private const string PageUp = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageUpButton/Button";
        private const string ScrollDown = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollDownButton/Button";
        private const string ScrollUp = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollUpButton/Button";

        //recentlist
        private const string RecentList = "--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Recent Screen/Player Tags/";

        //leaderboard
        private const string Leaderboard = "--------------LOGIC--------------/Heinhouser products/Leaderboard/Player Tags/";
        private const string HTag_1 = "HighscoreTag/";
        private const string HTag_2 = "HighscoreTag (1)/";
        private const string HTag_3 = "HighscoreTag (2)/";
        private const string HTag_4 = "HighscoreTag (3)/";
        private const string HTag_5 = "HighscoreTag (4)/";
        private const string HTag_P = "PersonalHighscoreTag/";

        //--------------------------------------------------


        //--------------------------------------------------
        //variables
        private readonly bool debug = true;
        private bool friendlistbuttonstate = false;
        private bool friendlistbuttonwait = false;
        private bool scenechanged = false;

        private int recentupdate = 0;
        private int boarddelay = 0;
        private int loaddelay = 0;

        private string currentScene = "";
        private string currentName = "";
        private string currentBP = "";
        //--------------------------------------------------

        //--------------------------------------------------
        //objects
        private PlayerTag playertag;
        private PhonePage phonepage;
        private InteractionButton b1;
        private InteractionButton b2;
        private InteractionButton b3;
        private InteractionButton b4;
        //--------------------------------------------------

        //--------------------------------------------------
        //structures/arrays/lists
        private string[] Tags = new string[6];
        private string[] Tags2 = new string[6];
        //--------------------------------------------------

        //initializes things
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
            Tags[0] = PTag_1;
            Tags[1] = PTag_2;
            Tags[2] = PTag_3;
            Tags[3] = PTag_4;
            Tags[4] = PTag_5;
            Tags[5] = PTag_6;
            Tags2[0] = HTag_1;
            Tags2[1] = HTag_2;
            Tags2[2] = HTag_3;
            Tags2[3] = HTag_4;
            Tags2[4] = HTag_5;
            Tags2[5] = HTag_P;
        }

        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();

            try
            {
                if (currentScene == "Gym")
                {
                    //Check for "log request"
                    GetFriendListButtonStatus();
                    GetRecentPageStatus();

                    if ((recentupdate == 1 | friendlistbuttonstate | scenechanged) && loaddelay <= 0)
                    {
                        //Leaderboard
                        if (scenechanged)
                        {
                            for (int i = 0; i < Tags.Length; i++)
                            {
                                GetFromBoardList(Leaderboard + Tags2[i] + Tags[0]);

                                if (currentName != "")
                                {
                                    SearchandReplaceInFile(currentName, currentBP);
                                }
                            }
                            scenechanged = false;
                            //Debug
                            if (debug) MelonLogger.Msg("Leaderboard Checked.");

                        }

                        //Friend List
                        if (friendlistbuttonstate)
                        {
                            for (int i = 0; i < Tags.Length; i++)
                            {
                                GetFromBoardList(FriendList + Tags[i]);

                                if (currentName != "")
                                {
                                    SearchandReplaceInFile(currentName, currentBP);
                                }
                            }
                            friendlistbuttonstate = false;
                            //Debug
                            if (debug) MelonLogger.Msg("Friend Board Checked.");
                        }

                        //Recent List
                        if (recentupdate == 1)
                        {
                            if (boarddelay > 90)
                            {
                                for (int i = 0; i < Tags.Length; i++)
                                {
                                    GetFromBoardList(RecentList + Tags[i]);

                                    if (currentName != "")
                                    {
                                        SearchandReplaceInFile(currentName, currentBP);
                                    }
                                }
                                recentupdate = 2;
                                //Debug
                                if (debug) MelonLogger.Msg("Recent Board Checked");
                            }
                            boarddelay++;
                        }

                        //Debug
                        if (debug) MelonLogger.Msg("Something evaluated.");

                    }
                    if (loaddelay > 0)
                    {
                        loaddelay--;
                        if (debug && loaddelay == 1) MelonLogger.Msg("LoadDelay over.");
                    }

                }
            }
            catch 
            {

            }
        }

        //Custom Functions
        public void GetFromBoardList(string TagString)
        {
            playertag = GameObject.Find(TagString).GetComponent<PlayerTag>();
            if (playertag.UserData != null && playertag.isActiveAndEnabled)
            {
                currentName = playertag.UserData.publicName.ToString() ;
                currentName = SanitizeName(currentName) ;
                currentBP = playertag.UserData.battlePoints.ToString();
            }
            else
            {
                currentName = "";
                currentBP = "";
            }
        }
        
        public void GetRecentPageStatus()
        {
            phonepage = GameObject.Find("--------------LOGIC--------------/Heinhouser products/Telephone 2.0 REDUX special edition/Recent Screen").GetComponent<PhonePage>();

            if (!phonepage.pagePositionIsUpdating && !phonepage.pageActivated && recentupdate == 2)
            {
                recentupdate = 0;
                boarddelay = 0;
                //Debug
                if (debug) MelonLogger.Msg("Recent Board OFF");
            }
            if (!phonepage.pagePositionIsUpdating && phonepage.pageActivated && recentupdate == 0)
            {
                recentupdate = 1;
                boarddelay = 0;
                //Debug
                if (debug) MelonLogger.Msg("Recent Board ON");
            }
        }

        public void GetFriendListButtonStatus()
        {
            b1 = GameObject.Find(ScrollUp).GetComponent<InteractionButton>();
            b2 = GameObject.Find(ScrollDown).GetComponent<InteractionButton>();
            b3 = GameObject.Find(PageUp).GetComponent<InteractionButton>();
            b4 = GameObject.Find(PageDown).GetComponent<InteractionButton>();
            if (!b1.isPressed && !b2.isPressed && !b3.isPressed && !b4.isPressed && !friendlistbuttonwait)
            {
                friendlistbuttonwait = true;
                if (debug) MelonLogger.Msg("No Button pressed.");
            }
            if ((b1.isPressed || b2.isPressed || b3.isPressed || b4.isPressed) && !friendlistbuttonstate && friendlistbuttonwait)
            {
                friendlistbuttonwait = false;
                friendlistbuttonstate = true;
                if (debug) MelonLogger.Msg("Button pressed.");
            }
        }

        public string SanitizeName(string Input)
        {
            bool RemoveChars = false;
            char[] chars = Input.ToCharArray();
            string Output = "";

            for(int c = 0; c < Input.Length;c++)
            {
                if (chars[c] == '<')
                {
                    if (chars[c+1] == '#')
                    {
                        RemoveChars = true;
                    }
                }
                if (!RemoveChars)
                {
                    Output = Output + chars[c];
                }
                if (chars[c] == '>')
                {
                    RemoveChars = false;
                }
            }
            return Output;
        }

        public string FormatForFile(string st1, string st2)
        {
            return st1 + ";" + st2 + Environment.NewLine;
        }

        public void SearchandReplaceInFile(string Input,string Input2)
        {
            int Line = -1;
            int loop = 0;

            if (System.IO.File.Exists(LogFilePath))
            {
                string[] fileContents = System.IO.File.ReadAllLines(LogFilePath);

                foreach(var item in fileContents)
                {
                    if (item.Contains(Input))
                    {
                        Line = loop;
                    }
                    loop++;
                }

                if (Line != -1)
                {
                    if (!fileContents[Line].Contains(Input2))
                    {
                        fileContents[Line] = FormatForFile(Input, Input2);
                        File.WriteAllLines(LogFilePath, fileContents);

                        //Debug
                        if (debug) MelonLogger.Msg("Changed: " + Input + " " + Input2 + " in Line: " + Line.ToString());
                    }
                    else
                    {
                        //Debug
                        if (debug) MelonLogger.Msg("Changed Nothing in Line " + Line.ToString());
                    }
                }
                else
                {
                    File.AppendAllText(LogFilePath, FormatForFile(Input, Input2));

                    //Debug
                    if (debug) MelonLogger.Msg("Appended: " + Input + " " + Input2);
                }
            }
            else
            {
                File.AppendAllText(LogFilePath, FormatForFile("Name", "BP"));
                File.AppendAllText(LogFilePath, FormatForFile(Input, Input2));

                //Debug
                if (debug) MelonLogger.Msg("NF Appended: " + Input + " " + Input2);
            }
        }


        //Methods
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
            loaddelay = SceneDelay;
            scenechanged = true;
        }


    }
}

