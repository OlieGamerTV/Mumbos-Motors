using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Mumbos_Motors.marker;

namespace Mumbos_Motors
{
    public class script : ModdingTab
    {
        /*
        [SCRIPT File Structure]
        This file doesn't appear to have anything to specify the amount of entries or total file size. Just table entries.

        Every ID is the size of the entry.
        */
        public class BaseEntry
        {
            public BaseEntry()
            {
                this.entryLength = 0;
                entryID = 0;
            }

            public BaseEntry(int entryLength)
            {
                this.entryLength = entryLength;
                entryID = 0;
            }

            public BaseEntry(int entryLength, int entryID)
            {
                this.entryLength = entryLength;
                this.entryID = entryID;
            }

            public int entryLength;
            public int entryID;
        }

        public class CommentEntry : BaseEntry
        {
            public CommentEntry() : base() { }

            public CommentEntry(int entryLength) : base(entryLength) { }

            public CommentEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public string comment;
        }

        public class ChallengeEntry : BaseEntry
        {
            public ChallengeEntry() : base() { }

            public ChallengeEntry(int entryLength) : base(entryLength) { }

            public ChallengeEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public string comment;
            public int fileUUID, unk1;
        }

        public class ScriptEntry : BaseEntry
        {
            public ScriptEntry() : base() { }

            public ScriptEntry(int entryLength) : base(entryLength) { }

            public ScriptEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int fileUUID;
        }

        public class MarkerEntry : BaseEntry
        {
            public MarkerEntry() : base() { }

            public MarkerEntry(int entryLength) : base(entryLength) { }

            public MarkerEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int fileUUID, unk1;
        }

        public class XMVEntry : BaseEntry
        {
            public XMVEntry() : base() { }

            public XMVEntry(int entryLength) : base(entryLength) { }

            public XMVEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int xmvID, unk1, unk2, unk3, unk4, unk5;
        }

        public class AttractEntry : BaseEntry
        {
            public AttractEntry() : base() { }

            public AttractEntry(int entryLength) : base(entryLength) { }

            public AttractEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int[] xmvIDs;
        }

        public class CaffEntry : BaseEntry
        {
            public CaffEntry() : base() { }

            public CaffEntry(int entryLength) : base(entryLength) { }

            public CaffEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int unk1, caffID, multiCaffID;
        }

        public class GameFlagEntry : BaseEntry
        {
            public GameFlagEntry() : base() { }

            public GameFlagEntry(int entryLength) : base(entryLength) { }

            public GameFlagEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public string gameFlag;
            public int value;
        }

        public class FileEntry : BaseEntry
        {
            public FileEntry() : base() { }

            public FileEntry(int entryLength) : base(entryLength) { }

            public FileEntry(int entryLength, int entryID) : base(entryLength, entryID) { }

            public int fileUUID;
        }

        int entryCount = 0;
        List<BaseEntry> entryIDs;

        private static string BASE_ENTRY = "Script Entry";

        public script(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public script(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            int pos = 0;
            entryIDs = new List<BaseEntry>();
            while (pos < hxd.sectionData[0].Length)
            {
                BaseEntry entry = new BaseEntry();
                entry.entryLength =  DataMethods.readInt32(hxd.sectionData[0], pos);
                entry.entryID = DataMethods.readInt32(hxd.sectionData[0], pos + 4);

                ReadSpecialEntry(ref entry, pos);

                pos += entry.entryLength;
                entryIDs.Add(entry);
                entryCount++;
            }

            MetaBlock_Text("Entry Count: ", 1, 0x0, 0x0, 1);
            EditLastTextBox("" + entryCount, 110);

            createNode(BASE_ENTRY, entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                MetaBlock_Text("Entry Length: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entryIDs[i].entryLength, 110);

                MetaBlock_Text("Entry ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + entryIDs[i].entryID, 110);

                DisplaySpecialEntry(i);
            }
        }

        public void ReadSpecialEntry(ref BaseEntry entry, int entryOffset)
        {
            switch (entry.entryID)
            {
                default:
                    break;
                case 0x2:
                    entry = new FileEntry(entry.entryLength, entry.entryID);
                    (entry as FileEntry).fileUUID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 8);
                    break;
                case 0x3:
                    entry = new CaffEntry(entry.entryLength, entry.entryID);
                    (entry as CaffEntry).unk1 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x8);
                    (entry as CaffEntry).caffID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0xC);
                    (entry as CaffEntry).multiCaffID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x10);
                    break;
                case 0x12:
                    entry = new XMVEntry(entry.entryLength, entry.entryID);
                    (entry as XMVEntry).xmvID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x8);
                    (entry as XMVEntry).unk1 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0xC);
                    (entry as XMVEntry).unk2 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x10);
                    (entry as XMVEntry).unk3 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x14);
                    (entry as XMVEntry).unk4 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x18);
                    (entry as XMVEntry).unk5 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x1C);
                    break;
                case 0x4E:
                    entry = new MarkerEntry(entry.entryLength, entry.entryID);
                    (entry as MarkerEntry).fileUUID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 8);
                    (entry as MarkerEntry).unk1 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0xC);
                    break;
                case 0x50:
                    entry = new CommentEntry(entry.entryLength, entry.entryID);
                    (entry as CommentEntry).comment = DataMethods.readString(hxd.sectionData[0], entryOffset + 8, 0x100);
                    break;
                case 0x52:
                    entry = new ScriptEntry(entry.entryLength, entry.entryID);
                    (entry as ScriptEntry).fileUUID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 8);
                    break;
                case 0x60:
                    entry = new ChallengeEntry(entry.entryLength, entry.entryID);
                    (entry as ChallengeEntry).comment = DataMethods.readString(hxd.sectionData[0], entryOffset + 8, 0x40);
                    (entry as ChallengeEntry).fileUUID = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x48);
                    (entry as ChallengeEntry).unk1 = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x4C);
                    break;
                case 0x5D:
                case 0x89:
                case 0x90:
                    entry = new GameFlagEntry(entry.entryLength, entry.entryID);
                    (entry as GameFlagEntry).gameFlag = DataMethods.readString(hxd.sectionData[0], entryOffset + 8, 0x40);
                    (entry as GameFlagEntry).value = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 0x48);
                    break;
                case 0xA7:
                    entry = new AttractEntry(entry.entryLength, entry.entryID);

                    (entry as AttractEntry).xmvIDs = new int[(entry.entryLength - 8) / 4];
                    for (int i = 0; i < (entry as AttractEntry).xmvIDs.Length; i++)
                    {
                        (entry as AttractEntry).xmvIDs[i] = DataMethods.readInt32(hxd.sectionData[0], entryOffset + 8 + (4 * i));
                    }
                    break;
            }
        }

        public void DisplaySpecialEntry(int index)
        {
            var entry = entryIDs[index];

            switch (entry.entryID)
            {
                default :
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Unknown/Not Defined", 440);
                    break;
                case 0x0:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("End Of Script", 440);
                    break;
                case 0x2:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Load File", 440);

                    MetaBlock_Text("File UUID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as FileEntry).fileUUID, 110, 1);
                    break;
                case 0x3:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Load CAFF Files", 440);

                    MetaBlock_Text("UNK1: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as CaffEntry).unk1, 110, 1);

                    MetaBlock_Text("CAFF ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as CaffEntry).caffID, 110, 1);

                    MetaBlock_Text("MULTICAFF ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as CaffEntry).multiCaffID, 110, 1);

                    string[] path1 = GetHexStringArray((entryIDs[index] as CaffEntry).caffID);
                    string[] path2 = GetHexStringArray((entryIDs[index] as CaffEntry).multiCaffID);
                    string caffPath = $"{path1[0]}/{path1[1] + path1[2] + path1[3]}";
                    string multiCaffPath = $"{path2[0]}/{path2[1] + path2[2] + path2[3]}";

                    if((entryIDs[index] as CaffEntry).caffID != 0x00000000)
                    {
                        MetaBlock_String("CAFF Path: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                        EditLastStringBox($"GAME:Bundle/{caffPath}", 390);
                    }

                    if ((entryIDs[index] as CaffEntry).multiCaffID != 0x00000000)
                    {
                        MetaBlock_String("MULTICAFF Path: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                        EditLastStringBox($"GAME:Bundle/{multiCaffPath}", 390);
                    }
                    break;
                case 0x12:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Play XMV File(s)", 440);

                    MetaBlock_Text("XMV 1: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as XMVEntry).xmvID, 110, 1);

                    MetaBlock_Text("UNK1: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as XMVEntry).unk1, 110, 1);

                    string xmvPath = string.Join("/", GetHexStringArray((entryIDs[index] as XMVEntry).xmvID));

                    MetaBlock_String("Path: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox($"GAME:Debug/{xmvPath}", 440);
                    break;
                case 0x4E:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Load Marker File", 440);

                    MetaBlock_Text("File UUID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as MarkerEntry).fileUUID, 110, 1);

                    MetaBlock_Text("UNK1: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as MarkerEntry).unk1, 110, 1);
                    break;
                case 0x50:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Comment", 440);

                    MetaBlock_String("Comment: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("" + (entryIDs[index] as CommentEntry).comment, 430);
                    break;
                case 0x52:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Load Script File", 440);

                    MetaBlock_Text("File UUID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as ScriptEntry).fileUUID, 110, 1);
                    break;
                case 0x60:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Load Challenge File", 440);

                    MetaBlock_String("Comment: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("" + (entryIDs[index] as ChallengeEntry).comment, 430);

                    MetaBlock_Text("File UUID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as ChallengeEntry).fileUUID, 110, 1);

                    MetaBlock_Text("File UUID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as ChallengeEntry).unk1, 110, 1);
                    break;
                case 0x89:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Game Flag", 440);

                    MetaBlock_String("Game Flag: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("" + (entryIDs[index] as GameFlagEntry).gameFlag, 430);

                    MetaBlock_Text("UNK1: ", 1, 0x0, 0x0, 1, BASE_ENTRY, index);
                    EditLastTextBox("" + (entryIDs[index] as GameFlagEntry).value, 110, 1);
                    break;
                case 0xA7:
                    MetaBlock_String("TYPE: ", 1, 0x0, 0x0, BASE_ENTRY, index);
                    EditLastStringBox("Attract List", 440);

                    createNode("Attract List", (entryIDs[index] as AttractEntry).xmvIDs.Length);
                    for (int i = 0; i < (entryIDs[index] as AttractEntry).xmvIDs.Length; i++)
                    {
                        MetaBlock_Text($"XMV: ", 1, 0x0, 0x0, 1, "Attract List", i);
                        EditLastTextBox("" + (entryIDs[index] as AttractEntry).xmvIDs[i], 110, 1);

                        string path = string.Join("/", GetHexStringArray((entryIDs[index] as AttractEntry).xmvIDs[i]));

                        MetaBlock_String("Path: ", 1, 0x0, 0x0, "Attract List", i);
                        EditLastStringBox($"GAME:Debug/{path}", 440);
                    }
                    break;
            }
        }

        string[] GetHexStringArray(int hexValue)
        {
            byte[] array = BitConverter.GetBytes(hexValue);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            string[] pathSegments = new string[4];
            for (int p = 0; p < 4; p++)
            {
                pathSegments[p] = array[p].ToString("X2");
            }
            return pathSegments;
        }
    }
}
