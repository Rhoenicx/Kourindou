using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.GameContent.UI.Elements;

namespace Kourindou.UI
{
    internal class CustomHairMenu : UIState
    {
        private DragableUIPanel _CustomHairPanel;

        public static bool Visible;

        public override void OnInitialize()
        {
            _CustomHairPanel = new DragableUIPanel();
            _CustomHairPanel.SetPadding(10);
			_CustomHairPanel.Left.Set(10f, 0.1f);
			_CustomHairPanel.Top.Set(10f, 0.5f);
			_CustomHairPanel.Width.Set(400f, 0f);
			_CustomHairPanel.Height.Set(320f, 0f);
			_CustomHairPanel.BackgroundColor = new Color(73, 94, 171);

            Append(_CustomHairPanel);
        }

        internal void UpdateHairList()
        {

        }
    }
}