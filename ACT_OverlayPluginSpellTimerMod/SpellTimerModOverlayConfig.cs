using RainbowMage.OverlayPlugin;
using System;

namespace ACT_OverlayPluginSpellTimerMod
{
    [Serializable]
    public class SpellTimerModOverlayConfig : OverlayConfigBase
    {
        public SpellTimerModOverlayConfig(string name) : base(name)
        {
            this.Url = new Uri(System.IO.Path.Combine(SpellTimerModOverlayAddon.ResourcesDirectory, @"spelltimermod.html")).ToString();
        }

        // XmlSerializer用
        private SpellTimerModOverlayConfig() : base(null)
        {

        }

        public override Type OverlayType
        {
            get { return typeof(SpellTimerModOverlay); }
        }
    }
}
