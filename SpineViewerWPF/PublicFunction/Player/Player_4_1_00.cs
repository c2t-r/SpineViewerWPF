﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpineViewerWPF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Spine4_1_00;

public class Player_4_1_00 : IPlayer
{
    private Skeleton skeleton;
    private AnimationState state;
    private SkeletonRenderer skeletonRenderer;
    private ExposedList<Animation> listAnimation;
    private ExposedList<Skin> listSkin;
    private Atlas atlas;
    private SkeletonData skeletonData;
    private AnimationStateData stateData;
    private SkeletonBinary binary;
    private SkeletonJson json;

    public void Initialize()
    {
        Player.Initialize(ref App.graphicsDevice, ref App.spriteBatch);
    }

    public void LoadContent(ContentManager contentManager)
    {
        skeletonRenderer = new SkeletonRenderer(App.graphicsDevice);
        skeletonRenderer.PremultipliedAlpha = App.globalValues.Alpha;

        if (App.mulitTexture != null && App.mulitTexture.Length == 0)
        {
            atlas = new Atlas(App.globalValues.SelectAtlasFile, new XnaTextureLoader(App.graphicsDevice));
        }
        else
        {
            atlas = new Atlas(App.globalValues.SelectAtlasFile, new XnaTextureLoader(App.graphicsDevice, true, App.mulitTexture));
        }

        if (Common.IsBinaryData(App.globalValues.SelectSpineFile))
        {
            binary = new SkeletonBinary(atlas);
            binary.Scale = App.globalValues.Scale;
            skeletonData = binary.ReadSkeletonData(App.globalValues.SelectSpineFile);
        }
        else
        {
            json = new SkeletonJson(atlas);
            json.Scale = App.globalValues.Scale;
            skeletonData = json.ReadSkeletonData(App.globalValues.SelectSpineFile);
        }
        App.globalValues.SpineVersion = skeletonData.Version;
        skeleton = new Skeleton(skeletonData);


        

        Common.SetInitLocation(skeleton.Data.Height);
        App.globalValues.FileHash = skeleton.Data.Hash;

        stateData = new AnimationStateData(skeleton.Data);

        state = new AnimationState(stateData);

        List<string> AnimationNames = new List<string>();
        listAnimation = state.Data.skeletonData.Animations;
        foreach (Animation An in listAnimation)
        {
            AnimationNames.Add(An.name);
        }
        App.globalValues.AnimeList = AnimationNames;

        List<string> SkinNames = new List<string>();
        listSkin = state.Data.skeletonData.Skins;
        foreach (Skin Sk in listSkin)
        {
            SkinNames.Add(Sk.name);
        }
        App.globalValues.SkinList = SkinNames;

        if (App.globalValues.SelectAnimeName != "")
        {
            state.SetAnimation(0, App.globalValues.SelectAnimeName, App.globalValues.IsLoop);
        }
        else
        {
            state.SetAnimation(0, state.Data.skeletonData.animations.Items[0].name, App.globalValues.IsLoop);
        }

        if (App.isNew)
        {
            if (App.globalValues.CoordinatedInCenter)
            {
                App.globalValues.PosX = (float)App.canvasWidth / 2;
                App.globalValues.PosY = (float)App.canvasHeight / 2;
            }
            else
            {
                App.globalValues.PosX = 0;
                App.globalValues.PosY = App.globalValues.SkeletonHeader.Height * App.globalValues.Scale;
            }
            MainWindow.SetCBAnimeName();
        }
        App.isNew = false;

    }



    public void Update(GameTime gameTime)
    {
        if (App.globalValues.SelectAnimeName != "" && App.globalValues.SetAnime)
        {
            state.ClearTracks();
            skeleton.SetToSetupPose();
            state.SetAnimation(0, App.globalValues.SelectAnimeName, App.globalValues.IsLoop);
            App.globalValues.SetAnime = false;
        }

        if (App.globalValues.SelectSkin != "" && App.globalValues.SetSkin)
        {
            skeleton.SetSkin(App.globalValues.SelectSkin);
            skeleton.SetSlotsToSetupPose();
            App.globalValues.SetSkin = false;
        }


    }

    public void Draw()
    {
        if (App.globalValues.SelectSpineVersion != "4.1.00" || App.globalValues.FileHash != skeleton.Data.Hash)
        {
            state = null;
            skeletonRenderer = null;
            return;
        }
        App.graphicsDevice.Clear(Color.Transparent);

        Player.DrawBG(ref App.spriteBatch);

        
        state.Update(App.globalValues.Speed / 1000f);
        state.Apply(skeleton);
        state.TimeScale = App.globalValues.TimeScale;
        if (binary != null)
        {
            if (App.globalValues.Scale != binary.Scale)
            {
                binary.Scale = App.globalValues.Scale;
                skeletonData = binary.ReadSkeletonData(App.globalValues.SelectSpineFile);
                skeleton = new Skeleton(skeletonData);
            }
        }
        else if (json != null)
        {
            if (App.globalValues.Scale != json.Scale)
            {
                json.Scale = App.globalValues.Scale;
                skeletonData = json.ReadSkeletonData(App.globalValues.SelectSpineFile);
                skeleton = new Skeleton(skeletonData);
            }
        }

        skeleton.X = App.globalValues.PosX;
        skeleton.Y = App.globalValues.PosY;
        skeleton.ScaleX = (App.globalValues.FilpX ? -1 : 1) ;
        skeleton.ScaleY = (App.globalValues.FilpY ? -1 : 1) ;


        skeleton.RootBone.Rotation = App.globalValues.Rotation;
        skeleton.UpdateWorldTransform();
        skeletonRenderer.PremultipliedAlpha = App.globalValues.Alpha;
        if (skeletonRenderer.Effect is BasicEffect)
        {
            ((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, App.graphicsDevice.Viewport.Width, App.graphicsDevice.Viewport.Height, 0, 1, 0);
        }
        else
        {
            skeletonRenderer.Effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, App.graphicsDevice.Viewport.Width, App.graphicsDevice.Viewport.Height, 0, 1, 0));
        }
        skeletonRenderer.Begin();
        skeletonRenderer.Draw(skeleton);
        skeletonRenderer.End();

        if (state != null)
        {
            TrackEntry entry = state.GetCurrent(0);
            if (entry != null)
            {
                if (App.globalValues.IsRecoding && (App.globalValues.GifList != null || App.recordImageCount >0 ) && !entry.IsComplete)
                {
                    if (App.recordImageCount == 1)
                    {
                        TrackEntry te = state.GetCurrent(0);
                        te.trackTime = 0;
                        App.globalValues.TimeScale = 1;
                        App.globalValues.Lock = 0;
                    }

                    Common.TakeRecodeScreenshot(App.graphicsDevice);
                }

                if (App.globalValues.IsRecoding && entry.IsComplete)
                {
                    state.TimeScale = 0;
                    App.globalValues.IsRecoding = false;
                    Common.RecodingEnd(entry.AnimationEnd);

                    state.TimeScale = 1;
                    App.globalValues.TimeScale = 1;
                }

                if (App.globalValues.TimeScale == 0)
                {
                    entry.TrackTime = entry.AnimationEnd * App.globalValues.Lock;
                    entry.TimeScale = 0;
                }
                else
                {
                    App.globalValues.Lock = entry.AnimationTime / entry.AnimationEnd;
                    entry.TimeScale = 1;
                }
                App.globalValues.LoadingProcess = $"{ Math.Round(entry.AnimationTime / entry.AnimationEnd * 100, 2)}%";
            }
        }


    }

    public void ChangeSet()
    {
        App.appXC.ContentManager.Dispose();
        atlas.Dispose();
        atlas = null;
        App.appXC.LoadContent.Invoke(App.appXC.ContentManager);
    }

    public void SizeChange()
    {
        if (App.graphicsDevice != null)
            Player.UserControl_SizeChanged(ref App.graphicsDevice);
    }

    public void Dispose()
    {
        ChangeSet();
    }
}

