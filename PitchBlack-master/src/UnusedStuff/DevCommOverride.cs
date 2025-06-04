#if false
using IL.MoreSlugcats;
using Color = UnityEngine.Color;
using StringSplitOptions = System.StringSplitOptions;
using MonoMod.Cil;
using File = System.IO.File;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace PitchBlack;
// 
// 
// THE DEFAULT PATH THE GAME READS FILES FROM YOUR MOD WILL BE "YourMod/text/text_eng"
// The base game files also include separate folders for all supported languages, such as "text_spa" and "text_ita", 
//     I assume you will have to do the translations and put them in these folders manually for translations to work.
// File names in these folders are all under the same name, following the format "regionAcronym_roomName-slugcatid", and where your commentary goes.
//     An example would be "gw_b05-white.txt"
//     I also do not know how sigificant the id is, but I believe if you put the id (SlugcatStats.Name.value) as anything other than white,
//     the token will only show if the slugcat being played has the matching id.
// The only other things to note are that each time you press "Enter" in notpad is an entirely new line, so you must put your prefix again,
//     And doing <LINE> will make a linebreak so that all the text in a single message can fit in on the screen, essentially wordwraping at that location, if that makes sense.
// 
// 
public class DevCommOverride
{
    public static void Apply() {
        ChatlogData.getDevComm += MoreSlugcats_ChatlogData_getDevComm;
        On.Conversation.InitalizePrefixColor += Conversation_InitalizePrefixColor;
    }
    // The purpose of this IL hook is to have the getDevComm return a non-decrypted key, so that you can write the plain text 
    // just in your file and not think about how to use the encryption.
    public static void MoreSlugcats_ChatlogData_getDevComm(ILContext il) {
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBlt(out var _), i => i.MatchLdloc(2))) {
            return;
        }
        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate((string[] array2, string path) => {
            if (path.Contains("3032862920") || path.Contains("PitchBlack")) {
                string rawText = File.ReadAllText(path);
                return rawText.Split(new string[]{ "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            return array2;
        });
    }
    public static void Conversation_InitalizePrefixColor(On.Conversation.orig_InitalizePrefixColor orig) {
        orig();
        // Conversation.PrefixColors is a static dictionary.
        // Add a key here. Each new line that begins with this value (i.e "Moon: This mod is awesome!") will have the color specified by the key here.
        Conversation.PrefixColors.Add("Moon", new Color
            (71f/255f, 200f/255f, 148f/255f)); //#47c894

        Conversation.PrefixColors.Add("Lurzard", new Color
            (96f/255f, 71f/255f, 255f/255f)); //#4d47ff

        Conversation.PrefixColors.Add("Millisec", new Color
            (255f/255f, 89f/255f, 71f/255f)); //#ff5947

        Conversation.PrefixColors.Add("Niko", new Color
            (133f/255f, 77f/255f, 255f/255f)); //#854dff

        Conversation.PrefixColors.Add("Serpanoy", new Color
            (20f/169f, 122f/255f, 61f/255f)); //#147a3d

        Conversation.PrefixColors.Add("Opey", new Color
            (166f/255f, 20f/255f, 188f/255f)); //#a614bc

        Conversation.PrefixColors.Add("Spinch", new Color
            (247f/255f, 42f/255f, 42f/255f)); //#f72a2a

        //Deathpits

        //Ludocrypt

        //Willow

        //Detrax
    }
}
#endif