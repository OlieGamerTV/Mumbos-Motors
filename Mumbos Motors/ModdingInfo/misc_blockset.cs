using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mumbos_Motors
{
    public class misc_blockset : ModdingTab
    {
        int itemCount = 0;
        int[] partAmount;
        int[] partID;

        int[] partIdents;
        string[] partNames;

        public misc_blockset(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public misc_blockset(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            VehicleParts.readPartStore(out partIdents, out partNames);
            int posCheck = 0;
            while(posCheck < hxd.sectionData[0].Length)
            {
                posCheck += 8;
                itemCount++;
            }

            partAmount = new int[itemCount];
            partID = new int[itemCount];
            for (int i = 0, p = 0; i < itemCount; i++, p += 8)
            {
                partAmount[i] = DataMethods.readInt32(hxd.sectionData[0], 0x0 + p);
                partID[i] = DataMethods.readInt32(hxd.sectionData[0], 0x4 + p);
            }

            string nodeName = "Parts";
            createNode(nodeName, itemCount);

            for(int i = 0, p = 0; i < itemCount; i++, p += 8)
            {
                int ident = DataMethods.readInt32(hxd.sectionData[0], 0x4 + p);
                MetaBlock_Text("Amount:", 1, 0x0 + p, 0x4, 0, nodeName, i);
                MetaBlock_Combo_Custom("Ident:", 1, 0x4 + p, 0x4, nodeName, i);
                metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox = createIdentList(metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox, ident, i);
            }
        }

        public ComboBox createIdentList(ComboBox box, int ident, int blockID)
        {
            box.Name = blockID + "";
            box.Items.Clear();
            for (int i = 0; i < partNames.Length; i++)
            {
                box.Items.Add(partNames[i]);
            }
            box.Text = getPartNameFromIdent(ident);
            return box;
        }

        public string getPartNameFromIdent(int ident)
        {
            for (int i = 0; i < partIdents.Length; i++)
            {
                if (partIdents[i] == ident)
                {
                    return partNames[i];
                }
            }
            return "unknown";
        }

        public int getIdentFromPartName(string name)
        {
            for (int i = 0; i < partIdents.Length; i++)
            {
                if (partNames[i] == name)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
