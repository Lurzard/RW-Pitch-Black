using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SlugBase.SaveData;

namespace PitchBlack
{
    public static class SaveManager
    {
        //Thanatosis
        public static bool canDoThanatosis;
        public static string thanatosisString = "AllowThanatosis";

        public static bool GetCanDoThanatosis(this SaveState save)
        {
            var data = save.deathPersistentSaveData.GetSlugBaseData();
            if (!data.TryGet(thanatosisString, out bool canDoThanatosis))
            {
                canDoThanatosis = false;
            }
            return canDoThanatosis;
        }

        public static void SetCanDoThanatosis(this SaveState save, bool canDoThanatosis) => save.deathPersistentSaveData.GetSlugBaseData().Set(thanatosisString, canDoThanatosis);

    }
}
