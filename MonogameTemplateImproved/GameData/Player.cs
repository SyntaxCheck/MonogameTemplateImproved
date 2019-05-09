using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Player
{
    public Player()
    { }

    public void HandleInput(InputState inputState, ref GameData gameData)
    {
        MouseState mouseState;
        if (inputState.IsNewLeftMouseClick(out mouseState))
        {
            Vector2 worldPosition = Vector2.Transform(new Vector2(mouseState.Position.X, mouseState.Position.Y), Matrix.Invert(Global.Camera.TranslationMatrix));
            gameData.Animations.Add(gameData.OnClickAnimFactory.Build(new Point((int)worldPosition.X, (int)worldPosition.Y)));
        }
    }
}