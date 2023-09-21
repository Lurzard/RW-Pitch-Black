using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace PitchBlack;

public class PBOptions : OptionInterface
{
    public static readonly PBOptions Instance = new();
    private readonly ManualLogSource Logger;

    public PBOptions()
    {
        //Logger = loggerSource;
		maxFlashStore = this.config.Bind<int>("maxFlashStore", 4, new ConfigAcceptableRange<int>(0, 10));
		shockStun = this.config.Bind<bool>("shockStun", true);
		elecImmune = this.config.Bind<bool>("elecImmune", false);
		chargeSpears = this.config.Bind<bool>("chargeSpears", false);
		pursuer = this.config.Bind<bool>("pursuer", true);
    }

    
	public static Configurable<int> maxFlashStore;
	public static Configurable<bool> shockStun;
	public static Configurable<bool> elecImmune;
	public static Configurable<bool> chargeSpears;
	public static Configurable<bool> pursuer;

    private UIelement[] UIArrPlayerOptions;

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
		int barLngt = 35 * 3;
		float sldPad = 15;
		Tabs[0].AddItems(new UIelement[]
		{
            pDistOp = new OpSlider(PBOptions.maxFlashStore, new Vector2(margin + 250, lineCount-5), barLngt)
			{description = dsc},
            lblOp1 = new OpLabel(pDistOp.pos.x + ((barLngt * 1) / 6f), pDistOp.pos.y - 20, Translate("Max Flashbangs"), bigText: false)
			{alignment = FLabelAlignment.Center}
		});
		
		
		dsc = Translate("Something is pursuing you...");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox1 = new OpCheckBox(PBOptions.pursuer, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox1.pos.x + 30, mpBox1.pos.y+3, Translate("Pursuer"))
			{description = dsc},
        });
		
		
		lineCount -= 60;
        OpCheckBox mpBox4;
        dsc = Translate("Photomaniac becomes stunned after using their shock");
        Tabs[0].AddItems(new UIelement[]
        {
            mpBox4 = new OpCheckBox(PBOptions.shockStun, new Vector2(margin, lineCount))
            {description = dsc},
            new OpLabel(mpBox4.pos.x + 30, mpBox4.pos.y+3, Translate("Shock Stun"))
            {description = dsc}
        });
		
		
		lineCount -= 60;
		dsc = Translate("Photomaniac gains resistance to electricity");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox5 = new OpCheckBox(PBOptions.elecImmune, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox5.pos.x + 30, mpBox5.pos.y+3, Translate("Electricity Resistance"))
			{description = dsc}
		});
        
		
		lineCount -= 60;
		dsc = Translate("Photomaniac will charge uncharged electric spears");
		Tabs[0].AddItems(new UIelement[]
		{
			mpBox7 = new OpCheckBox(PBOptions.chargeSpears, new Vector2(margin, lineCount))
			{description = dsc},
			new OpLabel(mpBox7.pos.x + 30, mpBox7.pos.y+3, Translate("Charge Spears"))
			{description = dsc}
		});
		
		
		
        int descLine = 225;
        Tabs[0].AddItems(new OpLabel(25f, descLine + 25f, "--- How It Works: ---"));
        // Tabs[0].AddItems(new OpLabel(25f, descLine, "Press up against stuck creatures to push them. Grab them to pull"));
        // descLine -= 20;
        Tabs[0].AddItems(new OpLabel(25f, descLine, Translate("Entering a pipe will create a warp beacon for other players")));
        descLine -= 20;
    }

    public override void Update()
    {
        
    }

}