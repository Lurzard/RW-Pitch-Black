using UnityEngine;
using Menu;
using Menu.Remix.MixedUI;

namespace PitchBlack;

public class CollectionDialogBox : MenuDialogBox
{
    public string text = "";
    public CollectionDialogBox(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size) : base(menu, owner, "", pos, size, true)
    {
        darkSprite.isVisible = false;
        darkSprite.RemoveFromContainer();
        foreach (FSprite sprite in roundedRect.sprites) {
            sprite.isVisible = false;
            sprite.RemoveFromContainer();
        }
        descriptionLabel.pos.y -= 10;
    }
    public override void Update()
    {
        base.Update();
        this.descriptionLabel.text = text.WrapText(false, size.x - 40, true);
    }
}