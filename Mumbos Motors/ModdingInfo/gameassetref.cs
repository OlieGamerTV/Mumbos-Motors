using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mumbos_Motors.marker;

namespace Mumbos_Motors.ModdingInfo
{
    public class gameassetref : ModdingTab
    {
        class AssetRef
        {
            public string filename;
            public int id;
        }

        private static string BASE_ENTRY = "Asset Reference Entry";

        List<AssetRef> assetReferences;

        public gameassetref(CAFF caff, int symbolID) : base(caff, symbolID)
        {
            buildMetaPage();
        }

        public gameassetref(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            assetReferences = new List<AssetRef>();
            int pos = 0;

            while(pos < hxd.sectionData[0].Length)
            {
                AssetRef assetRef = new AssetRef();

                assetRef.filename = DataMethods.readString(hxd.sectionData[0], pos, 0x40);
                assetRef.id = DataMethods.readInt32(hxd.sectionData[0], pos + 0x40);

                assetReferences.Add(assetRef);

                pos += 0x44;
            }

            createNode(BASE_ENTRY, assetReferences.Count);
            for (int i = 0; i < assetReferences.Count; i++)
            {
                MetaBlock_String("Filename: ", 1, 0x0, 0x0, BASE_ENTRY, i);
                EditLastStringBox("" + assetReferences[i].filename, 380);

                MetaBlock_Text("ID: ", 1, 0x0, 0x0, 1, BASE_ENTRY, i);
                EditLastTextBox("" + assetReferences[i].id, 110, 1);
            }
        }
    }
}
