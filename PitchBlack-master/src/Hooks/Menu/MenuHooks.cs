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

        // Collections Menu
        IL.ProcessManager.PostSwitchMainProcess += IL_ProcessManager_PostSwitchMainProcess;
        On.MoreSlugcats.CollectionsMenu.ctor += MoreSlugcats_CollectionsMenu_ctor;
        On.MoreSlugcats.CollectionsMenu.Singal += MoreSlugcats_CollectionsMenu_Singal;

        // Slugcat Select Menu
        IL.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatSelectMenu_SlugcatPageContinue_ctor;
    }

    private static void SlugcatSelectMenu_SlugcatPageContinue_ctor(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Int32>("ToString")))
        {
            Plugin.logger.LogError($"Pitch Black: Error in {nameof(SlugcatSelectMenu_SlugcatPageContinue_ctor)}");
            return;
        }
        cursor.Emit(OpCodes.Ldarg, 4);
        cursor.EmitDelegate((string cycleNum, SlugcatStats.Name slugcatNumber) =>
        {
            if (MiscUtils.IsBeaconOrPhoto(slugcatNumber))
            {
                int startingRange = 0;
                try
                {
                    startingRange = Convert.ToInt32(cycleNum);
                }
                catch (Exception err)
                {
                    Debug.Log($"Pitch Black: cycle number was not, in fact, a number!\n{err}");
                    startingRange = cycleNum.Length;
                }
                return MiscUtils.GenerateRandomString(startingRange, startingRange + 10);
            }
            return cycleNum;
        });
    }

    private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
    {
        bool wasPBScene = false;
        if (self.sceneID == PBCollectionMenu.PBCollectionScene) {
            string regionToDisplay = File.ReadAllText(Plugin.regionMenuDisplaySavePath);
            self.sceneID = Region.GetRegionLandscapeScene(regionToDisplay);
            wasPBScene = true;
        }

        orig(self);

        if (wasPBScene) {
            self.sceneID = PBCollectionMenu.PBCollectionScene;
        }

        if (self.sceneID == PBCollectionMenu.PBCollectionScene) {
            for (int i = self.depthIllustrations.Count-1; i >= 0; i--) {
                self.depthIllustrations[i].sprite.MoveToBack();
                self.depthIllustrations[i].depth *= 3;
            }

            self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "collectionMenu";
            MenuDepthIllustration hologramIllustration = new MenuDepthIllustration(self.menu, self, self.sceneFolder, "hologrambkg", new Vector2(803.9481f, -17.7778f), 10f, MenuDepthIllustration.MenuShader.Normal);
            hologramIllustration.sprite.scaleY *= 1.7f;
            hologramIllustration.sprite.scaleX *= 1.15f;
            self.AddIllustration(hologramIllustration);

            MenuDepthIllustration overseer = new MenuDepthIllustration(self.menu, self, self.sceneFolder, "Illustration_sans_titre_1", new Vector2(565.4542f, -10.66667f), 10, MenuDepthIllustration.MenuShader.Normal);
            overseer.sprite.scale *= 0.75f;
            self.AddIllustration(overseer);

            MenuDepthIllustration lights = new MenuDepthIllustration(self.menu, self, self.sceneFolder, "Illustration_sans_titre_2", new Vector2(694.2281f, 172.9778f), 10, MenuDepthIllustration.MenuShader.Basic);
            lights.sprite.scale *= 0.75f;
            self.AddIllustration(lights);

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
            //Debug.LogError(il); il errors
            throw;
        }

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate((ProcessManager self, ProcessManager.ProcessID ID) =>
        {
            if (ID == PitchBlackCollection)
            {
                self.currentMainLoop = new PBCollectionMenu(self);
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