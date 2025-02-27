using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors
{
    public class marker : ModdingTab
    {
        /*
        [MARKER File Structure]
        This file doesn't appear to have anything to specify the amount of entries or total file size. Just table entries.

        For each entry:
        0x00 Entry Length (BE Int32)
        0x14 X Pos(?) (BE Float)
        0x18 Y Pos(?) (BE Float)
        0x1C Z Pos(?) (BE Float)

        If the entry length is 0x7C:
        0x38 Scene Indicator Tag (String)
        */

        public struct Vector3
        {
            public Vector3(float x)
            {
                this.x = x;
                this.y = 0;
                this.z = 0;
            }

            public Vector3(float x, float y)
            {
                this.x = x;
                this.y = y;
                this.z = 0;
            }

            public Vector3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public float x, y, z;
        }

        public class BaseEntry
        {
            public BaseEntry()
            {
                this.entryLength = 0;
                this.position = new Vector3();
                this.sceneIndicator = "";
            }

            public BaseEntry(int entryLength, Vector3 position)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = "";
            }

            public BaseEntry(int entryLength, Vector3 position, string sceneIndicator)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = sceneIndicator;
            }

            public int entryLength;
            public Vector3 position;
            public string sceneIndicator;
        }

        public class GameFlagEntry : BaseEntry
        {
            public GameFlagEntry()
            {
                this.entryLength = 0;
                this.position = new Vector3();
                this.gameFlag = "";
                this.sceneIndicator = "";
            }

            public GameFlagEntry(int entryLength, Vector3 position)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.gameFlag = "";
                this.sceneIndicator = "";
            }

            public GameFlagEntry(int entryLength, Vector3 position, string sceneIndicator)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.gameFlag = "";
                this.sceneIndicator = sceneIndicator;
            }

            public GameFlagEntry(int entryLength, Vector3 position, string sceneIndicator, string gameFlag)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.gameFlag = gameFlag;
                this.sceneIndicator = sceneIndicator;
            }

            public string gameFlag;
        }

        public class NPCEntry : BaseEntry
        {
            public NPCEntry()
            {
                this.entryLength = 0;
                this.position = new Vector3();
                this.sceneIndicator = "";
            }

            public NPCEntry(int entryLength, Vector3 position)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = "";
            }

            public NPCEntry(int entryLength, Vector3 position, string sceneIndicator)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = sceneIndicator;
            }

            public string objFlag;
        }

        public class CrateEntry : BaseEntry
        {
            public CrateEntry()
            {
                this.entryLength = 0;
                this.position = new Vector3();
                this.sceneIndicator = "";
            }

            public CrateEntry(int entryLength, Vector3 position)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = "";
            }

            public CrateEntry(int entryLength, Vector3 position, string sceneIndicator)
            {
                this.entryLength = entryLength;
                this.position = position;
                this.sceneIndicator = sceneIndicator;
            }

            public string gameFlag, collectedCrate, unlockedCrate;
        }

        private static string BASE_ENTRY = "Base Marker Entry";
        private static string SPECIFIC_ENTRY_UNK1 = "Marker Entry - Scene Indicator";

        List<BaseEntry> entries;



        public marker(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public marker(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        
        public override void buildMetaPage()
        {
            int pos = 0;
            int count = 0;
            entries = new List<BaseEntry>();
            while (pos < hxd.sectionData[0].Length)
            {
                BaseEntry entry = new BaseEntry();
                int length = DataMethods.readInt32(hxd.sectionData[0], pos);
                Vector3 worldPos = new Vector3();
                worldPos.x = DataMethods.readFloat32(hxd.sectionData[0], pos + 0x14);
                worldPos.y = DataMethods.readFloat32(hxd.sectionData[0], pos + 0x18);
                worldPos.z = DataMethods.readFloat32(hxd.sectionData[0], pos + 0x1C);
                entry.entryLength = length;
                entry.position = worldPos;

                if (length == 0x7C)
                {
                    entry.sceneIndicator = DataMethods.readString(hxd.sectionData[0], pos + 0x38, 0x40);
                }
                if (length == 0xD0)
                {
                    entry = new GameFlagEntry(length, worldPos);
                    entry.sceneIndicator = DataMethods.readString(hxd.sectionData[0], pos + 0x90, 0x40);
                    (entry as GameFlagEntry).gameFlag = DataMethods.readString(hxd.sectionData[0], pos + 0x48, 0x40);
                }
                if (length == 0x12C)
                {
                    entry = new NPCEntry(length, worldPos);
                    (entry as NPCEntry).objFlag = DataMethods.readString(hxd.sectionData[0], pos + 0x44, 0x40);
                    entry.sceneIndicator = DataMethods.readString(hxd.sectionData[0], pos + 0xEC, 0x40);
                }
                if (length == 0x148)
                {
                    entry = new CrateEntry(length, worldPos);
                    (entry as CrateEntry).collectedCrate = DataMethods.readString(hxd.sectionData[0], pos + 0x3C, 0x40);
                    (entry as CrateEntry).unlockedCrate = DataMethods.readString(hxd.sectionData[0], pos + 0x7C, 0x40);
                    (entry as CrateEntry).gameFlag = DataMethods.readString(hxd.sectionData[0], pos + 0x108, 0x40);
                }

                pos += length;
                count++;
                entries.Add(entry);
            }

            MetaBlock_Text("Entry Count: ", 1, 0x0, 0x0, 1);
            EditLastTextBox("" + count, 110);

            createNode(BASE_ENTRY, count);
            for(int i = 0; i < count; i++)
            {
                MetaBlock_Text("Entry Length: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].entryLength, 110);

                if (entries[i].entryLength == 0x7C)
                {
                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);
                }
                if (entries[i].entryLength == 0xD0)
                {
                    MetaBlock_String("Game Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as GameFlagEntry).gameFlag, 380);

                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);
                }
                if (entries[i].entryLength == 0x12C)
                {
                    MetaBlock_String("Object Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as NPCEntry).objFlag, 380);

                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);
                }
                if (entries[i].entryLength == 0x148)
                {
                    MetaBlock_String("Collected Crate Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as CrateEntry).collectedCrate, 380);

                    MetaBlock_String("Unlocked Crate Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as CrateEntry).unlockedCrate, 380);

                    MetaBlock_String("Game Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as CrateEntry).gameFlag, 380);
                }

                MetaBlock_Text("World Pos - X: ", 1, 0x0, 0x0, 2, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].position.x, 110, 2);
                MetaBlock_Text("World Pos - Y: ", 1, 0x0, 0x0, 2, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].position.y, 110, 2);
                MetaBlock_Text("World Pos - Z: ", 1, 0x0, 0x0, 2, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].position.z, 110, 2);
            }
        }
    }
}
