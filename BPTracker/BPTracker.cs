using Il2CppSystem;
using MelonLoader;
using RUMBLE.Interactions.InteractionBase;
using RUMBLE.Social.Phone;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BPTracker
{
    public class PlayerInfo
    {
        private readonly string listseparator = ",";
        public string Username { get; set; }
        public string BattlePoints { get; set; }
        public string Platform { get; set; }
        public string UserID { get; set; }
        public string BPChangeTime { get; set; }

        public string ReturnFileString()
        {
            return BattlePoints + listseparator + Username + listseparator + Platform + listseparator + UserID + listseparator + BPChangeTime + Environment.NewLine;
        }

        public void SanitizeName(string Input)
        {
            bool RemoveChars = false;
            char[] chars = Input.ToCharArray();
            string Output = "";

            if (Input.Contains("<u>")) Input.Replace("<u>", "");
            if (Input.Contains(",")) Input.Replace(",", "");

            for (int c = 0; c < Input.Length; c++)
            {
                if (chars[c] == '<' && c != Input.Length)
                {
                    if (chars[c + 1] == '#' || chars[c + 1] == 'c')
                    {
                        RemoveChars = true;
                    }
                }
                if (!RemoveChars)
                {
                    Output += chars[c];
                }
                if (chars[c] == '>')
                {
                    RemoveChars = false;
                }
            }
            this.Username = Output;
        }


    }
    public class BPTrackerClass : MelonMod
    {
        //constants
        private const double SceneDelay = 5.0;
        private const double RecentDelay = 5.0;

        private const string LogFilePath = @"UserData\BPTracker\Logs\BPList.csv";

        //game objects
        private const string FriendList = "/Telephone 2.0 REDUX special edition/Friend Screen/Player Tags/";
        private const string PageDown = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageDownButton/Button";
        private const string PageUp = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageUpButton/Button";
        private const string ScrollDown = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollDownButton/Button";
        private const string ScrollUp = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollUpButton/Button";
        private const string RecentList = "/Telephone 2.0 REDUX special edition/Recent Screen/Player Tags/";
        private const string SlidingObject = "/Telephone 2.0 REDUX special edition/Recent Screen";
        private const string ParkPrefix = "________________LOGIC__________________ /Heinhouwser products";
        private const string GymPrefix = "--------------LOGIC--------------/Heinhouser products";
        private const string Leaderboard = "--------------LOGIC--------------/Heinhouser products/Leaderboard/Player Tags/";

        //variables
        private readonly bool debug = false;
        private bool friendlistbuttonstate = false;
        private bool friendlistbuttonwait = false;
        private bool scenechanged = false;
        private bool recentleverlockout = false;
        private bool prefixlockout = false;
        private bool logfileerror = false;

        private string currentScene = "";
        private string objprefix = "";

        //objects
        private PlayerInfo InfoObj;
        private PlayerTag playertag;
        private PhonePage phonepage;
        private InteractionButton b1;
        private InteractionButton b2;
        private InteractionButton b3;
        private InteractionButton b4;

        private Delay Delay_SceneLoad = new Delay { name = "LoadDelay" };
        private Delay Delay_RecentBoard = new Delay { name = "RecentDelay" };
        private Folders folders = new Folders();


        //structures/arrays/lists
        private string[] Tags = new string[6];
        private string[] Tags2 = new string[6];


        //initializes things
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();

            folders.SetModFolderNamespace();
            folders.AddSubFolder("Logs");
            folders.CheckAllFoldersExist();

            Tags[0] = "Player Tag 2.0";
            Tags[1] = "Player Tag 2.0 (1)";
            Tags[2] = "Player Tag 2.0 (2)";
            Tags[3] = "Player Tag 2.0 (3)";
            Tags[4] = "Player Tag 2.0 (4)";
            Tags[5] = "Player Tag 2.0 (5)";
            Tags2[0] = "HighscoreTag/";
            Tags2[1] = "HighscoreTag (1)/";
            Tags2[2] = "HighscoreTag (2)/";
            Tags2[3] = "HighscoreTag (3)/";
            Tags2[4] = "HighscoreTag (4)/";
            Tags2[5] = "PersonalHighscoreTag/";
        }

        //Run every update
        public override void OnUpdate()
        {
            //Base Updates
            base.OnUpdate();

            if (!logfileerror)
            {
                SwapPrefix();

                try
                {
                    if ((currentScene == "Gym" || currentScene == "Park"))
                    {
                        //Check for FriendList/RecentList Trigger
                        GetFriendListButtonStatus();
                        GetRecentPageStatus();

                        if ((friendlistbuttonstate | scenechanged | Delay_RecentBoard.Done) && Delay_SceneLoad.Done)
                        {
                            //Leaderboard
                            if (scenechanged && currentScene == "Gym")
                            {
                                for (int i = 0; i < Tags.Length; i++)
                                {
                                    InfoObj = GetFromBoardList(Leaderboard + Tags2[i] + Tags[0]);

                                    if (InfoObj != null)
                                    {
                                        SearchandReplaceInFile(InfoObj);
                                        if (logfileerror) return;
                                    }
                                }
                                scenechanged = false;

                                if (debug) MelonLogger.Msg("Leaderboard Checked.");

                            }

                            //Friend List
                            if (friendlistbuttonstate)
                            {
                                for (int i = 0; i < Tags.Length; i++)
                                {
                                    InfoObj = GetFromBoardList(objprefix + FriendList + Tags[i]);

                                    if (InfoObj != null)
                                    {
                                        SearchandReplaceInFile(InfoObj);
                                        if (logfileerror) return;
                                    }
                                }
                                friendlistbuttonstate = false;

                                if (debug) MelonLogger.Msg("Friend Board Checked.");
                            }

                            //Recent List
                            if (Delay_RecentBoard.Done)
                            {
                                for (int i = 0; i < Tags.Length; i++)
                                {
                                    InfoObj = GetFromBoardList(objprefix + RecentList + Tags[i]);

                                    if (InfoObj != null)
                                    {
                                        SearchandReplaceInFile(InfoObj);
                                        if (logfileerror) return;
                                    }
                                }
                                if (debug) MelonLogger.Msg("Recent Board Checked");
                            }


                            if (debug) MelonLogger.Msg("Something evaluated.");

                        }


                    }
                }
                catch
                {
                    if ((friendlistbuttonstate | scenechanged | Delay_RecentBoard.Done) && Delay_SceneLoad.Done)
                    {
                        if (debug) MelonLogger.Msg("Try Failed.");
                    }
                }

            }

        }

        //Functions
        #region Information Acquisition
        public PlayerInfo GetFromBoardList(string TagString)
        {
            char temp;
            PlayerInfo playerInfo = new PlayerInfo();
            playertag = GameObject.Find(TagString).GetComponent<PlayerTag>();
            if (playertag.UserData != null && playertag.isActiveAndEnabled)
            {

                playerInfo.SanitizeName(playertag.UserData.publicName);
                playerInfo.BattlePoints = playertag.UserData.battlePoints.ToString();
                playerInfo.UserID = playertag.UserData.playFabId.ToString();   

                temp = playertag.UserData.platformId;
                switch (temp)
                {
                    case 'O':
                        playerInfo.Platform = "Oculus";
                        break;
                    case 'S':
                        playerInfo.Platform = "Steam";
                        break;
                    default:
                        playerInfo.Platform = "Unknown";
                        break;
                }
                
            }
            else
            {
                playerInfo = null;
            }
            return playerInfo;
        }
        public void GetFriendListButtonStatus()
        {
            b1 = GameObject.Find(objprefix + ScrollUp).GetComponent<InteractionButton>();
            b2 = GameObject.Find(objprefix + ScrollDown).GetComponent<InteractionButton>();
            b3 = GameObject.Find(objprefix + PageUp).GetComponent<InteractionButton>();
            b4 = GameObject.Find(objprefix + PageDown).GetComponent<InteractionButton>();
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
        public void GetRecentPageStatus()
        {
            phonepage = GameObject.Find(objprefix + SlidingObject).GetComponent<PhonePage>();

            if (!phonepage.pagePositionIsUpdating && !phonepage.pageActivated && recentleverlockout)
            {
                Delay_RecentBoard.Done = false;
                recentleverlockout = false;
            }
            if (!phonepage.pagePositionIsUpdating && phonepage.pageActivated && !Delay_RecentBoard.Done && !recentleverlockout)
            {
                Delay_RecentBoard.Delay_Start(RecentDelay);
                recentleverlockout = true;
            }
        }
        public void SwapPrefix()
        {
            if(scenechanged && !prefixlockout)
            {
                switch (currentScene)
                {
                    case "Gym":
                        objprefix = GymPrefix;
                        prefixlockout = true;   
                        if (debug) MelonLogger.Msg("Prefix changed to Gym.");
                        break;
                    case "Park":
                        objprefix = ParkPrefix;
                        prefixlockout = true;
                        scenechanged = false;
                        if (debug) MelonLogger.Msg("Prefix changed to Park.");
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        public void SearchandReplaceInFile(PlayerInfo Input)
        {
            int Line = -1;
            int loop = 0;
            string[] fileContents;


            if (System.IO.File.Exists(LogFilePath))
            {
                try
                {
                     fileContents = File.ReadAllLines(LogFilePath);
                }
                catch
                {
                    MelonLogger.Msg("CLOSE THE LOG FILE. EXECUTION SUSPENDED UNTIL NEXT SCENE CHANGE.");
                    logfileerror = true;
                    return;
                }

                foreach(var item in fileContents)
                {
                    if (item.Contains(Input.UserID))
                    {
                        Line = loop;
                        break;
                    }
                    loop++;
                }

                if (Line != -1)
                {
                    if (!fileContents[Line].Contains(Input.BattlePoints))
                    {
                        Input.BPChangeTime = DateTime.Now.ToString();
                        fileContents[Line] = Input.ReturnFileString();
                        File.WriteAllLines(LogFilePath, fileContents);

                        
                        if (debug) MelonLogger.Msg("Changed: " + Input.UserID + " " + Input.Username + " " + Input.BattlePoints + " in Line: " + Line.ToString());
                    }
                    else
                    {
                        
                        if (debug) MelonLogger.Msg("Changed Nothing in Line " + Line.ToString());
                    }
                }
                else
                {
                    Input.BPChangeTime = "New Entry";
                    File.AppendAllText(LogFilePath, Input.ReturnFileString());

                    
                    if (debug) MelonLogger.Msg("Appended: " + Input.UserID + " " + Input.Username + " " + Input.BattlePoints);
                }
                RemoveEmptyLinesFromFile();
            }
            else
            {
                Input.BPChangeTime = "New Entry";
                File.AppendAllText(LogFilePath, "BP,Name,Platform,UserID,LastChange" + Environment.NewLine);
                File.AppendAllText(LogFilePath, Input.ReturnFileString());

                
                if (debug) MelonLogger.Msg("NF Appended: " + Input.UserID + " " + Input.Username + " " + Input.BattlePoints);
            }

        }
        private void RemoveEmptyLinesFromFile()
        {
            File.WriteAllLines(LogFilePath, System.IO.File.ReadAllLines(LogFilePath).Where(l => !string.IsNullOrWhiteSpace(l)));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
            scenechanged = true;

            prefixlockout = false;
            logfileerror = false;

            Delay_SceneLoad.Delay_Start(SceneDelay);

            if (debug) MelonLogger.Msg("Scene changed to " + currentScene.ToString() + " = " + scenechanged.ToString());
        }


    }
}

