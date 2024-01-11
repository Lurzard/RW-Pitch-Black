using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class PBOptions : OptionInterface
{
    public static readonly PBOptions Instance = new();
	public static Configurable<int> maxFlashStore;
	public static Configurable<bool> shockStun;
	public static Configurable<bool> elecImmune;
	public static Configurable<bool> chargeSpears;
	public static Configurable<bool> pursuer;
    public static Configurable<int> pursuerAgro;
    public static Configurable<bool> hazHat;

    public PBOptions()
    {
		maxFlashStore = config.Bind<int>("maxFlashStore", 4, new ConfigAcceptableRange<int>(0, 10));
		shockStun = config.Bind<bool>("shockStun", true);
		elecImmune = config.Bind<bool>("elecImmune", false);
		chargeSpears = config.Bind<bool>("chargeSpears", false);
		pursuer = config.Bind<bool>("pursuer", true);
        pursuerAgro = config.Bind<int>("pursuerAgro", 2, new ConfigAcceptableRange<int>(0, 10));
        hazHat = config.Bind<bool>("hazHat", false);
    }
    public override void Initialize()
    {
        OpTab opTab = new OpTab(this, "Options");
        Tabs = new[]
        {
            opTab
        };

		int barLngt = 135;
        #nullable enable
        UIelement[]? uIelements = new UIelement[]
        {
            new OpLabel(200, 575, Translate("Pitch Black Options"), true) {alignment=FLabelAlignment.Center},

            new OpSlider(maxFlashStore, new Vector2(300, 455), barLngt) {description=Translate("Maximum number of flashbangs stored")},
            new OpLabel(300, 435, Translate("Flashbang storage amount")),

            new OpSlider(pursuerAgro, new Vector2(300, 395), barLngt) {description = Translate("How long it takes for the pursuer to track you down")},
            new OpLabel(300, 375, Translate("Pursuer Aggro"), false),

            new OpCheckBox(shockStun, new Vector2(20, 460)) {description = Translate("Photomaniac becomes stunned after using their shock")},
            new OpLabel(50, 463, Translate("Photomaniac's Shock Stun")),

            new OpCheckBox(elecImmune, new Vector2(20, 400)) {description = Translate("Photomaniac gains resistance to electricity")},
            new OpLabel(50, 403, Translate("Photomaniac's electricity resistance")),

            new OpCheckBox(hazHat, new Vector2(20, 340)) {description=Translate("If the PB slugcats wear a hat to protect their eyes in other campaigns")},
            new OpLabel(50, 340, Translate("Wear Hats"), false),

            new OpLabel(25, 225, "Beacon:"),
            new OpLabel(25, 205, Translate("Flashbang creation: Costs 1 food pip per rock + SHIFT / Grab (Automatically added to storage).")),
            new OpLabel(25, 185, Translate("Add flashbang to storage: Have a flashbang in hand + hold SHIFT / Grab.")),
            new OpLabel(25, 165, Translate("Remove flashbang from storage: Have a stored flashbang + hold SHIFT / Grab.")),
            new OpLabel(25, 145, Translate("Quick-throw flashbang: Have a stored flashbang + X / Throw on an empty hand.")),
            new OpLabel(25, 100, "Photomaniac:"),
            new OpLabel(25, 80, Translate("Electric Spear creation: Costs 1 food pip per spear + SHIFT / Grab.")),
            new OpLabel(25, 60, Translate("Electric shockwave ability: SHIFT / Grab + Z / Jump."))
        };
        opTab.AddItems(uIelements);
		
		// // DISABLING UNTIL FIXED
		// dsc = Translate("Something is pursuing you...");
		// Tabs[0].AddItems(new UIelement[]
		// {
		// 	mpBox1 = new OpCheckBox(PBOptions.pursuer, new Vector2(margin, lineCount))
		// 	{description = dsc},
		// 	new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, Translate("Beacon's Pursuer Spawns"))
		// 	{description = dsc},
        // });

        //Not exactly sure what to do with this so I will leave it here for now
        /*
		lineCount -= 60;
		dsc = Translate("Photomaniac will charge uncharged electric spears");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox7 = new OpCheckBox(PBOptions.chargeSpears, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox7.pos.x + 30, mpBox7.pos.y+3, Translate("Charge Spears"))
			{description = dsc}
		});
		*/
    }
}