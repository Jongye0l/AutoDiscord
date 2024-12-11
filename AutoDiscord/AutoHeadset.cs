using System;
using System.Collections.Generic;
using System.Diagnostics;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using MonsterLove.StateMachine;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AutoDiscord;

public class AutoHeadset : Feature {
    public static bool StartFirst;
    public static bool Muted;
    public static List<AudioSessionControl> MutedSessions = [];
    public static AutoHeadsetSettings Settings;
    public static MMDeviceEnumerator deviceEnumerator;
    public static MMDevice device;
    public static AudioSessionManager sessionManager;
    public readonly SettingGUI settingGUI = new(Main.Instance);
    private float guiMutePercent;
    private string guiMutePercentString;

    public AutoHeadset() : base(Main.Instance, nameof(AutoHeadset), true, typeof(AutoHeadset), typeof(AutoHeadsetSettings)) {
        Settings = (AutoHeadsetSettings) Setting;
        guiMutePercent = Settings.MutePercent * 100;
    }

    protected override void OnEnable() {
        deviceEnumerator = new MMDeviceEnumerator();
        device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        sessionManager = device.AudioSessionManager;
        Application.quitting += OnDisable;
        if(!Settings.AnnounceAlreadyMuted) return;
        SessionCollection sessions = sessionManager.Sessions;
        List<AudioSessionControl> mutedDiscordSessions = [];
        for(int i = 0; i < sessions.Count; i++) {
            try {
                AudioSessionControl session = sessions[i];
                if(session.GetProcessID != 0 && session.SimpleAudioVolume.Mute && Process.GetProcessById((int) session.GetProcessID).ProcessName.ToLower().Contains("discord")) {
                    mutedDiscordSessions.Add(session);
                }
            } catch (Exception e) {
                Main.Instance.LogException(e);
            }
        }
        if(mutedDiscordSessions.Count > 0) _ = new MuteAnnounce(mutedDiscordSessions);
    }

    protected override void OnDisable() {
        Application.quitting -= OnDisable;
        Unmute();
        deviceEnumerator.Dispose();
    }

    protected override void OnGUI() {
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.OnlyStartZero, localization["AutoHeadset.OnlyStartZero"]);
        settingGUI.AddSettingSliderFloat(ref guiMutePercent, 80, ref guiMutePercentString, localization["AutoHeadset.MutePercent"], 0, 100, MutePercentUpdate);
        settingGUI.AddSettingToggle(ref Settings.IgnoreNoFail, localization["AutoHeadset.IgnoreNoFail"]);
        settingGUI.AddSettingToggle(ref Settings.AnnounceAlreadyMuted, localization["AutoHeadset.AnnounceAlreadyMuted"]);
    }

    private void MutePercentUpdate() => Settings.MutePercent = guiMutePercent / 100;

    [JAPatch(typeof(scrPlanet), "MoveToNextFloor", PatchType.Postfix, false)]
    public static void UpdatePercent() {
        if(Muted || Settings.OnlyStartZero && !StartFirst || Settings.IgnoreNoFail && scrController.instance.noFail || !ADOBase.lm) return;
        if(scrController.instance.percentComplete >= Settings.MutePercent && scrController.instance.percentComplete != 1) {
            SessionCollection sessions = sessionManager.Sessions;
            for(int i = 0; i < sessions.Count; i++) {
                try {
                    AudioSessionControl session = sessions[i];
                    if(session.GetProcessID != 0 && !session.SimpleAudioVolume.Mute && Process.GetProcessById((int) session.GetProcessID).ProcessName.ToLower().Contains("discord")) {
                        session.SimpleAudioVolume.Mute = true;
                        MutedSessions.Add(session);
                    }
                } catch (Exception e) {
                    Main.Instance.LogException(e);
                }
            }
            Muted = true;
        }
    }

    [JAPatch(typeof(scrUIController), "WipeToBlack", PatchType.Postfix, false)]
    [JAPatch(typeof(scnEditor), "ResetScene", PatchType.Postfix, false)]
    [JAPatch(typeof(scrController), "StartLoadingScene", PatchType.Postfix, false)]
    public static void Unmute() {
        if(!Muted) return;
        foreach(AudioSessionControl session in MutedSessions) {
            try {
                session.SimpleAudioVolume.Mute = false;
            } catch (Exception e) {
                Main.Instance.LogException(e);
            }
        }
        MutedSessions.Clear();
        Muted = false;
    }

    [JAPatch(typeof(StateBehaviour), "ChangeState", PatchType.Postfix, true, ArgumentTypesType = [typeof(Enum)])]
    public static void OnChangeState(Enum newState) {
        if((States) newState is States.Fail or States.Won) Unmute();
    }

    [JAPatch(typeof(scnGame), "Play", PatchType.Postfix, false)]
    [JAPatch(typeof(scrPressToStart), "ShowText", PatchType.Postfix, false)]
    public static void OnStart() {
        StartFirst = scrController.instance.currentSeqID == 0;
        Unmute();
    }

    public class AutoHeadsetSettings(JAMod mod, JObject jsonObject = null) : JASetting(mod, jsonObject) {
        public bool OnlyStartZero = true;
        public float MutePercent = 0.8f;
        public bool IgnoreNoFail = true;
        public bool AnnounceAlreadyMuted = true;
    }
}