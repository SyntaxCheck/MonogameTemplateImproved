using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameData
{
    public GameSettings Settings { get; set; }
    public MapStatistics MapStatistics { get; set; } //Map stats for the top bar on the HUD
    public List<SpriteBase> Sprites { get; set; } //List of sprites on the map
    public List<SpriteBase> DeadSprites{ get; set; } //Used for writing stats at the end
    public SpriteBase Focus { get; set; } //Camera focus, the camera class will follow whatever Creature is selected here
    public int FocusIndex { get; set; } //Camera focus index, this value is used when Paging between Creatures
    public GridData[,] MapGridData { get; set; }
    public bool ResetGame { get; set; }
    public bool BuildSettingsPanel { get; set; }
    public bool ShowChart { get; set; }
    public bool ShowControls { get; set; }
    public bool HighlightSprite { get; set; }
    public bool SpriteMarkers { get; set; }
    public bool ShowSpriteStats { get; set; }
    public bool ShowDebugData { get; set; }
    public bool ShowSettingsPanel { get; set; }

    public GameData()
    {
        MapStatistics = new MapStatistics();
        Sprites = new List<SpriteBase>();
        DeadSprites = new List<SpriteBase>();
        Focus = null; //Init the focus to null to not follow any creatures
        FocusIndex = -1;
        HighlightSprite = false;
        SpriteMarkers = false;
        ShowChart = true;
        ShowControls = true;
        ShowDebugData = false;
        ShowSettingsPanel = false;
        ResetGame = false;
        BuildSettingsPanel = false;
    }

    public void SetIndexPositionsForSprites()
    {
        for (int i = 0; i < Sprites.Count; i++)
        {
            if (Sprites[i] == Focus)
            {
                FocusIndex = i;
                break;
            }
        }
    }
    public void AddDeadSpriteToList(SpriteBase creature)
    {
        DeadSprites.Add(creature);
    }
    public void AddSpriteToGrid(SpriteBase sprite)
    {
        foreach (Point p in sprite.GridPositions)
        {
            MapGridData[p.X, p.Y].Sprites.Add(sprite);
        }
    }
    public void AddSpriteDeltaToGrid(SpriteBase sprite, List<Point> toBeAdded)
    {
        foreach (Point p in toBeAdded)
        {
            MapGridData[p.X, p.Y].Sprites.Add(sprite);
        }
    }
    public void RemoveSpriteFromGrid(SpriteBase sprite, List<Point> toBeRemoved)
    {
        foreach (Point p in toBeRemoved)
        {
            MapGridData[p.X, p.Y].Sprites.Remove(sprite);
        }
    }
}