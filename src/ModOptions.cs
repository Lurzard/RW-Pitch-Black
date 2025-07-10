using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class ModOptions : OptionInterface
{
    public static readonly ModOptions Instance = new();
	public static Configurable<int> maxFlashStore;
	public static Configurable<bool> shockStun;
	public static Configurable<bool> elecImmune;
	public static Configurable<bool> chargeSpears;
	public static Configurable<bool> pursuer;
    public static Configurable<int> pursuerAgro;
    public static Configurable<bool> hazHat;
    public static Configurable<bool> universalPursuer;
    public static Configurable<bool> scavStealing;

    public ModOptions()
    {
		//maxFlashStore = config.Bind<int>("maxFlashStore", 4, new ConfigAcceptableRange<int>(0, 10));
		//shockStun = config.Bind<bool>("shockStun", true);
		elecImmune = config.Bind("elecImmune", false);
		chargeSpears = config.Bind("chargeSpears", false);
		pursuer = config.Bind("pursuer", true);
        pursuerAgro = config.Bind("pursuerAgro", 2, new ConfigAcceptableRange<int>(0, 10));
        hazHat = config.Bind("hazHat", false);
        universalPursuer = config.Bind("universalPursuer", false);
        scavStealing = config.Bind("scavStealing", false);
    }
    public override void Initialize()
    {
        OpTab page1 = new OpTab(this, "Options");
        OpTab page2 = new OpTab(this, "Dev Options");
        Tabs =
        [
	        page1,
	        page2
        ];

		const int sliderBarLength = 135;
        const int rightSidePos = 360;
        const int leftSidePos = 60;
        #nullable enable
        UIelement[]? page1Elements =
        [
	        new OpLabel(200, 575, Translate("Pitch Black Options"), true) {alignment=FLabelAlignment.Center},

            // Make the options on the right side
            //new OpSlider(maxFlashStore, new Vector2(rightSidePos, 520), sliderBarLength) {description=Translate("Beacon's Max Stored Flashbangs")},
            //new OpLabel(rightSidePos, 500, Translate("Flashbang storage amount")),

            new OpSlider(pursuerAgro, new Vector2(rightSidePos, 440), sliderBarLength) {description = Translate("Determines how long it takes for the pursuer to spawn")},
            new OpLabel(rightSidePos, 420, Translate("Pursuer Aggro")),

            new OpCheckBox(hazHat, new Vector2(rightSidePos, 360)) {description=Translate("PB slugcats wear a hat to protect their eyes in other campaigns")},
            new OpLabel(rightSidePos+30, 363, Translate("Wear Hats")),

            // Make the options on the left side
            new OpCheckBox(pursuer, new Vector2(leftSidePos, 520)) {description=Translate("Something is pursuing you...")},
            new OpLabel(leftSidePos+30, 523, Translate("Beacon's Pursuer Spawns")),

            new OpCheckBox(scavStealing, new Vector2(leftSidePos, 440)) {description = Translate("Scavengers can steal Beacon's flares (causes graphical issues with storage)")},
            new OpLabel(leftSidePos+30, 443, Translate("Scavenger Griefing")),

            new OpCheckBox(elecImmune, new Vector2(leftSidePos, 360)) {description = Translate("Photomaniac gains resistance to electricity")},
            new OpLabel(leftSidePos+30, 363, Translate("Photomaniac's Electricity Resistance")),

            // Put the universal pursuer option in the middle
            new OpCheckBox(universalPursuer, new Vector2(230f, 280f)) {description = Translate("The pursuer appears in all campaigns for all slugcats")},
            new OpLabel(260f, 283f, Translate("Universal Pursuer")),

            // Make the text at the bottom
	        // NOTE: Increment YPos by 20
            new OpLabel(25, 225, "Beacon:"),
            new OpLabel(25, 205, Translate("Flashbang creation: Costs 1 food pip per rock + SHIFT / Grab (Automatically added to storage).")),
            new OpLabel(25, 185, Translate("Add flashbang to storage: Have a flashbang in hand + hold SHIFT / Grab.")),
            new OpLabel(25, 165, Translate("Remove flashbang from storage: Have a stored flashbang + hold SHIFT / Grab.")),
            new OpLabel(25, 145, Translate("Quick-throw flashbang: Have a stored flashbang + X / Throw on an empty hand.")),
            new OpLabel(25, 100, "Photomaniac:"),
            new OpLabel(25, 80, Translate("Electric Spear creation: Costs 1 food pip per spear + SHIFT / Grab.")),
            new OpLabel(25, 60, Translate("Electric shockwave ability: SHIFT / Grab + Z / Jump."))
        ];
        page1.AddItems(page1Elements);

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