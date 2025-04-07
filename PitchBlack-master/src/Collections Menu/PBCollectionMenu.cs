using System.Collections.Generic;
using Menu;
using MoreSlugcats;
using UnityEngine;
using DataPearlType = DataPearl.AbstractDataPearl.DataPearlType;
using static PitchBlack.OverseerEx;
using static MoreSlugcats.ChatlogData;
using System.Linq;
using System.IO;
using RWCustom;

namespace PitchBlack;

public class PBCollectionMenu : Menu.Menu
{
    const int FADE_TIMER_MAX = 20;
    internal static MenuScene.SceneID PBCollectionScene = new MenuScene.SceneID(nameof(PBCollectionScene), true);
    public static Dictionary<ChatlogID, string> chatLogIDToButtonName = new Dictionary<ChatlogID, string>{
        { PB_CC, "CC" },
        { PB_DS, "DS" },
        { PB_GW, "GW" },
        { PB_HI, "HI" },
        { PB_LF_bottom, "LF1" },
        { PB_LF_west, "LF2" },
        { PB_SB_filtration, "SB" },
        { PB_SH, "SH" },
        { PB_SI_top, "SI1" },
        { PB_SI_west, "SI2" },
        { PB_SL_bridge, "SL1" },
        { PB_SL_chimney, "SL2" },
        { PB_SL_moon, "SL3" },
        { PB_SU, "SU1" },
        { PB_SU_filt, "SU2" },
        { PB_UW, "UW" },
        { PB_Techy, MiscUtils.GenerateRandomString(9, 16)}
    };
    private CollectionDialogBox displayRelayInfoBox;
    private int fadeTextToBlack = 0;
    private string swapText;
    float ScreenWidth => manager.rainWorld.options.ScreenSize.x;
    float ScreenHeight => manager.rainWorld.options.ScreenSize.y;
    Vector2 ScreenCenter => manager.rainWorld.options.ScreenSize/2f;
    public PBCollectionMenu(ProcessManager processManager) : base(processManager, MenuHooks.PitchBlackCollection) {
        mySoundLoopID = SoundID.MENU_Main_Menu_LOOP;
        pages.Add(new Page(this, null, "aaaaaa", 0));
        const float ButtonStartHeight = 94.5f;
        const int ButtonStartWidth = 100;
        const float ButtonHeight = 30;
        const float ButtonWidth = 165;
        const int ButtonsPerColumn = 12;
        for (int i = 0; i < chatLogIDToButtonName.Count; i++) {
            ChatlogID chatLogID = chatLogIDToButtonName.Keys.ElementAt(i);
            string buttonName = chatLogIDToButtonName[chatLogID];
            if (Plugin.collectionSaveData.TryGetValue(chatLogID.value, out bool val) && val) {
                pages[0].subObjects.Add(new SimpleButton(this, pages[0], buttonName, buttonName, new Vector2((i/ButtonsPerColumn)*(ButtonWidth+30) + ButtonStartWidth, (i%ButtonsPerColumn)*(ButtonHeight+20) + ButtonStartHeight), new Vector2(ButtonWidth, ButtonHeight)));
            }
            else if (!Plugin.collectionSaveData.TryGetValue(chatLogID.value, out var _)) {
                Debug.LogError("Key not found in loading PB collection menu! " + i + " " + chatLogID);
            }
            else {
                pages[0].subObjects.Add(new SimpleButton(this, pages[0], "???", "???", new Vector2((i/ButtonsPerColumn)*(ButtonWidth+30) + ButtonStartWidth, (i%ButtonsPerColumn)*(ButtonHeight+20) + ButtonStartHeight), new Vector2(ButtonWidth, ButtonHeight)));
            }
            // Applying the shader makes the sprites a bit too much to look at easily. Disabled it, but leaving the code here.
            // foreach(FSprite sprite in (pages[0].subObjects.Last(x => x is SimpleButton) as SimpleButton).roundedRect.sprites) {
            //     sprite.shader = manager.rainWorld.Shaders["Hologram"];
            // }
        }

        scene = new InteractiveMenuScene(this, pages[0], PBCollectionScene);
        pages[0].subObjects.Add(scene);

        // Back and Exit buttons
        pages[0].subObjects.Add(new SimpleButton(this, pages[0], "Exit", "EXIT", new Vector2(0.8f*ScreenWidth, 30), new Vector2(110, 30)));
        pages[0].subObjects.Add(new SimpleButton(this, pages[0], "Back", "BACK", new Vector2(0.7f*ScreenWidth, 30), new Vector2(110, 30)));

        // Text box for pearl dialog
        displayRelayInfoBox = new CollectionDialogBox(this, pages[0], Vector2.zero, new Vector2(350, 500));
        displayRelayInfoBox.descriptionLabel.label.MoveBehindOtherNode(scene.depthIllustrations.First(x => x.fileName == "hologrambkg").sprite);
        pages[0].subObjects.Add(displayRelayInfoBox);

        #region Bars
        MenuContainer menuContainer = new MenuContainer(this, pages[0], Vector2.zero);
        pages[0].subObjects.Add(menuContainer);

        FSprite verticalBar = new FSprite("Futile_White");
        verticalBar.SetPosition(new Vector2(ButtonStartWidth-10, ScreenCenter.y));
        verticalBar.scaleY = 40;
        verticalBar.scaleX = 0.1f;
        verticalBar.shader = this.manager.rainWorld.Shaders["Hologram"];
        menuContainer.Container.AddChild(verticalBar);

        FSprite horizontalBar1 = new FSprite("Futile_White");
        horizontalBar1.SetAnchor(0.05f, 0.5f);
        horizontalBar1.SetPosition(new Vector2(ButtonStartWidth-10, ScreenHeight*0.1f));
        horizontalBar1.scaleX = 30;
        horizontalBar1.scaleY = 0.1f;
        horizontalBar1.shader = this.manager.rainWorld.Shaders["Hologram"];
        menuContainer.Container.AddChild(horizontalBar1);

        FSprite horizontalBar2 = new FSprite("Futile_White");
        horizontalBar2.SetAnchor(0.05f, 0.5f);
        horizontalBar2.SetPosition(new Vector2(ButtonStartWidth-10, ScreenHeight*0.9f));
        horizontalBar2.scaleX = 30;
        horizontalBar2.scaleY = 0.1f;
        horizontalBar2.shader = this.manager.rainWorld.Shaders["Hologram"];
        menuContainer.Container.AddChild(horizontalBar2);
        #endregion
    }
    public override void Update()
    {
        base.Update();
        Shader.SetGlobalVector(RainWorld.ShadPropSpriteRect, new Vector4(0, 0, 1, 1));
        Shader.SetGlobalVector(RainWorld.ShadPropScreenSize, manager.rainWorld.screenSize);
        foreach (var obj in pages[0].subObjects) {
            if (obj is SimpleButton simpButton && simpButton.menuLabel.myText == chatLogIDToButtonName[PB_Techy]) {
                simpButton.menuLabel.label.text = MiscUtils.GenerateRandomString(20, 30);
            }
        }
        Color color = displayRelayInfoBox.descriptionLabel.label.color;
        color.a = Mathf.Lerp(0, 1, Mathf.Abs((float)fadeTextToBlack / FADE_TIMER_MAX));
        displayRelayInfoBox.descriptionLabel.label.color = color;
        if (fadeTextToBlack > -FADE_TIMER_MAX) {
            fadeTextToBlack--;
        }
        if (fadeTextToBlack == 0) {
            displayRelayInfoBox.text = swapText;
        }
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        displayRelayInfoBox.descriptionLabel.label.SetPosition((scene as InteractiveMenuScene).depthIllustrations.First(x => x.fileName == "hologrambkg").sprite.GetPosition() + new Vector2(342, 485));
    }
    public override void Singal(MenuObject sender, string message)
    {
        base.Singal(sender, message);
        if (message == "EXIT") {
            manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
            PlaySound(SoundID.MENU_Switch_Page_Out);
            manager.menuMic.PlayLoop(SoundID.MENU_Main_Menu_LOOP, 0, 1, 1, true);
        }
        if (message == "BACK") {
			manager.RequestMainProcessSwitch(MoreSlugcatsEnums.ProcessID.Collections);
			PlaySound(SoundID.MENU_Switch_Page_In);
        }
        if (fadeTextToBlack == -FADE_TIMER_MAX) {
            foreach (KeyValuePair<ChatlogID, string> keyValuePair in chatLogIDToButtonName) {
                if (keyValuePair.Value == message) {
                    string text = "";
                    foreach (string t in ChatlogData.getChatlog(keyValuePair.Key)) {
                        text += t.Replace("<LINE>", "\n") + "\n";
                    }
                    if (swapText != text) {
                        PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                        swapText = text;
                        fadeTextToBlack = FADE_TIMER_MAX;
                    }
                    break;
                }
            }
        }
    }
}