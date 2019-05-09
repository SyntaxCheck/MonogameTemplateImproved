using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextAnimatorFactory
{
    public SpriteFont TextSpriteFont { get; set; }
    public Texture2D TextBackgoundTexture { get; set; }

    public TextAnimatorFactory(SpriteFont textSpriteFont, Texture2D backgroundTexture)
    {
        TextSpriteFont = textSpriteFont;
        TextBackgoundTexture = backgroundTexture;
    }

    public TextAnimatorPositionAndOpacity Build(string text, double totalDurationMilliseconds, double millisecondsBetweenTicks, Vector2 startPosition, Vector2 endPosition, int startOpacity, int endOpacity, Color textColor)
    {
        TextAnimatorPositionAndOpacity tapo = new TextAnimatorPositionAndOpacity();

        tapo.Load(text, TextSpriteFont, TextBackgoundTexture, totalDurationMilliseconds, millisecondsBetweenTicks, startPosition, endPosition, startOpacity, endOpacity, textColor);

        return tapo;
    }
}