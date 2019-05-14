using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TextureContainer
{
    //To be used in a List of textures using the Enum to search the list
    public Texture2D Texture { get; set; }
    public TextureName Name { get; set; }

    //Could declare explicit textures for faster loading
    public Texture2D WhitePixel { get; set; }
    public Texture2D ParticleCollisionWestTopSpriteSheet { get; set; }
    public Texture2D ParticleCollisionWestBottomSpriteSheet { get; set; }
    public Texture2D ParticleCollisionEastTopSpriteSheet { get; set; }
    public Texture2D ParticleCollisionEastBottomSpriteSheet { get; set; }
    public Texture2D ParticleCollisionNorthLeftSpriteSheet { get; set; }
    public Texture2D ParticleCollisionNorthRightSpriteSheet { get; set; }
    public Texture2D ParticleCollisionSouthLeftSpriteSheet { get; set; }
    public Texture2D ParticleCollisionSouthRightSpriteSheet { get; set; }
    public Texture2D ClickExplosionSpriteSheet { get; set; }
    
    //Minimap textures
    public Texture2D MiniMapFrame { get; set; }
    public Texture2D MiniMapBuildingTexture { get; set; }
    public Texture2D MiniMapMovingObjectTexture { get; set; }

    public TextureContainer()
    {
    }
}

public enum TextureName
{
    WhitePixel,
    ParticleCollisionWestTop,
    ParticleCollisionWestBottom,
    ParticleCollisionEastTop,
    ParticleCollisionEastBottom,
    ParticleCollisionNorthLeft,
    ParticleCollisionNorthRight,
    ParticleCollisionSouthLeft,
    ParticleCollisionSouthRight,
    ClickExplosionSpriteSheet
}