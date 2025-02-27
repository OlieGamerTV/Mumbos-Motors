using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors
{
    public class garageGrouping : ModdingTab
    {
        class Group
        {
            public string name, group, gameFlag;
        }

        List<Group> groups = new List<Group>(); 

        public garageGrouping(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public garageGrouping(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            string label = "Groupings";
            groups = new List<Group>();

            BinaryReader reader = new BinaryReader(new MemoryStream(hxd.sectionData[0]));
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Group group = new Group();
                group.name = string.Join("", reader.ReadChars(0x40)).Trim();
                group.group = string.Join("", reader.ReadChars(0x40)).Trim();
                int unk1 = reader.ReadInt32();
                int unk2 = reader.ReadInt32();
                group.gameFlag = string.Join("", reader.ReadChars(0x40)).Trim();
                int unk3 = reader.ReadInt32();
                groups.Add(group);
            }

            createNode(label, groups.Count);

            for (int i = 0; i < groups.Count; i++)
            {
                MetaBlock_String("Name Tag:", 1, 0, 0, label, i);
                EditLastStringBox(groups[i].name, 430);

                MetaBlock_String("Group Tag:", 1, 0, 0, label, i);
                EditLastStringBox(groups[i].group, 430);

                MetaBlock_String("Game Flag:", 1, 0, 0, label, i);
                EditLastStringBox(groups[i].gameFlag, 430);
            }
        }
    }
}
