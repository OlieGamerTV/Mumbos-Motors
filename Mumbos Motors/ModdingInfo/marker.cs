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
        0x04 Entry Type (BE Int16)
        0x06 Entry ID (BE Int16)
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

            public BaseEntry(int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "")
            {
                this.entryLength = entryLength;
                this.entryType = 0;
                this.entryID = 0;
                this.position = position;
                this.sceneIndicator = "";
            }

            public BaseEntry(int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "")
            {
                this.entryLength = entryLength;
                this.entryType = entryType;
                this.entryID = entryID;
                this.position = position;
                this.sceneIndicator = sceneIndicator;
            }

            public int entryLength;
            public int entryType;
            public int entryID;
            public Vector3 position;
            public string sceneIndicator;
        }

        public class ObjectEntry : BaseEntry
        {
            public ObjectEntry() : base() { }

            public ObjectEntry(int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, position, sceneIndicator)
            {
                this.gameFlag = "";
            }

            public ObjectEntry(int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, entryType, entryID, position, sceneIndicator)
            {
                this.gameFlag = "";
            }

            public ObjectEntry(string gameFlag, int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, position, sceneIndicator)
            {
                this.gameFlag = gameFlag;
            }

            public ObjectEntry(string gameFlag, int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, entryType, entryID, position, sceneIndicator)
            {
                this.gameFlag = gameFlag;
            }

            public string gameFlag;

            public uint objParamID;
            public uint scriptID;
        }

        public class NPCEntry : BaseEntry
        {
            public NPCEntry() : base() { }

            public NPCEntry(int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, position, sceneIndicator) { }

            public NPCEntry(int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, entryType, entryID, position, sceneIndicator) { }

            public string objFlag;
        }

        public class PortalEntry : BaseEntry
        {
            public PortalEntry() : base() { }

            public PortalEntry(int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, position, sceneIndicator) { }

            public PortalEntry(int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, entryType, entryID, position, sceneIndicator) { }

            public uint objParamUUID, scriptUUID;
        }

        public class CrateEntry : BaseEntry
        {
            public CrateEntry()
            {
                this.entryLength = 0;
                this.position = new Vector3();
                this.sceneIndicator = "";
            }

            public CrateEntry(int entryLength = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, position, sceneIndicator) { }

            public CrateEntry(int entryLength = 0, int entryType = 0, int entryID = 0, Vector3 position = new Vector3(), string sceneIndicator = "") : base(entryLength, entryType, entryID, position, sceneIndicator) { }

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
            byte[] tempSectionData = hxd.sectionData[0];
            int pos = 0;
            int count = 0;
            entries = new List<BaseEntry>();
            while (pos < tempSectionData.Length)
            {
                BaseEntry entry = new BaseEntry();
                int length = DataMethods.readInt32(tempSectionData, pos);
                int type = DataMethods.readInt16(tempSectionData, pos + 0x4);
                int id = DataMethods.readInt16(tempSectionData, pos + 0x6);
                Vector3 worldPos = new Vector3();
                worldPos.x = DataMethods.readFloat32(tempSectionData, pos + 0x14);
                worldPos.y = DataMethods.readFloat32(tempSectionData, pos + 0x18);
                worldPos.z = DataMethods.readFloat32(tempSectionData, pos + 0x1C);
                entry.entryLength = length;
                entry.entryType = type;
                entry.entryID = id;
                entry.position = worldPos;

                if (type == 0xE)
                {
                    entry.sceneIndicator = DataMethods.readString(tempSectionData, pos + 0x38, 0x40);
                }
                if (type == 0x1C)
                {
                    entry = new ObjectEntry(length, type, id, worldPos);
                    entry.sceneIndicator = DataMethods.readString(tempSectionData, pos + 0x90, 0x40);
                    (entry as ObjectEntry).gameFlag = DataMethods.readString(tempSectionData, pos + 0x48, 0x40);
                    (entry as ObjectEntry).objParamID = DataMethods.readUInt32(tempSectionData, pos + 0x3C);
                    (entry as ObjectEntry).scriptID = DataMethods.readUInt32(tempSectionData, pos + 0x40);
                }
                if (type == 0x6)
                {
                    entry = new NPCEntry(length, type, id, worldPos);
                    (entry as NPCEntry).objFlag = DataMethods.readString(tempSectionData, pos + 0x44, 0x40);
                    entry.sceneIndicator = DataMethods.readString(tempSectionData, pos + 0xEC, 0x40);
                }
                if (type == 0x24)
                {
                    entry = new CrateEntry(length, type, id, worldPos);
                    (entry as CrateEntry).collectedCrate = DataMethods.readString(tempSectionData, pos + 0x3C, 0x40);
                    (entry as CrateEntry).unlockedCrate = DataMethods.readString(tempSectionData, pos + 0x7C, 0x40);
                    (entry as CrateEntry).gameFlag = DataMethods.readString(tempSectionData, pos + 0x108, 0x40);
                }

                pos += length;
                count++;
                entries.Add(entry);
            }

            pos = 0;

            MetaBlock_Text("Entry Count: ", 1, 0x0, 0x0, 1);
            EditLastTextBox("" + count, 110);

            createNode(BASE_ENTRY, count);
            for(int i = 0; i < count; i++)
            {
                MetaBlock_Text("Entry Length: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].entryLength, 110);

                MetaBlock_Text("Entry Type: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].entryType, 110);

                MetaBlock_Text("Entry ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entries[i].entryID, 110);

                if (entries[i].entryType == 0xE)
                {
                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);
                }
                if (entries[i].entryType == 0x1C)
                {
                    MetaBlock_String("Game Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as ObjectEntry).gameFlag, 380);

                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);

                    MetaBlock_Text("Obj Param ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                    EditLastTextBox("" + (entries[i] as ObjectEntry).objParamID, 380, 1);

                    MetaBlock_Text("Script ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                    EditLastTextBox("" + (entries[i] as ObjectEntry).scriptID, 380, 1);
                }
                if (entries[i].entryType == 0x6)
                {
                    MetaBlock_String("Object Flag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + (entries[i] as NPCEntry).objFlag, 380);

                    MetaBlock_String("Indicator Tag: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                    EditLastStringBox("" + entries[i].sceneIndicator, 380);
                }
                if (entries[i].entryType == 0x24)
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
