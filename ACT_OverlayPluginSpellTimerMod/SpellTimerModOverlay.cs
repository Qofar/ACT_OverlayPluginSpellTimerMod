using Advanced_Combat_Tracker;
using RainbowMage.OverlayPlugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Script.Serialization;

namespace ACT_OverlayPluginSpellTimerMod
{
    public class SpellTimerModOverlay : OverlayBase<SpellTimerModOverlayConfig>
    {
        private static readonly TextInfo textinfo = new CultureInfo("en-US", false).TextInfo;
        private static Dictionary<string, string> NAMEtoJOB = new Dictionary<string, string>();
        private static string CurrentZoneName = string.Empty;

        public SpellTimerModOverlay(SpellTimerModOverlayConfig config)
            : base(config, config.Name)
        {
        }

        protected override void Update()
        {
            if (CurrentZoneName != ActGlobals.oFormActMain.ActiveZone.ZoneName)
            {
                NAMEtoJOB.Clear();
                CurrentZoneName = ActGlobals.oFormActMain.ActiveZone.ZoneName;
            }
            if (ActGlobals.oFormActMain.ActiveZone.ActiveEncounter != null)
            {
                List<CombatantData> allies = ActGlobals.oFormActMain.ActiveZone.ActiveEncounter.GetAllies();
                foreach (CombatantData cd in allies)
                {
                    string job = cd.GetColumnByName("Job");
                    if (job != string.Empty)
                    {
                        NAMEtoJOB[cd.Name.ToLower()] = job;
                    }
                }
            }

            try
            {
                var updateScript = CreateEventDispatcherScript();

                if (this.Overlay != null &&
                    this.Overlay.Renderer != null &&
                    this.Overlay.Renderer.Browser != null)
                {
                    this.Overlay.Renderer.Browser.GetMainFrame().ExecuteJavaScript(updateScript, null, 0);
                }

            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Update: {1}", this.Name, ex);
            }
        }

        internal string CreateJsonData()
        {
            // Overlay に渡すオブジェクト
            List<TimerInfo> tf = new List<TimerInfo>();

            List<TimerFrame> timerFrames = ActGlobals.oFormSpellTimers.GetTimerFrames();
            foreach (TimerFrame timerFrame in timerFrames)
            {
                TimerInfo t = new TimerInfo();
                t.name = timerFrame.Name;
                t.combatant = FormatCharName(timerFrame.Combatant);
                t.key = timerFrame.Name + "_" + t.combatant.Replace(" ", "_");

                t.startTime = timerFrame.GetMostRecentTime(false).ToString("o"); // ISO 8601 "2019-09-09T01:03:43.7610000+09:00"
                t.timeLeft = timerFrame.GetLargestVal(false);

                t.startCount = timerFrame.TimerData.TimerValue;
                t.warningCount = timerFrame.TimerData.WarningValue;
                t.expireCount = timerFrame.TimerData.RemoveValue;

                t.color = timerFrame.TimerData.FillColor.ToArgb();

                string job = string.Empty;
                if (NAMEtoJOB.TryGetValue(timerFrame.Combatant, out job))
                {
                    t.job = job;
                }
                tf.Add(t);
            }

            // シリアライザ
            return new JavaScriptSerializer().Serialize(tf);
        }

        private string CreateEventDispatcherScript()
        {
            return "var ActXiv = { 'timerFrames': " + this.CreateJsonData() + " };\n" +
               "document.dispatchEvent(new CustomEvent('onOverlayDataUpdate', { detail: ActXiv }));";
        }

        private string FormatCharName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            switch (name)
            {
                case "you":
                    return "YOU";
                case "unknown":
                    return "unknown";
                default:
                    return textinfo.ToTitleCase(name);
            }
        }

        //// JSON用オブジェクト
        private class TimerInfo
        {
            public string name { get; set; }
            public string combatant { get; set; }
            public string key { get; set; }
            public string job { get; set; }
            public string startTime { get; set; }
            public int startCount { get; set; }
            public int warningCount { get; set; }
            public int expireCount { get; set; }
            public int timeLeft { get; set; }
            public int color { get; set; }
        }
    }
}
