using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using XNCPLib.XNCP;
using Shuriken.Models;
using Shuriken.Commands;
using System.Windows;
using Shuriken.Misc;
using System.Reflection;

namespace Shuriken.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public List<string> MissingTextures { get; set; }
        public ObservableCollection<ViewModelBase> Editors { get; set; }

        public FAPCFile WorkFile { get; set; }
        public string WorkFilePath { get; set; }
        public MainViewModel()
        {
            MissingTextures = new List<string>();

            Editors = new ObservableCollection<ViewModelBase>
            {
                new ScenesViewModel(),
                new SpritesViewModel(),
                new FontsViewModel(),
                new AboutViewModel()
            };

            WorkFile = new FAPCFile();
#if DEBUG
            //LoadTestXNCP();
#endif
        }

        public void LoadTestXNCP()
        {
            Load("Test/ui_gameplay.xncp");
        }

        /// <summary>
        /// Loads a Ninja Chao Project file for editing
        /// </summary>
        /// <param name="filename">The path of the file to load</param>
        public void Load(string filename)
        {
            WorkFile.Load(filename);

            string root = Path.GetDirectoryName(Path.GetFullPath(filename));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            Clear();

            TextureList texList = new TextureList("textures");
            foreach (XTexture texture in xTextures)
            {
                string texPath = Path.Combine(root, texture.Name);
                if (File.Exists(texPath))
                    texList.Textures.Add(new Texture(texPath));
                else
                    MissingTextures.Add(texture.Name);
            }

            if (MissingTextures.Count > 0)
                WarnMissingTextures();

            if (xScenes.Count > 0)
            {
                // Hack: we load sprites from the first scene only since whatever tool sonic team uses
                // seems to work the same way as SWIF:
                // Sprites belong to textures and layers and fonts reference a specific sprite using the texutre index and sprite index.
                int subImageIndex = 0;
                foreach (SubImage subimage in xScenes[0].SubImages)
                {
                    int textureIndex = (int)subimage.TextureIndex;
                    if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                    {
                        int id = Project.CreateSprite(texList.Textures[textureIndex], subimage.TopLeft.Y, subimage.TopLeft.X,
                            subimage.BottomRight.Y, subimage.BottomRight.X);
                        
                        texList.Textures[textureIndex].Sprites.Add(id);
                    }
                    ++subImageIndex;
                }
            }

            foreach (var entry in xFontList.FontIDTable)
            {
                UIFont font = new UIFont(entry.Name);
                foreach (var mapping in xFontList.Fonts[(int)entry.Index].CharacterMappings)
                {
                    var sprite = Utilities.FindSpriteIDFromNCPScene((int)mapping.SubImageIndex, xScenes[0].SubImages, texList.Textures);
                    font.Mappings.Add(new Models.CharacterMapping(mapping.SourceCharacter, sprite));
                }

                Project.Fonts.Add(font);
            }

            foreach (SceneID sceneID in xIDs)
                Project.Scenes.Add(new UIScene(xScenes[(int)sceneID.Index], sceneID.Name, texList, Project.Fonts));

            Project.TextureLists.Add(texList);

            WorkFilePath = filename;
        }

        // Very barebones save method which doesn't add anything into the original NCP file, and only changes what's already there
        // It also *may* not save anything, but it's progress...
        public void Save(string path)
        {
            if (path == null) path = WorkFilePath;

            string root = Path.GetDirectoryName(Path.GetFullPath(WorkFilePath));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            TextureList texList = Project.TextureLists[0];
            int nextTextureIndex = 0;
            foreach (XTexture texture in xTextures)
            {
                string texPath = Path.Combine(root, texture.Name);
                if (File.Exists(texPath))
                {
                    Texture t = texList.Textures[nextTextureIndex++];
                    texture.Name = Path.GetFileName(t.FullName).Substring(0, texture.Name.Length); // TODO: This will break with names larger than the original one
                }
                
            }

            int nextFontIndex = 0;
            foreach (var entry in xFontList.FontIDTable)
            {
                UIFont font = Project.Fonts[nextFontIndex++];
                int nextMappingIndex = 0;
                foreach (var mapping in xFontList.Fonts[(int)entry.Index].CharacterMappings)
                {
                    Models.CharacterMapping map = font.Mappings[nextMappingIndex++];
                    mapping.SourceCharacter = map.Character;
                    Sprite sp = Project.TryGetSprite(map.Sprite);
                    mapping.SubImageIndex = Utilities.FindSubImageIndexFromSprite(sp, xScenes[0].SubImages, texList.Textures); // TODO: This may be wrong...
                }
            }

            // Sprite Saving: Disabled for now due to conversion issues
            /*
            int nextSpriteId = 1;
            if (xScenes.Count > 0)
            {
                foreach (SubImage subimage in xScenes[0].SubImages)
                {
                    int textureIndex = (int)subimage.TextureIndex;
                    if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                    {
                        if (nextSpriteId == 303)
                        {
                            int abc = 123;
                        }
                        Sprite sprite = Project.TryGetSprite(nextSpriteId++);
                        int newTextureIndex = texList.Textures.IndexOf(sprite.Texture);

                        subimage.TextureIndex = (uint) newTextureIndex;
                        subimage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, sprite.Y / sprite.Texture.Height);
                        subimage.BottomRight = new Vector2(((float)sprite.Width / sprite.Texture.Width) + subimage.TopLeft.X, ((float)sprite.Height / sprite.Texture.Height) + subimage.TopLeft.Y);
                    }
                }
            }
            */

            int sceneIndex = 0;
            foreach (SceneID sceneID in xIDs)
            {
                Scene scene = xScenes[(int)sceneID.Index];
                UIScene uiScene = Project.Scenes[sceneIndex++];

                sceneID.Name = uiScene.Name.Substring(0, sceneID.Name.Length); // TODO: This will break with names larger than the original one

                scene.Field00 = uiScene.Field00;
                scene.ZIndex = uiScene.ZIndex;
                scene.Field0C = uiScene.Field0C;
                scene.Field10 = uiScene.Field10;
                scene.AspectRatio = uiScene.AspectRatio;
                scene.AnimationFramerate = uiScene.AnimationFramerate;

                int textureSizeIndex = 0;
                for (int i = 0; i < scene.Data1.Count; ++i)
                {
                    scene.Data1[i] = uiScene.TextureSizes[textureSizeIndex++];
                }

                SaveCasts(uiScene, scene);
            }

            WorkFile.Save(path);
        }

        private void SaveCasts(UIScene uiScene, Scene scene)
        {
            for (int g = 0; g < scene.GroupCount; ++g)
            {
                scene.UICastGroups[g].Field08 = uiScene.Groups[g].Field08;
            }

            // Pre-process animations
            Dictionary<int, int> entryIndexMap = new Dictionary<int, int>();
            int animIndex = 0;
            foreach (var entry in scene.AnimationDictionaries)
            {
                scene.AnimationFrameDataList[(int)entry.Index].Field00 = uiScene.Animations[animIndex].Field00;
                scene.AnimationFrameDataList[(int)entry.Index].FrameCount = uiScene.Animations[animIndex].Duration;

                entryIndexMap.Add(animIndex++, (int)entry.Index);
            }

            // process group layers
            for (int g = 0; g < uiScene.Groups.Count; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].CastCount; ++c)
                {
                    Cast cast = scene.UICastGroups[g].Casts[c];
                    UICast uiCast = uiScene.Groups[g].CastsOrderedByIndex[c];

                    cast.Field00 = uiCast.Field00;
                    cast.Field04 = (uint)uiCast.Type;
                    cast.IsEnabled = uiCast.IsEnabled ? (uint)1 : 0;

                    /* TODO: Process these
                    float right = Math.Abs(cast.TopRight.X) - Math.Abs(cast.TopLeft.X);
                    float top = Math.Abs(cast.TopRight.Y) - Math.Abs(cast.BottomRight.Y);
                    Anchor = new Vector2(right, top);
                    */

                    cast.Field2C = uiCast.Field2C;
                    cast.Field34 = uiCast.Field34;
                    cast.Field38 = uiCast.Flags;
                    cast.Field3C = uiCast.Field3C;

                    cast.FontCharacters = uiCast.FontCharacters;

                    cast.Field4C = uiCast.Field4C;
                    cast.Width = uiCast.Width;
                    cast.Height = uiCast.Height;
                    cast.Field58 = uiCast.Field58;
                    cast.Field5C = uiCast.Field5C;

                    cast.Offset = new Vector2(uiCast.Offset);

                    cast.Field68 = uiCast.Field68;
                    cast.Field6C = uiCast.Field6C;
                    cast.FontSpacingCorrection = uiCast.FontSpacingCorrection;

                    // Cast Info
                    cast.CastInfoData.Field00 = uiCast.InfoField00;
                    cast.CastInfoData.Translation = new Vector2(uiCast.Translation);
                    cast.CastInfoData.Rotation = uiCast.Rotation;
                    cast.CastInfoData.Scale = new Vector2(uiCast.Scale.X, uiCast.Scale.Y);

                    cast.CastInfoData.Field00 = uiCast.InfoField00;
                    cast.CastInfoData.Color = uiCast.Color.ToUint();
                    cast.CastInfoData.GradientTopLeft = uiCast.GradientTopLeft.ToUint();
                    cast.CastInfoData.GradientBottomLeft = uiCast.GradientBottomLeft.ToUint();
                    cast.CastInfoData.GradientTopRight = uiCast.GradientTopRight.ToUint();
                    cast.CastInfoData.GradientBottomRight = uiCast.GradientBottomRight.ToUint();
                    cast.CastInfoData.Field30 = uiCast.InfoField30;
                    cast.CastInfoData.Field34 = uiCast.InfoField34;
                    cast.CastInfoData.Field38 = uiCast.InfoField38;

                    /*
                    // Sprites (not correct)
                    if (uiCast.Type == DrawType.Sprite)
                    {
                        int[] castSprites = scene.UICastGroups[g].Casts[c].CastMaterialData.SubImageIndices;
                        for (int index = 0; index < cast.Sprites.Count; ++index)
                        {
                            cast.Sprites[index] = Utilities.FindSpriteIDFromNCPScene(castSprites[index], scene.SubImages, texList.Textures);
                        }
                    }
                    else if (uiCast.Type == DrawType.Font)
                    {
                        foreach (var font in fonts)
                        {
                            if (font.Name == scene.UICastGroups[g].Casts[c].FontName)
                                cast.Font = font;
                        }
                    }
                    */
                }

                /* Animations
                foreach (var entry in entryIndexMap)
                {
                    XNCPLib.XNCP.Animation.AnimationKeyframeData keyframeData = scene.AnimationKeyframeDataList[entry.Value];
                    for (int c = 0; c < keyframeData.GroupAnimationDataList[g].CastCount; ++c)
                    {
                        XNCPLib.XNCP.Animation.CastAnimationData castAnimData = keyframeData.GroupAnimationDataList[g].CastAnimationDataList[c];
                        List<AnimationTrack> tracks = new List<AnimationTrack>((int)XNCPLib.Misc.Utilities.CountSetBits(castAnimData.Flags));

                        int castAnimDataIndex = 0;
                        for (int i = 0; i < 12; ++i)
                        {
                            // check each animation type if it exists in Flags
                            if ((castAnimData.Flags & (1 << i)) != 0)
                            {
                                AnimationType type = (AnimationType)(1 << i);
                                AnimationTrack anim = new AnimationTrack(type)
                                {
                                    Field00 = castAnimData.SubDataList[castAnimDataIndex].Field00,
                                };

                                foreach (var key in castAnimData.SubDataList[castAnimDataIndex].Keyframes)
                                {
                                    anim.Keyframes.Add(new Keyframe(key));
                                }

                                tracks.Add(anim);
                                ++castAnimDataIndex;
                            }
                        }

                        if (tracks.Count > 0)
                        {
                            AnimationList layerAnimationList = new AnimationList(tempCasts[c], tracks);
                            Animations[entry.Key].LayerAnimations.Add(layerAnimationList);
                        }
                    }
                }
                */
            }
        }

        public void Clear()
        {
            Project.Clear();
            MissingTextures.Clear();
        }

        private void WarnMissingTextures()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("The loaded UI file uses textures that were not found.\n");
            foreach (var texture in MissingTextures)
                builder.AppendLine(texture);

            MessageBox.Show(builder.ToString(), "Missing Textures", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
