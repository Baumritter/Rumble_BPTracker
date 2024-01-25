using Il2CppSystem;
using MelonLoader;
using RUMBLE.Interactions.InteractionBase;
using RUMBLE.Social.Phone;
using System.IO;
using System.Linq;
using UnityEngine;
namespace BPTracker
{
    public class BPTrackerClass : MelonMod
    {
        //--------------------------------------------------
        //--------------------------------------------------
        //constants
        private const double SceneDelay = 5.0;
        private const double RecentDelay = 5.0;

        private const string LogFilePath = @"UserData\BPTracker\Logs\BPList.csv";
        private const string PTag_1 = "Player Tag 2.0";
        private const string PTag_2 = "Player Tag 2.0 (1)";
        private const string PTag_3 = "Player Tag 2.0 (2)";
        private const string PTag_4 = "Player Tag 2.0 (3)";
        private const string PTag_5 = "Player Tag 2.0 (4)";
        private const string PTag_6 = "Player Tag 2.0 (5)";

        //friendlist
        private const string FriendList = "/Telephone 2.0 REDUX special edition/Friend Screen/Player Tags/";
        private const string PageDown = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageDownButton/Button";
        private const string PageUp = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/PageUpButton/Button";
        private const string ScrollDown = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollDownButton/Button";
        private const string ScrollUp = "/Telephone 2.0 REDUX special edition/Friend Screen/Friend Scroll Bar/ScrollUpButton/Button";

        //recentlist
        private const string RecentList = "/Telephone 2.0 REDUX special edition/Recent Screen/Player Tags/";
        private const string SlidingObject = "/Telephone 2.0 REDUX special edition/Recent Screen";

        //park-string
        private const string ParkPrefix = "________________LOGIC__________________ /Heinhouwser products";

        //gym-string
        private const string GymPrefix = "--------------LOGIC--------------/Heinhouser products";

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


        //--------------------------------------------------
        //--------------------------------------------------
        //variables
        private readonly bool debug = false;
        private bool friendlistbuttonstate = false;
        private bool friendlistbuttonwait = false;
        private bool scenechanged = false;
        private bool loaddelaydone = false;
        private bool loadlockout = false;
        private bool recentdelaydone = false;
        private bool recentlockout = false;
        private bool recentleverlockout = false;
        private bool prefixlockout = false;

        private DateTime boarddelay;
        private DateTime loaddelay;

        private string currentScene = "";
        private string currentName = "";
        private string currentBP = "";
        private string currentplatform = "";
        private string objprefix = "";
        //--------------------------------------------------
        //--------------------------------------------------

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

        //--------------------------------------------------
        //--------------------------------------------------
        //structures/arrays/lists
        private string[] Tags = new string[6];
        private string[] Tags2 = new string[6];
        //--------------------------------------------------
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

            LoadDelayLogic();
            SwapPrefix();

            try
            {
                if (currentScene == "Gym" || currentScene == "Park")
                {
                    //Check for FriendList/RecentList Trigger
                    GetFriendListButtonStatus();
                    GetRecentPageStatus();

                    if ((friendlistbuttonstate | scenechanged | (recentdelaydone && !recentlockout)) && loaddelaydone)
                    {
                        //Leaderboard
                        if (scenechanged && currentScene == "Gym")
                        {
                            for (int i = 0; i < Tags.Length; i++)
                            {
                                GetFromBoardList(Leaderboard + Tags2[i] + Tags[0]);

                                if (currentName != "")
                                {
                                    SearchandReplaceInFile(currentName, currentBP, currentplatform);
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
                                GetFromBoardList(objprefix + FriendList + Tags[i]);

                                if (currentName != "")
                                {
                                    SearchandReplaceInFile(currentName, currentBP, currentplatform);
                                }
                            }
                            friendlistbuttonstate = false;
                            
                            if (debug) MelonLogger.Msg("Friend Board Checked.");
                        }

                        //Recent List
                        if (recentdelaydone)
                        {
                            for (int i = 0; i < Tags.Length; i++)
                            {
                                GetFromBoardList(objprefix + RecentList + Tags[i]);

                                if (currentName != "")
                                {
                                    SearchandReplaceInFile(currentName, currentBP, currentplatform);
                                }
                            }
                            recentlockout = true;
                            
                            if (debug) MelonLogger.Msg("Recent Board Checked");
                        }

                        
                        if (debug) MelonLogger.Msg("Something evaluated.");

                    }


                }
            }
            catch
            {
                if ((friendlistbuttonstate | scenechanged | recentdelaydone) && loaddelaydone)
                {
                    if (debug) MelonLogger.Msg("Try Failed.");
                }
            }
        }

        //Functions
        public void GetFromBoardList(string TagString)
        {
            char temp;
            playertag = GameObject.Find(TagString).GetComponent<PlayerTag>();
            if (playertag.UserData != null && playertag.isActiveAndEnabled)
            {
                currentName = playertag.UserData.publicName.ToString() ;
                currentName = SanitizeName(currentName) ;
                currentBP = playertag.UserData.battlePoints.ToString();
                temp = playertag.UserData.platformId;
                switch (temp)
                {
                    case 'O':
                        currentplatform = "Oculus";
                        break;
                    case 'S':
                        currentplatform = "Steam";
                        break;
                    default:
                        currentplatform = "Unknown";
                        break;
                }
                
            }
            else
            {
                currentName = "";
                currentBP = "";
            }
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
                recentdelaydone = false;
                recentlockout = false;
                recentleverlockout = false;
                
                if (debug) MelonLogger.Msg("Recent Board: OFF");
            }
            if (!phonepage.pagePositionIsUpdating && phonepage.pageActivated && !recentlockout && !recentleverlockout && !recentdelaydone)
            {
                boarddelay = DateTime.Now.AddSeconds(RecentDelay);
                recentleverlockout = true;
                
                if (debug) MelonLogger.Msg("Recent Board: Delay Start");
                if (debug) MelonLogger.Msg(boarddelay.ToString());
            }
            if (DateTime.Now >= boarddelay && recentleverlockout && !recentdelaydone)
            {
                recentdelaydone = true;
                
                if (debug) MelonLogger.Msg("Recent Board: Delay Done");
            }
        }

        public void LoadDelayLogic()
        {
            if (scenechanged && !loaddelaydone && !loadlockout)
            {
                loaddelay = DateTime.Now.AddSeconds(SceneDelay);
                loadlockout = true;
                if (debug) MelonLogger.Msg("LoadDelay: Start.");
                if (debug) MelonLogger.Msg(loaddelay.ToString());
            }
            if (DateTime.Now >= loaddelay && !loaddelaydone)
            {
                loaddelaydone = true;
                if (debug) MelonLogger.Msg("LoadDelay: End.");
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

        public string FormatForFile(string st1, string st2, string st3)
        {
            return st1 + ";" + st2 + ";" + st3 + Environment.NewLine;
        }

        public void SearchandReplaceInFile(string Input,string Input2,string Input3)
        {
            int Line = -1;
            int loop = 0;

            if (System.IO.File.Exists(LogFilePath))
            {
                string[] fileContents = File.ReadAllLines(LogFilePath);

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
                        fileContents[Line] = FormatForFile(Input2, Input,Input3);
                        File.WriteAllLines(LogFilePath, fileContents);

                        
                        if (debug) MelonLogger.Msg("Changed: " + Input + " " + Input2 + " in Line: " + Line.ToString());
                    }
                    else
                    {
                        
                        if (debug) MelonLogger.Msg("Changed Nothing in Line " + Line.ToString());
                    }
                }
                else
                {
                    File.AppendAllText(LogFilePath, FormatForFile(Input2, Input, Input3));

                    
                    if (debug) MelonLogger.Msg("Appended: " + Input + " " + Input2);
                }
                RemoveEmptyLinesFromFile();
            }
            else
            {
                File.AppendAllText(LogFilePath, FormatForFile("BP", "Name","Platform"));
                File.AppendAllText(LogFilePath, FormatForFile(Input2, Input, Input3));

                
                if (debug) MelonLogger.Msg("NF Appended: " + Input + " " + Input2);
            }

        }

        public void RemoveEmptyLinesFromFile()
        {
            File.WriteAllLines(LogFilePath, System.IO.File.ReadAllLines(LogFilePath).Where(l => !string.IsNullOrWhiteSpace(l)));
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
            scenechanged = true;
            loaddelaydone = false; 
            prefixlockout = false;
            loadlockout = false;
            if (debug) MelonLogger.Msg("Scene changed to " + currentScene.ToString() + " = " + scenechanged.ToString());
        }


    }
}

