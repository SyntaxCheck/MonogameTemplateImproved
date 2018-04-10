using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera
{
    // Construct a new Camera class with standard zoom (no scaling)
    public Camera()
    {
        Zoom = 0.5f;
    }

    // Centered Position of the Camera in pixels.
    public Vector2 Position { get; private set; }
    // Current Zoom level with 1.0f being standard
    public float Zoom { get; private set; }
    private float ZoomMin = 0.25f;
    private float ZoomMax = 10.0f;
    // Current Rotation amount with 0.0f being standard orientation
    public float Rotation { get; private set; }

    // Height and width of the viewport window which we need to adjust
    // any time the player resizes the game window.
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }

    // Center of the Viewport which does not account for scale
    public Vector2 ViewportCenter
    {
        get
        {
            return new Vector2(ViewportWidth * 0.5f, ViewportHeight * 0.5f);
        }
    }

    // Create a matrix for the camera to offset everything we draw,
    // the map and our objects. since the camera coordinates are where
    // the camera is, we offset everything by the negative of that to simulate
    // a camera moving. We also cast to integers to avoid filtering artifacts.
    public Matrix TranslationMatrix
    {
        get
        {
            return Matrix.CreateTranslation(-(int)Position.X,
               -(int)Position.Y, 0) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
               Matrix.CreateTranslation(new Vector3(ViewportCenter, 0));
        }
    }

    // Call this method with negative values to zoom out
    // or positive values to zoom in. It looks at the current zoom
    // and adjusts it by the specified amount. If we were at a 1.0f
    // zoom level and specified -0.5f amount it would leave us with
    // 1.0f - 0.5f = 0.5f so everything would be drawn at half size.
    public void AdjustZoom(float amount)
    {
        float zoomAmount = 0f;
        if (amount > 0)
        {
            zoomAmount = (ZoomMax - Zoom) / 4;
        }
        else
        {
            zoomAmount = (Zoom - ZoomMin) / -4;
        }
        if (zoomAmount > 0.1f)
            zoomAmount = 0.1f;
        if (zoomAmount < -0.1f)
            zoomAmount = -0.1f;

        Zoom += zoomAmount;
        if (Zoom < ZoomMin)
        {
            Zoom = ZoomMin;
        }
        if (Zoom > ZoomMax)
        {
            Zoom = ZoomMax;
        }
    }

    // Move the camera in an X and Y amount based on the cameraMovement param.
    // if clampToMap is true the camera will try not to pan outside of the
    // bounds of the map.
    public void MoveCamera(Vector2 cameraMovement, bool clampToMap = false)
    {
        Vector2 newPosition = Position + cameraMovement;

        if (clampToMap)
        {
            Position = MapClampedPosition(newPosition);
        }
        else
        {
            Position = newPosition;
        }
    }

    public Rectangle ViewportWorldBoundry()
    {
        Vector2 viewPortCorner = ScreenToWorld(new Vector2(0, 0));
        Vector2 viewPortBottomCorner =
           ScreenToWorld(new Vector2(ViewportWidth, ViewportHeight));

        return new Rectangle((int)viewPortCorner.X,
           (int)viewPortCorner.Y,
           (int)(viewPortBottomCorner.X - viewPortCorner.X),
           (int)(viewPortBottomCorner.Y - viewPortCorner.Y));
    }

    // Center the camera on specific pixel coordinates
    public void CenterOn(Vector2 position)
    {
        Position = position;
    }

    // Clamp the camera so it never leaves the visible area of the map.
    private Vector2 MapClampedPosition(Vector2 position)
    {
        var cameraMax = new Vector2(
            Global.WORLD_SIZE - (ViewportWidth / Zoom / 2) + 100,
            Global.WORLD_SIZE - (ViewportHeight / Zoom / 2) + 100
        );

        Vector2 clamped = new Vector2();
        clamped = Vector2.Clamp(position, new Vector2((ViewportWidth / Zoom / 2) - 100, (ViewportHeight / Zoom / 2) - 100), cameraMax);
        return clamped;
    }

    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition, TranslationMatrix);
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition,
            Matrix.Invert(TranslationMatrix));
    }

    // Move the camera's position based on input
    public void HandleInput(InputState inputState,
       PlayerIndex? controllingPlayer)
    {
        Vector2 cameraMovement = Vector2.Zero;
        float cameraMovementAmount = 0.1f / Zoom; //Adjust Movement based on Zoom

        if (inputState.IsScrollLeft(controllingPlayer))
        {
            cameraMovement.X = cameraMovementAmount * -1;
        }
        else if (inputState.IsScrollRight(controllingPlayer))
        {
            cameraMovement.X = cameraMovementAmount;
        }
        if (inputState.IsScrollUp(controllingPlayer))
        {
            cameraMovement.Y = cameraMovementAmount * -1;
        }
        else if (inputState.IsScrollDown(controllingPlayer))
        {
            cameraMovement.Y = cameraMovementAmount;
        }
        if (inputState.IsZoomIn(controllingPlayer))
        {
            AdjustZoom(0.25f);
        }
        else if (inputState.IsZoomOut(controllingPlayer))
        {
            AdjustZoom(-0.25f);
        }

        // When using a controller, to match the thumbstick behavior,
        // we need to normalize non-zero vectors in case the user
        // is pressing a diagonal direction.
        if (cameraMovement != Vector2.Zero)
        {
            cameraMovement.Normalize();
        }

        // scale our movement to move 25 pixels per second
        cameraMovement *= 25f;

        MoveCamera(cameraMovement, false);
    }
}