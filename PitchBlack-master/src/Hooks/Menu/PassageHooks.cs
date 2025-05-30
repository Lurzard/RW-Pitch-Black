using RWCustom;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Menu;

namespace PitchBlack;

public static class PassageHooks
{
	public static void Apply()
    {
        //On.WinState.CycleCompleted += BP_CycleCompleted; for now we'll omit this
        On.WinState.CreateAndAddTracker += BP_CreateAndAddTracker;
        On.WinState.PassageDisplayName += WinState_PassageDisplayName;

        On.FSprite.ctor_string_bool += FSprite_ctor_string_bool;
        On.FAtlasManager.GetElementWithName += FAtlasManager_GetElementWithName;
		
		On.Menu.MenuScene.BuildScene += BP_BuildScene;
        On.Menu.CustomEndGameScreen.GetDataFromSleepScreen += BP_GetDataFromSleepScreen;

        On.Menu.EndgameMeter.FloatMeter.GrafUpdate += FloatMeter_GrafUpdate;
        On.Menu.EndgameMeter.GrafUpdate += EndgameMeter_GrafUpdate;
    }

    private static void EndgameMeter_GrafUpdate(On.Menu.EndgameMeter.orig_GrafUpdate orig, EndgameMeter self, float timeStacker)
    {
        orig(self, timeStacker);
		//SPECIAL COLORS FOR THIS ONE
        if (self.tracker.ID == PBEnums.EndGameID.Hunted)
		{
            float num2 = Mathf.Lerp(self.lastShowAsFullFilled, self.showAsFullfilled, timeStacker);
            float num3 = Mathf.Lerp(self.lastAnimationLightUp, self.animationLightUp, timeStacker);
            Color color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.VeryDarkGrey), Menu.Menu.MenuRGB(self.fullfilledNow ? Menu.Menu.MenuColors.SaturatedGold : Menu.Menu.MenuColors.MediumGrey), Mathf.Max(Mathf.Pow(num2, 0.2f), num3));
            self.symbolSprite.color = color;
            self.circleSprite.color = color;
            self.glowSprite.color = RainWorld.GoldRGB;
            self.glowSprite.alpha = self.symbolSprite.alpha * 0.65f;
            self.label.color = RainWorld.GoldRGB;
        }
    }

    private static void FloatMeter_GrafUpdate(On.Menu.EndgameMeter.FloatMeter.orig_GrafUpdate orig, Menu.EndgameMeter.FloatMeter self, float timeStacker)
    {
		orig(self, timeStacker);
        //RERUN THESE
		if (self.owner.tracker.ID == PBEnums.EndGameID.Hunted)
		{
            float num = Mathf.Lerp(self.owner.lastMeterAnimation, self.owner.meterAnimation, timeStacker);
            float num2 = Mathf.Pow(Mathf.InverseLerp(0.5f, 1f, num), 3f);
			self.meterSprites[0].color = RainWorld.GoldRGB; // CurseColor(self, timeStacker, num2); //self.LossColor(timeStacker, num2);
            self.meterSprites[1].color = RainWorld.GoldRGB; // CurseColor(self, timeStacker, num2);
            self.tipSprite.color = RainWorld.GoldRGB;
            self.sideBarSprite.color = RainWorld.GoldRGB;
        }
        
    }

	//LIKE LOSSCOLOR OR GAINCOLOR, BUT VERY RED
    public static Color CurseColor(EndgameMeter.FloatMeter myMeter, float timeStacker, float colorCue)
    {
        return myMeter.AllColorsViaThis(Color.Lerp(myMeter.FilledColor(timeStacker), new Color(1f, 0f, 0f), (1f - myMeter.pulse) * 0.5f * colorCue), timeStacker);
    }

    private static string WinState_PassageDisplayName(On.WinState.orig_PassageDisplayName orig, WinState.EndgameID ID)
	{
		if (ID == PBEnums.EndGameID.Hunted)
			return "The Hunted";
		else
			return orig.Invoke(ID);
	}


	private static FAtlasElement FAtlasManager_GetElementWithName(On.FAtlasManager.orig_GetElementWithName orig, FAtlasManager self, string elementName)
    {
		if (elementName == "PursuedA")
			return orig.Invoke(self, "HunterA"); //HunterA //foodSymbol
		else if (elementName == "PursuedB")
			return orig.Invoke(self, "HunterB"); //HunterB
		else
			return orig.Invoke(self, elementName);
	}

    

    private static void FSprite_ctor_string_bool(On.FSprite.orig_ctor_string_bool orig, FSprite self, string elementName, bool quadType)
    {
		if (elementName == "PursuedA")
			orig.Invoke(self, "HunterA", quadType); //HunterA
		else if (elementName == "PursuedB")
			orig.Invoke(self, "HunterB", quadType); //HunterB
		else
			orig.Invoke(self, elementName, quadType);
	}



	public static void BP_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
	{
		if (self.sceneID == PBEnums.SceneID.Endgame_Hunted)
		{
			//WE DIDN'T BUILD ONE YET, BUT IF YOU WANT TO....
			/*
			//FIRST PART ALL OF THEM GET
			if (self is Menu.InteractiveMenuScene)
			{
				(self as Menu.InteractiveMenuScene).idleDepths = new List<float>();
			}
			Vector2 vector = new Vector2(0f, 0f);
			// vector..ctor(0f, 0f);

			//NOW THE CUSTOM PART
			self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Endgame - Pursued";
			if (self.flatMode)
			{
				self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Endgame - The Pursued - Flat", new Vector2(683f, 384f), false, true));
			}
			else
			{
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 6", new Vector2(71f, 49f), 2.2f, Menu.MenuDepthIllustration.MenuShader.Lighten));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 5", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Normal));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 4", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.Normal));
				//self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(0.5f);
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 3", new Vector2(71f, 49f), 1.7f, Menu.MenuDepthIllustration.MenuShader.LightEdges));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 2", new Vector2(71f, 49f), 1.5f, Menu.MenuDepthIllustration.MenuShader.Normal));
				self.AddIllustration(new Menu.MenuDepthIllustration(self.menu, self, self.sceneFolder, "Pursued - 1", new Vector2(171f, 49f), 1.3f, Menu.MenuDepthIllustration.MenuShader.Normal)); //LightEdges
				//(self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(2.2f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.7f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.5f);
				(self as Menu.InteractiveMenuScene).idleDepths.Add(1.3f);
			}
			self.AddIllustration(new Menu.MenuIllustration(self.menu, self, self.sceneFolder, "Pursued - Symbol", new Vector2(683f, 35f), true, false));
			Menu.MenuIllustration MenuIllustration4 = self.flatIllustrations[self.flatIllustrations.Count - 1];
			MenuIllustration4.pos.x = MenuIllustration4.pos.x - (0.01f + self.flatIllustrations[self.flatIllustrations.Count - 1].size.x / 2f);
			*/
		}
		else
			orig.Invoke(self);
	}


	private static void BP_GetDataFromSleepScreen(On.Menu.CustomEndGameScreen.orig_GetDataFromSleepScreen orig, CustomEndGameScreen self, WinState.EndgameID endGameID)
	{
		if (endGameID == PBEnums.EndGameID.Hunted)
		{
			//GOTTA REPLICATE THE MENU SCREEN
			MenuScene.SceneID sceneID = Menu.MenuScene.SceneID.Empty;
			sceneID = PBEnums.SceneID.Endgame_Hunted;
			self.scene = new InteractiveMenuScene(self, self.pages[0], sceneID);
			self.pages[0].subObjects.Add(self.scene);
			self.pages[0].Container.AddChild(self.blackSprite);
			if (self.scene.flatIllustrations.Count > 0)
			{
				self.scene.flatIllustrations[0].RemoveSprites();
				self.scene.flatIllustrations[0].Container.AddChild(self.scene.flatIllustrations[0].sprite);
				self.glyphIllustration = self.scene.flatIllustrations[0];
				self.glyphGlowSprite = new FSprite("Futile_White", true);
				self.glyphGlowSprite.shader = self.manager.rainWorld.Shaders["FlatLight"];
				self.pages[0].Container.AddChild(self.glyphGlowSprite);
				self.localBloomSprite = new FSprite("Futile_White", true);
				self.localBloomSprite.shader = self.manager.rainWorld.Shaders["LocalBloom"];
				self.pages[0].Container.AddChild(self.localBloomSprite);
			}
			self.titleLabel = new MenuLabel(self, self.pages[0], "The Hunted", new Vector2(583f, 5f), new Vector2(200f, 30f), false, null);
			self.pages[0].subObjects.Add(self.titleLabel);
			self.titleLabel.text = self.Translate(WinState.PassageDisplayName(endGameID));
		}
		else
			orig.Invoke(self, endGameID);
	}

	private static void BP_GenerateAchievementScores(On.Expedition.ChallengeTools.orig_GenerateAchievementScores orig)
	{
		orig.Invoke();
		Expedition.ChallengeTools.achievementScores.Add(PBEnums.EndGameID.Hunted, 50);
	}
	
	
	
	public static void BP_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
	{
		orig.Invoke(self, game);
		//ONLY FOR BACON
		if (game.session is StoryGameSession session && (session.saveStateNumber == PBEnums.SlugcatStatsName.Beacon))
		{
            WinState.IntegerTracker integerTracker4 = self.GetTracker(PBEnums.EndGameID.Hunted, true) as WinState.IntegerTracker;
            if (integerTracker4 != null)
            {
                integerTracker4.SetProgress(100);
                //integerTracker4.lastShownProgress = 99; //PRETEND IT'S NEVER FULL... MAYBE? this makes us watch it every time though...
            }
        }
    }


    public static WinState.EndgameTracker BP_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
	{
		WinState.EndgameTracker endgameTracker = null;
		
		if (ID == PBEnums.EndGameID.Hunted)
		{
			endgameTracker = new WinState.IntegerTracker(ID, 99, 0, 0, 100); //default, min, showFrom, max
			Debug.Log("PURSUED TRACKER CREATED!");
		}
        else
            return orig.Invoke(ID, endgameTrackers); //JUST RUN THE ORIGINAL AND NOTHING ELSE BELOW IT



        //AND THEN RUN THE ORIGINAL STUFF THAT WOULD OTHERWISE BE SKIPPED
        if (endgameTracker != null && endgameTrackers != null)
		{
			bool flag = false;
			for (int j = 0; j < endgameTrackers.Count; j++)
			{
				if (endgameTrackers[j].ID == ID)
				{
					flag = true;
					endgameTrackers[j] = endgameTracker;
					break;
				}
			}
			if (!flag)
			{
				endgameTrackers.Add(endgameTracker);
			}
		}
		return endgameTracker;
	}
}