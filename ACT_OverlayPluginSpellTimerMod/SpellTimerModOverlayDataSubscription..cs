using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin.Common.Models;
using RainbowMage.OverlayPlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Web.Script.Serialization;

namespace SpellTimerMod
{
    public class SpellTimerModOverlay : OverlayBase<SpellTimerModOverlayConfig>
    {
        private Dictionary<uint, Combatant> combatantTable = new Dictionary<uint, Combatant>();
        private List<Combatant> partyCombatantList = new List<Combatant>();
        private IActPluginV1 ffxivPlugin;

        public SpellTimerModOverlay(SpellTimerModOverlayConfig config)
            : base(config, config.Name)
        {
            var timer = new Timer(new TimerCallback((state) =>
            {
                foreach (ActPluginData PluginData in ActGlobals.oFormActMain.ActPlugins)
                {
                    if (PluginData.pluginFile.Name.Contains("FFXIV_ACT_Plugin.dll") && PluginData.lblPluginStatus.Text.Contains("FFXIV Plugin Started."))
                    {
                        ffxivPlugin = PluginData.pluginObj;
                    }
                }
                if (ffxivPlugin != null)
                {
                    dynamic plugin_derived = ffxivPlugin;
                    plugin_derived.DataSubscription.CombatantAdded += new FFXIV_ACT_Plugin.Common.CombatantAddedDelegate(CombatantAdded);
                    plugin_derived.DataSubscription.CombatantRemoved += new FFXIV_ACT_Plugin.Common.CombatantRemovedDelegate(CombatantRemoved);
                    plugin_derived.DataSubscription.PartyListChanged += new FFXIV_ACT_Plugin.Common.PartyListChangedDelegate(PartyListChanged);
                    plugin_derived.DataSubscription.ZoneChanged += new FFXIV_ACT_Plugin.Common.ZoneChangedDelegate(ZoneChanged);
                }
                (state as Timer).Dispose();
            }));
            timer.Change(5000, 0);
        }

        private void ZoneChanged(uint ZoneID, string ZoneName)
        {
            this.partyCombatantList.Clear();
        }
        private void CombatantAdded(object Combatant)
        {
            Combatant cmb = (Combatant)Combatant;
            this.combatantTable[cmb.ID] = cmb;
        }
        private void CombatantRemoved(object Combatant)
        {
            Combatant cmb = (Combatant)Combatant;
            if (this.combatantTable.ContainsKey(cmb.ID))
            {
                this.combatantTable.Remove(cmb.ID);
            }
        }
        private void PartyListChanged(ReadOnlyCollection<uint> partyList, int partySize)
        {
            this.partyCombatantList.Clear();
            foreach (uint i in partyList)
            {
                Combatant cmb = null;
                if (this.combatantTable.TryGetValue(i , out cmb))
                {
                    this.partyCombatantList.Add(cmb);
                }
            }
        }

        protected override void Update()
        {
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
                t.combatant = timerFrame.Combatant;
                t.key = timerFrame.Name + "_" + timerFrame.Combatant.Replace(" ", "_");

                t.timeLeft = timerFrame.GetLargestVal(false);
                t.startTime = timerFrame.GetMostRecentTime(false).ToString("yyyy-MM-ddTHH:mm:ss");

                t.startCount = timerFrame.TimerData.TimerValue;
                t.warningCount = timerFrame.TimerData.WarningValue;
                t.expireCount = timerFrame.TimerData.RemoveValue;

                t.color = timerFrame.TimerData.FillColor.ToArgb();

                t.job = string.Empty;
                foreach (Combatant cmb in this.partyCombatantList)
                {
                    if (cmb.Name.ToLower() == timerFrame.Combatant.ToLower())
                    {
                        t.job = JobIDtoJob(cmb.Job);
                        t.combatant = cmb.Name;
                        break;
                    }
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

        private string JobIDtoJob(int jobid)
        {
            if (jobid <= 0)
            {
                return "";
            }
            if (Enum.IsDefined(typeof(JobEnum), jobid))
            {
                return Enum.GetName(typeof(JobEnum), jobid);
            }
            return "";
        }
        private enum JobEnum : byte
        {
            Adv,
            Gla,
            Pgl,
            Mrd,
            Lnc,
            Arc,
            Cnj,
            Thm,
            Crp,
            Bsm,
            Arm,
            Gsm,
            Ltw,
            Wvr,
            Alc,
            Cul,
            Min,
            Btn,
            Fsh,
            Pld,
            Mnk,
            War,
            Drg,
            Brd,
            Whm,
            Blm,
            Acn,
            Smn,
            Sch,
            Rog,
            Nin,
            Mch,
            Drk,
            Ast,
            Sam,
            Rdm,
            Blu,
            Chocobo = 250
        }
    }
}
