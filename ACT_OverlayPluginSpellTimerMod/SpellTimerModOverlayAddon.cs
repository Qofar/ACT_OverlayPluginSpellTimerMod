using RainbowMage.OverlayPlugin;
using System;
using System.Reflection;

namespace ACT_OverlayPluginSpellTimerMod
{
    public class SpellTimerModOverlayAddon : IOverlayAddon
    {
        public static string ResourcesDirectory = String.Empty;

        public SpellTimerModOverlayAddon()
        {
            // OverlayPlugin.Coreを期待
            Assembly asm = System.Reflection.Assembly.GetCallingAssembly();
            if (asm.Location == null || asm.Location == "")
            {
                // 場所がわからないなら自分の場所にする
                asm = Assembly.GetExecutingAssembly();
            }
            ResourcesDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(asm.Location), "resources");
        }

        public string Name
        {
            get { return "Spell Timer Mod"; }
        }

        public string Description
        {
            get { return ""; }
        }

        public Type OverlayType
        {
            get { return typeof(SpellTimerModOverlay); }
        }

        public Type OverlayConfigType
        {
            get { return typeof(SpellTimerModOverlayConfig); }
        }

        public Type OverlayConfigControlType
        {
            get { return typeof(SpellTimerModConfigPanel); }
        }

        public IOverlay CreateOverlayInstance(IOverlayConfig config)
        {
            return new SpellTimerModOverlay((SpellTimerModOverlayConfig)config);
        }

        public IOverlayConfig CreateOverlayConfigInstance(string name)
        {
            return new SpellTimerModOverlayConfig(name);
        }

        public System.Windows.Forms.Control CreateOverlayConfigControlInstance(IOverlay overlay)
        {
            return new SpellTimerModConfigPanel((SpellTimerModOverlay)overlay);
        }

        public void Dispose()
        {

        }
    }
}
