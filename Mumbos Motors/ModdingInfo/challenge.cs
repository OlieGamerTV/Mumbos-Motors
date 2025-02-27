using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors
{
    public class challenge : ModdingTab
    {
        /*
        [CHALLENGE File Structure]
        --- Table Header ---
        0x00 Type (BE Int32)
        0x04 Padding(?)
        0x08 Padding(?)
        0x0C Table Size (BE Int32)

        --- Main File Header (0x01) ---
        0x00 - 0x0C Table Header
        0x10 World Name Tag (String, max size is 0x20)
        0x30 Mission Name Tag (String, max size is 0x20)
        0x50 Game Style Tag (String, max size is 0x20)
        0x70 ??? (??? Int32)
        0x78 Vehicle ID (BE Int32)
        0xBC Intro Dialogue ID (??? Int32)
        0xC0 Success Dialogue ID (??? Int32)
        0xC4 Objective Cutscene ID (??? Int32)
        0xC8 End of Challenge Cutscene ID (??? Int32)
        0xD8 Game Seen Objective Flag Type (String, max size is 0x40)
        0x118 Game Style Flag Type (String, max size is 0x40)
        0x158 Game Notes Only Flag Type (String, max size is 0x40)
        0x198 Game Beaten Flag Type (String, max size is 0x40)
        0x1D8 Game Beaten C+ Flag Type (String, max size is 0x40)
        0x218 Game Jiggies Won World Flag Type (String, max size is 0x44)
        0x25C Score Act Game World Flag Type (String, max size is 0x4C?)
        
        0x2C8 Max A Value (BE Float)
        0x2CC Max B Value (BE Float)
        0x2D0 Max C Value (BE Float)
        0x2D4 Max D Value (BE Float)
        0x2E0 Timer Type (BE Int32, Switches between Regular and Simplified UI)
        0x2E4 Target Amount (BE Int32)
        0x2EC Countdown Time (BE Float, if set, will count down instead of counting up)

        --- Table (0x13) ---
        0x00 ??? (BE Float)
        0x04 ??? (BE Float)
        0x08 ??? (BE Int32)
        */

        /*
        [CHALLENGE gameFlag Types]
        gameFlag_Normal_Null
        gameFlag_Normal_Style_Open
        gameFlag_Normal_Style_Limited
        gameFlag_Normal_Style_Ingame
        */

        /*
        [CHALLENGE counter Types]
        gameCounter_BanjoX_Null
        */

        /*
        [CHALLENGE Scene Indicator Types]
        sceneIndicatorType_Null
        sceneIndicatorType_General_Arrow
        sceneIndicatorType_General_RegionShown
        sceneIndicatorType_General_JinjoFetchItem
        sceneIndicatorType_General_Heart
        sceneIndicatorType_Gate
        sceneIndicatorType_Bomb
        sceneIndicatorType_HotRock
        sceneIndicatorType_Coconut
        sceneIndicatorType_General_Portal
        sceneIndicatorType_NPC_ChallengeGrunty
        sceneIndicatorType_NPC_Boggy
        sceneIndicatorType_NPC_Blubber
        sceneIndicatorType_NPC_Pikelet
        sceneIndicatorType_NPC_JinjoGreen
        sceneIndicatorType_NPC_JinjoGreenMapOnly
        sceneIndicatorType_NPC_JinjoYellow
        sceneIndicatorType_NPC_JinjoRedMapOnly
        sceneIndicatorType_NPC_JinjoPink
        sceneIndicatorType_NuttyAcres_Fireball
        sceneIndicatorType_NuttyAcres_Bomb
        sceneIndicatorType_NuttyAcres_MrPatch
        sceneIndicatorType_CPU_Antenna
        sceneIndicatorType_CPU_Workers
        sceneIndicatorType_CPU_BrightnessUnit
        sceneIndicatorType_CPU_Chip
        sceneIndicatorType_CPU_Fan
        sceneIndicatorType_CPU_FlubberSponge
        sceneIndicatorType_CPU_Laptop
        sceneIndicatorType_Banjoland_ClankerEye
        sceneIndicatorType_Banjoland_GeorgeIceCube
        sceneIndicatorType_Banjoland_Football
        sceneIndicatorType_WorldOfSport_NineBall_Highlight
        sceneIndicatorType_WorldOfSport_Beachball
        sceneIndicatorType_WorldOfSport_Dice
        sceneIndicatorType_WorldOfSport_Torch
        */

        /*
        [CHALLENGE Object Tag]
        objTag_Null
        objTag_BanjoX_Actor_Banjo
        objTag_BanjoX_Actor_Thomas
        objTag_BanjoX_Actor_Bottles
        objTag_BanjoX_Actor_Klungo
        objTag_BanjoX_Actor_MrFit
        objTag_BanjoX_Actor_Humba
        objTag_BanjoX_Actor_Grunty
        objTag_BanjoX_Actor_Piddles
        objTag_BanjoX_Actor_Blubber
        objTag_BanjoX_Actor_Pikelet
        objTag_BanjoX_Actor_Jinjo3
        objTag_BanjoX_Actor_Jinjo2
        objTag_BanjoX_Actor_Jinjo1
        objTag_BanjoX_CPU_Chip
        */

        public class ChallengeFile
        {
            
        }

        public class ChallengeHeader
        {
            public string worldNameTag, challengeTag, gameStyleTag;
            public string seenObjectivesFlag, gameStyleFlag;
            public string notesOnlyFlag, beatenFlag, beatenCPlusFlag, jiggiesWonFlag, worldScoreFlag;
        }

        public challenge(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public challenge(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            //throw new NotImplementedException();
        }
    }
}
