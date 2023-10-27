#if false
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Colour = UnityEngine.Color;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PitchBlack;

public class NightDay
{
    //unfinished code
    public static void Apply()
    {
        //On.RoofTopView.ctor += DarkRTV;
        IL.RoofTopView.ctor += RoofTopView_ctor;
    }

    private static void RoofTopView_ctor(ILContext il)
    {
        ILCursor c = new(il);
        c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdcR4(0.16078432f),
            x => x.MatchLdcR4(0.23137255f),
            x => x.MatchLdcR4(0.31764707f),
            x => x.MatchNewobj<UnityEngine.Color>(),
            x => x.MatchStfld<RoofTopView>("atmosphereColor")
            );
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((RoofTopView self) =>
        {
            if (ModManager.MSC && self.room?.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber))
            {
                self.atmosphereColor = new Colour(0.04882353f, 0.0527451f, 0.06843138f);
            }
        });


        //c.Index = 0;
        //c.Next.Operand = 0.04882353f;
        //c.Next.Operand = 0.0527451f;
        //c.Next.Operand = 0.06843138f;
    }

    //public static Color PB_atmosphereColor = new Color(0.04882353f, 0.0527451f, 0.06843138f);

    //private static void DarkRTV(On.RoofTopView.orig_ctor orig, RoofTopView self, Room room, RoomSettings.RoomEffect effect)
    //{
    //    if (ModManager.MSC && self.room?.game.session is StoryGameSession story && MiscUtils.IsBeaconOrPhoto(story.saveStateNumber))
    //    {
    //        self.effect = effect;
    //        self.sceneOrigo = self.RoomToWorldPos(room.abstractRoom.size.ToVector2() * 10f);
    //        room.AddObject(new RoofTopView.DustpuffSpawner());

    //        self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_Sky", new Vector2(683f, 384f));
    //        self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_Sky", new Vector2(683f, 384f));
    //        self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "Rf_Sky", new Vector2(683f, 384f));

    //        string text = "";
    //        bool flag = false;
    //        if ((ModManager.MSC && room.world.region != null && room.world.region.name == "DM") || self.room.abstractRoom.name.StartsWith("DM_"))
    //        {
    //            text = "_DM";
    //            flag = true;
    //        }

    //        #region isLC
    //        self.isLC = (ModManager.MSC && ((room.world.region != null && room.world.region.name == "LC") || self.room.abstractRoom.name.StartsWith("LC_")));
    //        if (self.isLC && (self.room.abstractRoom.name == "LC_entrancezone" || self.room.abstractRoom.name == "LC_shelter_above"))
    //        {
    //            self.isLC = false;
    //        }

    //        if (self.isLC)
    //        {
    //            self.daySky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_Sky", new Vector2(683f, 384f));
    //            self.duskSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_DuskSky", new Vector2(683f, 384f));
    //            self.nightSky = new BackgroundScene.Simple2DBackgroundIllustration(self, "AtC_NightSky", new Vector2(683f, 384f));
    //            self.AddElement(self.nightSky);
    //            self.AddElement(self.duskSky);
    //            self.AddElement(self.daySky);
    //            self.floorLevel = self.room.world.RoomToWorldPos(new Vector2(0f, 0f), self.room.abstractRoom.index).y - 30992.8f;
    //            self.floorLevel *= 22f;
    //            self.floorLevel = -self.floorLevel;
    //            float num3 = self.room.world.RoomToWorldPos(new Vector2(0f, 0f), self.room.abstractRoom.index).x - 11877f;
    //            num3 *= 0.01f;
    //            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", PB_atmosphereColor);
    //            Shader.SetGlobalVector("_MultiplyColor", Color.white);
    //            Shader.SetGlobalVector("_SceneOrigoPosition", self.sceneOrigo);
    //            self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 200f - num3).x, self.floorLevel * 0.2f - 170000f), 420.5f, 2f));
    //            self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, self.floorLevel * 0.25f - 116000f), 340f, 2f));
    //            self.AddElement(new RoofTopView.Building(self, "city3", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 70f - num3 * 0.5f).x, self.floorLevel * 0.3f - 85000f), 260f, 2f));
    //            self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 40f - num3 * 0.5f).x, self.floorLevel * 0.35f - 42000f), 180f, 2f));
    //            self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 90f - num3 * 0.2f).x, self.floorLevel * 0.4f + 5000f), 100f, 2f));
    //            self.AddElement(new RoofTopView.Floor(self, "floor", new Vector2(0f, self.floorLevel * 0.2f - 90000f), 400.5f, 500.5f));
    //            return;
    //        }
    //        #endregion

    //        self.AddElement(self.nightSky);
    //            self.AddElement(self.duskSky);
    //            self.AddElement(self.daySky);
    //            Shader.SetGlobalVector("_MultiplyColor", Color.white);
    //            self.AddElement(new RoofTopView.Floor(self, "floor", new Vector2(0f, self.floorLevel), 1f, 12f));
    //            Shader.SetGlobalVector("_AboveCloudsAtmosphereColor", PB_atmosphereColor);
    //            Shader.SetGlobalVector("_SceneOrigoPosition", self.sceneOrigo);
    //            for (int i = 0; i < 16; i++)
    //            {
    //                float f = (float)i / 15f;
    //                self.AddElement(new RoofTopView.Rubble(self, "Rf_Rubble", new Vector2(0f, self.floorLevel), Mathf.Lerp(1.5f, 8f, Mathf.Pow(f, 1.5f)), i));
    //            }
    //            self.AddElement(new RoofTopView.DistantBuilding(self, "Rf_HoleFix", new Vector2(-2676f, 9f), 1f, 0f));
    //            if (!ModManager.MSC || text == "")
    //            {
    //                self.AddElement(new RoofTopView.Building(self, "city2", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(1780f, 0f), 11.5f).x, self.floorLevel), 11.5f, 3f));
    //                self.AddElement(new RoofTopView.Building(self, "city1", new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(880f, 0f), 10.5f).x, self.floorLevel), 10.5f, 3f));
    //            }
    //            self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityA" + text, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(300f + (flag ? -300f : 0f), 0f), 8.5f).x, self.floorLevel - 25.5f), 8.5f, 0f));
    //            self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityB" + text, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(515f + (flag ? -300f : 0f), 0f), 6.5f).x, self.floorLevel - 13f), 6.5f, 0f));
    //            self.AddElement(new RoofTopView.DistantBuilding(self, "RF_CityC" + text, new Vector2(self.PosFromDrawPosAtNeutralCamPos(new Vector2(400f + (flag ? -300f : 0f), 0f), 5f).x, self.floorLevel - 8.5f), 5f, 0f));
    //            self.LoadGraphic("smoke1", false, false);
    //            self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 560f), 7f, 0, 2.5f, 0.1f, 0.8f, false));
    //            self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel), 4.2f, 0, 0.2f, 0.1f, 0f, true));
    //            self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 28f), 2f, 0, 0.5f, 0.1f, 0f, true));
    //            self.AddElement(new RoofTopView.Smoke(self, new Vector2(0f, self.floorLevel + 14f), 1.2f, 0, 0.75f, 0.1f, 0f, true));
    //    }

    //    else
    //    {
    //        orig(self, room, effect);
    //    }
    //} //unused


}
#endif