using System;
using System.Collections.Generic;
using System.IO;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace PitchBlack;

public static class MenuHooks
{
    public static ProcessManager.ProcessID PitchBlackCollection => new ProcessManager.ProcessID(SINGAL_NAME, true);
    const string SINGAL_NAME = "PitchBlackCollection";
    public static void Apply() {
        On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        IL.ProcessManager.PostSwitchMainProcess += IL_ProcessManager_PostSwitchMainProcess;
        On.MoreSlugcats.CollectionsMenu.ctor += MoreSlugcats_CollectionsMenu_ctor;
        On.MoreSlugcats.CollectionsMenu.Singal += MoreSlugcats_CollectionsMenu_Singal;
    }
    private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
    {
        bool wasPBScene = false;
        if (self.sceneID == PitchBlackCollectionMenu.PBCollectionScene) {
            string regionToDisplay = File.ReadAllText(Plugin.regionMenuDisplaySavePath);
            self.sceneID = Region.GetRegionLandscapeScene(regionToDisplay);
            wasPBScene = true;
        }

        orig(self);

        if (wasPBScene) {
            self.sceneID = PitchBlackCollectionMenu.PBCollectionScene;
        }

        if (self.sceneID == PitchBlackCollectionMenu.PBCollectionScene) {
            for (int i = self.depthIllustrations.Count-1; i >= 0; i--) {
                self.depthIllustrations[i].sprite.MoveToBack();
                self.depthIllustrations[i].depth *= 3;
            }

            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "collectionMenu";
            MenuDepthIllustration depthIllustration = new MenuDepthIllustration(self.menu, self, self.sceneFolder, "hologrambkg", new Vector2(803.9481f, -17.7778f), 10f, MenuDepthIllustration.MenuShader.Normal);
            depthIllustration.sprite.scaleY *= 1.7f;
            depthIllustration.sprite.scaleX *= 1.15f;
            self.AddIllustration(depthIllustration);
            (self as InteractiveMenuScene).idleDepths.AddRange(new List<float>{9, 11});
        }
    }
    private static void IL_ProcessManager_PostSwitchMainProcess(ILContext il) {
        var cursor = new ILCursor(il);

        try {
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                                        i => i.MatchLdfld<ProcessManager>("oldProcess"),
                                                        i => i.MatchLdarg(0),
                                                        i => i.MatchLdfld<ProcessManager>("currentMainLoop"),
                                                        i => i.MatchCallOrCallvirt<MainLoopProcess>("CommunicateWithUpcomingProcess")))
            {
                throw new Exception("Failed to match IL for ProcessManager_PostSwitchMainProcess!");
            }
        }
        catch (Exception ex) {
            Debug.LogError("Exception when matching IL for ProcessManager_PostSwitchMainProcess!");
            Debug.LogException(ex);
            Debug.LogError(il);
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
        {
            if (ID == PitchBlackCollection)
            {
                self.currentMainLoop = new PitchBlackCollectionMenu(self);
            }
        });

    }
    private static void MoreSlugcats_CollectionsMenu_Singal(On.MoreSlugcats.CollectionsMenu.orig_Singal orig, MoreSlugcats.CollectionsMenu self, MenuObject sender, string message)
    {
        orig(self, sender, message);
        if (message == SINGAL_NAME) {
            self.manager.RequestMainProcessSwitch(PitchBlackCollection);
            self.PlaySound(SoundID.MENU_Switch_Page_In);
        }
    }
    private static void MoreSlugcats_CollectionsMenu_ctor(On.MoreSlugcats.CollectionsMenu.orig_ctor orig, MoreSlugcats.CollectionsMenu self, ProcessManager manager)
    {
        orig(self, manager);
        self.pages[0].subObjects.Add(new SimpleButton(self, self.pages[0], "Pitch Black", SINGAL_NAME, new Vector2(325, 50), new Vector2(110, 31)));
    }
}