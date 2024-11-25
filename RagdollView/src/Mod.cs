using BoneLib;
using BoneLib.BoneMenu;

using Il2CppSLZ.Marrow;

using MelonLoader;

using UnityEngine;

namespace RagdollView;

public class RagdollViewMod : MelonMod 
{
    public const string Version = "1.2.0";

    public static MelonPreferences_Category MelonPrefCategory { get; private set; }
    public static MelonPreferences_Entry<bool> MelonPrefEnabled { get; private set; }

    public static bool IsEnabled { get; private set; }

    public static Page MainPage { get; private set; }
    public static BoolElement EnabledElement { get; private set; }

    private static bool _isRigDirty;

    private static bool _preferencesSetup = false;

    public override void OnInitializeMelon() 
    {
        SetupMelonPrefs();
        SetupBoneMenu();
    }

    public static void SetupMelonPrefs() 
    {
        MelonPrefCategory = MelonPreferences.CreateCategory("Ragdoll View");
        MelonPrefEnabled = MelonPrefCategory.CreateEntry("IsEnabled", true);

        IsEnabled = MelonPrefEnabled.Value;

        _preferencesSetup = true;
    }

    public static void SetupBoneMenu()
    {
        MainPage = Page.Root.CreatePage("Ragdoll View", Color.cyan);
        EnabledElement = MainPage.CreateBool("Mod Toggle", Color.yellow, IsEnabled, OnSetEnabled);
    }

    public static void OnSetEnabled(bool value) 
    {
        IsEnabled = value;
        MelonPrefEnabled.Value = value;
        MelonPrefCategory.SaveToFile(false);
    }

    public override void OnPreferencesLoaded() 
    {
        if (!_preferencesSetup)
        {
            return;
        }

        IsEnabled = MelonPrefEnabled.Value;

        EnabledElement.Value = IsEnabled;
    }

    public override void OnLateUpdate() 
    {
        OnParentView();
    }

    private static void OnParentView() 
    {
        var rm = Player.RigManager;

        if (rm == null)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            return;
        }

        var openControllerRig = rm.controllerRig.TryCast<OpenControllerRig>();

        var playspaceHead = openControllerRig.m_head.transform;
        var playspace = openControllerRig.transform;

        var physicsHead = rm.physicsRig.m_head;

        bool ragdolled = rm.physicsRig.torso.shutdown || !rm.physicsRig.ballLocoEnabled;

        if (IsEnabled && ragdolled && !rm.activeSeat)
        {
            playspace.localPosition = Vector3.zero;
            playspace.localRotation = Quaternion.identity;

            playspace.rotation = Quaternion.identity;
            playspace.rotation = physicsHead.rotation * Quaternion.Inverse(playspaceHead.rotation);

            playspace.position += physicsHead.position - playspaceHead.position;

            _isRigDirty = true;
        }
        else if (_isRigDirty)
        {
            playspace.localPosition = Vector3.zero;
            playspace.localRotation = Quaternion.identity;
            rm.remapHeptaRig.SetTwist(0f);
            _isRigDirty = false;
        }
    }
}
