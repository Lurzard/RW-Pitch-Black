using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class PBOptions : OptionInterface
{
    public static readonly PBOptions Instance = new();

    public PBOptions()
    {
		maxFlashStore = this.config.Bind<int>("maxFlashStore", 4, new ConfigAcceptableRange<int>(0, 10));
		shockStun = this.config.Bind<bool>("shockStun", true);
		elecImmune = this.config.Bind<bool>("elecImmune", false);
		chargeSpears = this.config.Bind<bool>("chargeSpears", false);
		pursuer = this.config.Bind<bool>("pursuer", true);
        pursuerAgro2 = this.config.Bind<int>("pursuerAgro2", 6, new ConfigAcceptableRange<int>(0, 6));

        debugMsg = this.config.Bind<bool>("debugMsg", false);
        universalPursuer = this.config.Bind<bool>("universalPursuer", false);
    }

    
	public static Configurable<int> maxFlashStore;
	public static Configurable<bool> shockStun;
	public static Configurable<bool> elecImmune;
	public static Configurable<bool> chargeSpears;
	public static Configurable<bool> pursuer;
    public static Configurable<int> pursuerAgro2;

    public static Configurable<bool> debugMsg;
    public static Configurable<bool> universalPursuer;

    public OpSlider pDistOp;
	public OpCheckBox mpBox1;
    public OpCheckBox mpBox2;
	public OpCheckBox mpBox3;
	public OpCheckBox mpBox5;
    public OpCheckBox mpBox6;
    public OpCheckBox mpBox7;
    public OpLabel lblOp1;


    public override void Initialize()
    {
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };
		
		float lineCount = 580;
		int margin = 20;
		string dsc = "";
		
		Tabs[0].AddItems(new UIelement[]
		{
            new OpLabel(200, 575, Translate("Pitch Black Options"), bigText: true)
            {alignment = FLabelAlignment.Center},
        });
		
		
		lineCount -= 60;
		dsc = Translate("Maximum number of flashbangs stored");
		int barLngt = 45 * 3;
		Tabs[0].AddItems(new UIelement[]
		{
            pDistOp = new OpSlider(PBOptions.maxFlashStore, new Vector2(margin + 250, lineCount+5), barLngt)
			{description = dsc},
            lblOp1 = new OpLabel(pDistOp.pos.x, pDistOp.pos.y - 20, Translate("Beacon's Max Stored Flashbangs"), bigText: false)
			//{alignment = FLabelAlignment.Center}
		});
		
		// DISABLING UNTIL FIXED
		dsc = Translate("Something is pursuing you...");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox1 = new OpCheckBox(PBOptions.pursuer, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, Translate("Beacon's Pursuer Spawns"))
			{description = dsc},
        });
		
		
		lineCount -= 80;
		
        
        dsc = Translate("How long it takes for the pursuer to track you down");
        barLngt = 45 * 3;
        Tabs[0].AddItems(new UIelement[]
        {
            pDistOp = new OpSlider(PBOptions.pursuerAgro2, new Vector2(margin + 250, lineCount+5), barLngt)
            {description = dsc},
            lblOp1 = new OpLabel(pDistOp.pos.x, pDistOp.pos.y - 20, Translate("Pursuer Aggro"), bigText: false)
		});

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
		

        lineCount -= 60;
        OpCheckBox mpBox8;
        dsc = Translate("Shows debug messages on screen");
        Tabs[0].AddItems(new UIelement[]
        {
            mpBox8 = new OpCheckBox(PBOptions.debugMsg, new Vector2(margin, lineCount))
            {description = dsc},
            new OpLabel(mpBox8.pos.x + 30, mpBox8.pos.y+3, Translate("Debug Messages"))
            {description = dsc}
        });
        */

        OpCheckBox mpBox9;
        dsc = Translate("The Pursuer appears in all campaigns for all slugcats");
        Tabs[0].AddItems(new UIelement[]
        {
            mpBox9 = new OpCheckBox(PBOptions.universalPursuer, new Vector2(margin, lineCount))
            {description = dsc},
            new OpLabel(mpBox9.pos.x + 30, mpBox9.pos.y+3, Translate("Universal Pursuer"))
            {description = dsc}
        });



        lineCount -= 80;
        OpCheckBox mpBox4;
        dsc = Translate("Photomaniac becomes stunned after using their shock");
        Tabs[0].AddItems(new UIelement[]
        {
            mpBox4 = new OpCheckBox(PBOptions.shockStun, new Vector2(margin, lineCount))
            {description = dsc},
            new OpLabel(mpBox4.pos.x + 30, mpBox4.pos.y+3, Translate("Photomaniac's Shock Stun"))
            {description = dsc}
        });

        dsc = Translate("Photomaniac gains resistance to electricity");
        Tabs[0].AddItems(new UIelement[]
        {
            mpBox5 = new OpCheckBox(PBOptions.elecImmune, new Vector2(margin + 250, lineCount))
            {description = dsc},
            new OpLabel(mpBox5.pos.x + 30, mpBox5.pos.y+3, Translate("Photomaniac's Electricity Resistence"))
            {description = dsc}
        });


        int descLine = 200;
		Tabs[0].AddItems(new OpLabel(25f, descLine, "Beacon:"));
		descLine -= 20;
		Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Flashbang creation: Costs 1 food pip per rock + hold GRAB (Automatically added to storage).")));
        descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Add flashbang to storage: Have a flashbang in hand + hold GRAB.")));
        descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Remove flashbang from storage: Have a stored flashbang + hold GRAB.")));
        descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Quick-throw flashbang: Have a stored flashbang + THROW on an empty hand.")));

        descLine -= 45;
        Tabs[0].AddItems(new OpLabel(25f, descLine, "Photomaniac:"));
        descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Electric Spear creation: Costs 1 food pip per spear + hold GRAB.")));
        descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Electric shockwave ability: GRAB + JUMP.")));

    }

    public override void Update()
    {
        
    }

}