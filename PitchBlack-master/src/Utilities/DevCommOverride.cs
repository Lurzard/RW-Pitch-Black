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
        IL.MoreSlugcats.ChatlogData.getDevComm += MoreSlugcats_ChatlogData_getDevComm;
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
            if (path.Contains("PitchBlack")) {
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
        Conversation.PrefixColors.Add("Moon", new Color(71f/255f, 200f/255f, 148f/255f));
    }
}