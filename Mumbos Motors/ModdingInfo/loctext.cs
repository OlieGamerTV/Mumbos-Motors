using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors
{
    public class loctext : ModdingTab
    {
        /*
        [LOCTEXT File Structure]
        --- Main File Header ---
        0x00 4 byte Magic Header (Should be LSBL)
        0x04 ???
        0x08 ???
        0x0C ???
        0x10 Tags Table Offset (LE Int32, Relative to LSBL Header)
        0x14 Debug Label Table Offset (LE Int32, Relative to LSBL Header)
        0x18 ??? Table Offset (LE Int32, Relative to LSBL Header)
        Following this is the String Table.
        --- String Table Header ---
        0x00 Total Section Length (LE Int32)
        0x04 Total Strings (LE Int32)
        0x08 ???
        Following this is the String Info Table.
        --- String Info Table ---
        Each entry for the total of strings you have is the following:
        0x00 String ID (BE Int32)
        0x04 Total Length In Sequence (LE Int16)
        After that, the strings are stored as chars separated by null chars. Ignoring the initial nulls, read each string up to their specified length, skipping each null char.
        
        --- Tags Table ---
        0x00 Total Section Length (LE Int32)
        0x04 Total Tags (LE Int32)
        --- Tags Info Table ---
        Each entry for the total of strings you have is the following:
        0x00 Tag ID (?? Int16) (May not be entirely correct)
        0x02 ??? (LE Int16)
        0x04 Tag String Offset (LE Int32, relative to the start of the tag strings table.)
        Following this is a table of tag strings, these ones are just straight plain text and can be read until a null char is hit.
        */

        private static string STRING_NODE = "Strings";
        private static string COMMENTS_NODE = "Comments";

        byte[][] tempSectionData;

        long LSBL_Start = 0;

        // Strings
        int stringsCount;
        int stringsTableLength;
        int[] stringIDTable;
        int[] stringLengthTotalTable;
        string[] locStrings;

        // String Tags
        int tagsTableOffset;
        int tagsCount;
        int tagsTableLength;
        int[] tagsIDTable;
        int[] tagsUNK2;
        int[] tagStrOffsetTable;
        string[] tagsTable;

        // Comments
        int commentTableOffset;
        int commentTableLength;
        int commentCount;
        int[] commentID;
        int[] commentOffset;
        string[] commentsTable;

        public loctext(CAFF caff, int fileID) : base(caff, fileID)
        {
            buildMetaPage();
        }

        public loctext(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            tempSectionData = hxd.sectionData;

            // Read until we find the magic header.
            // Yes, this is actually needed, aid_loctext_banjox_challenges has a string placed before the LSBL header which differs from most of the files.
            char[] magic = new char[4];
            char magicChar = '\0';
            int magicSearch = 0;
            int magicPos = 0;
            while (true)
            {
                magicChar = (char)DataMethods.readInt(hxd.sectionData[0], magicSearch, 1);
                magicSearch++;
                switch (magicChar)
                {
                    default:
                        magic = new char[4];
                        magicPos = 0;
                        break;
                    case 'L':
                    case 'S':
                    case 'B':
                        magicPos++;
                        break;
                }

                if (magicPos == 4)
                {
                    LSBL_Start = magicSearch - 4;
                    break;
                }
                if (magicSearch > 512) return; // Unless a file proves otherwise, I'm going to add a cap on the magic search to prevent lengthy lock-ups.
            }
            Console.WriteLine("LSBL START - " + LSBL_Start);

            // Read String Count
            byte[] length = new byte[4]; 
            Array.ConstrainedCopy(hxd.sectionData[0], (int)LSBL_Start + 0x20, length, 0, 4);
            stringsCount = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);

            // Read Tag Table Offset
            Array.ConstrainedCopy(hxd.sectionData[0], (int)LSBL_Start + 0x10, length, 0, 4); 
            tagsTableOffset = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);

            // Read Comments Table Offset
            Array.ConstrainedCopy(hxd.sectionData[0], (int)LSBL_Start + 0x14, length, 0, 4);
            commentTableOffset = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);

            // Bulk Read the String Information.
            ReadStringInfo();

            // Bulk Read the Tag Information
            ReadTagInfo();

            // Check if the Comment Table Offset isn't 0, skip running this if it is.
            if(commentTableOffset != 0) ReadCommentInfo();

            // The rest of this is dedicated to parsing strings using BinaryReader.
            // Bulk Read the strings and add them to the node list.
            BinaryReader reader = new BinaryReader(new MemoryStream(hxd.sectionData[0]), Encoding.UTF7);
            long stringStart = (int)LSBL_Start + 0x28 + (6 * stringsCount);
            reader.BaseStream.Seek(stringStart, SeekOrigin.Begin);
            locStrings = new string[stringsCount];
            createNode(STRING_NODE, stringsCount);
            int readChars = 0;
            for (int i = 0, s = 1; i < stringsCount; i++, s++)
            {
                string tempStr = "";
                char temp = '\0';
                //reader.ReadChar();
                //if (s == 1) reader.ReadChar(); //Skip an extra blank char.
                while (readChars < stringLengthTotalTable[i] * 2)
                {
                    temp = reader.ReadChar();
                    tempStr += temp;
                    readChars++;
                }
                locStrings[i] = tempStr.Replace("\0", "");
                Console.WriteLine(locStrings[i] + " - " + stringLengthTotalTable[i]);
                MetaBlock_Combo_Custom("String:", 1, 0, 0, STRING_NODE, i);
                SetupStringTab(locStrings[i]);
            }

            // Bulk Read the tags and add them to the node list.
            tagsTable = new string[tagsCount];
            for (int i = 0, s = 1; i < tagsCount; i++, s++)
            {
                reader.BaseStream.Seek((tagsTableOffset + (int)LSBL_Start + 0x8) + (tagsCount * 8) + tagStrOffsetTable[i], SeekOrigin.Begin);
                string tempStr = "";
                char temp = '\0';
                while ((temp = reader.ReadChar()) != '\0')
                {
                    tempStr += temp;
                }
                Console.WriteLine(tempStr);
                tagsTable[i] = tempStr;
                MetaBlock_Combo_Custom("Tag:", 1, 0, 0, STRING_NODE, i);
                SetupStringTab(tempStr);
            }

            if(commentTableOffset != 0)
            {
                createNode(COMMENTS_NODE, commentCount);
                // Bulk Read the comments and add them to the node list.
                commentsTable = new string[commentCount];
                for (int i = 0, s = 1; i < commentCount; i++, s++)
                {
                    reader.BaseStream.Seek((commentTableOffset + (int)LSBL_Start + 0x8) + (commentCount * 8) + commentOffset[i], SeekOrigin.Begin);
                    string tempStr = "";
                    char temp = '\0';
                    while ((temp = reader.ReadChar()) != '\0')
                    {
                        tempStr += temp;
                    }
                    Console.WriteLine(tempStr);
                    commentsTable[i] = tempStr;
                    MetaBlock_Combo_Custom("Comment:", 1, 0, 0, COMMENTS_NODE, i);
                    SetupCommentTab(tempStr);
                }
            }

            // Display the rest of the information.
            MetaBlock_Text("Tags Table Offset: ", 1, (int)LSBL_Start + 0x10, 0x4, 1);
            MetaBlock_Text("Comments Offset: ", 1, (int)LSBL_Start + 0x14, 0x4, 1);
            MetaBlock_Text("UNK1 Offset: ", 1, (int)LSBL_Start + 0x18, 0x4, 1);

            MetaBlock_Text("String Table Length: ", 1, (int)LSBL_Start + 0x1C, 0x4, 3);
            MetaBlock_Text("Strings Count: ", 1, (int)LSBL_Start + 0x20, 0x4, 3);

            MetaBlock_Text("Tags Table Length: ", 1, tagsTableOffset + (int)LSBL_Start, 0x4, 3);
            MetaBlock_Text("Tags Count: ", 1, tagsTableOffset + (int)LSBL_Start + 0x4, 0x4, 3);

            metaTab.buildMetaFunPanel();
        }

        private void ReadStringInfo()
        {
            // Bulk Read the String Information.
            stringIDTable = new int[stringsCount];
            stringLengthTotalTable = new int[stringsCount];
            locStrings = new string[stringsCount];
            byte[] unk1 = new byte[4];
            byte[] unk2 = new byte[2];
            for (int i = 0; i < stringsCount; i++)
            {
                stringIDTable[i] = DataMethods.readInt32(hxd.sectionData[0], (int)LSBL_Start + 0x28 + (6 * i));
                Array.ConstrainedCopy(hxd.sectionData[0], (int)LSBL_Start + 0x2C + (6 * i), unk2, 0, 2);
                stringLengthTotalTable[i] = DataMethods.readInt(DataMethods.swapEndianness(unk2, 2), 0, 2);
                Console.WriteLine($"STR ID - {stringIDTable[i]}, STR TOTAL LENGTH - {stringLengthTotalTable[i]}");
            }
        }

        private void ReadTagInfo()
        {
            // Bulk Read the Tag Information
            byte[] length = new byte[4];
            Array.ConstrainedCopy(hxd.sectionData[0], tagsTableOffset + (int)LSBL_Start, length, 0, 4);
            tagsTableLength = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);
            Array.ConstrainedCopy(hxd.sectionData[0], tagsTableOffset + (int)LSBL_Start + 0x4, length, 0, 4);
            tagsCount = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);
            tagsIDTable = new int[tagsCount];
            tagsUNK2 = new int[tagsCount];
            tagStrOffsetTable = new int[tagsCount];
            byte[] unk1 = new byte[2];
            byte[] unk2 = new byte[2];
            byte[] tagStrOffset = new byte[4];
            for (int i = 0; i < stringsCount; i++)
            {
                Array.ConstrainedCopy(hxd.sectionData[0], (tagsTableOffset + (int)LSBL_Start + 0x8) + (8 * i), unk1, 0, 2);
                tagsIDTable[i] = DataMethods.readInt(unk1, 0, 2);
                Array.ConstrainedCopy(hxd.sectionData[0], (tagsTableOffset + (int)LSBL_Start + 0xA) + (8 * i), unk2, 0, 2);
                tagsUNK2[i] = DataMethods.readInt(DataMethods.swapEndianness(unk2, 2), 0, 2);
                Array.ConstrainedCopy(hxd.sectionData[0], (tagsTableOffset + (int)LSBL_Start + 0xC) + (8 * i), tagStrOffset, 0, 4);
                tagStrOffsetTable[i] = DataMethods.readInt(DataMethods.swapEndianness(tagStrOffset, 4), 0, 4);
                Console.WriteLine($"TAGS ID - {tagsIDTable[i]}, TAGS UNK2 - {tagsUNK2[i]}, TAGS STR OFFSET - {tagStrOffsetTable[i]}");
            }
        }

        private void ReadCommentInfo()
        {
            byte[] length = new byte[4];
            Array.ConstrainedCopy(hxd.sectionData[0], commentTableOffset + (int)LSBL_Start, length, 0, 4);
            commentTableLength = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);
            Array.ConstrainedCopy(hxd.sectionData[0], commentTableOffset + (int)LSBL_Start + 0x4, length, 0, 4);
            commentCount = DataMethods.readInt(DataMethods.swapEndianness(length, 4), 0, 0x4);
            commentID = new int[commentCount];
            commentOffset = new int[commentCount];
            byte[] id = new byte[4];
            byte[] offset = new byte[4];
            for (int i = 0; i < commentCount; i++)
            {
                Array.ConstrainedCopy(hxd.sectionData[0], (commentTableOffset + (int)LSBL_Start + 0x8) + (8 * i), id, 0, 4);
                commentID[i] = DataMethods.readInt(id, 0, 4);
                Array.ConstrainedCopy(hxd.sectionData[0], (commentTableOffset + (int)LSBL_Start + 0xC) + (8 * i), offset, 0, 4);
                commentOffset[i] = DataMethods.readInt(DataMethods.swapEndianness(offset, 4), 0, 4);
                Console.WriteLine($"COMMENT ID - {commentID[i]}, COMMENT OFFSET - {commentOffset[i]}");
            }
        }

        private void SetupStringTab(string str, int offset = 1)
        {
            // Yes, it's a very hacky way to display it as I didn't want to make too many adjustments, but it works for now.
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Text = str;
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Width = 460;
            var pos = metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Location;
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Location = new System.Drawing.Point(pos.X - 395, pos.Y);
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].comboBox.Enabled = false;
        }

        private void SetupCommentTab(string str, int offset = 1)
        {
            // Yes, it's a very hacky way to display it as I didn't want to make too many adjustments, but it works for now.
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Text = str;
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Width = 440;
            var pos = metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Location;
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].textBox.Location = new System.Drawing.Point(pos.X - 375, pos.Y);
            metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - offset].comboBox.Enabled = false;
        }
    }
}
