using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using XNCPLib.XNCP;
using XNCPLib.XNCP.Animation;
using Shuriken.Models;
using Shuriken.Commands;
using System.Windows;
using Shuriken.Misc;
using System.Reflection;
using Shuriken.Models.Animation;

namespace Shuriken.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public List<string> MissingTextures { get; set; }
        public ObservableCollection<ViewModelBase> Editors { get; set; }

        // File Info
        public FAPCFile WorkFile { get; set; }
        public string WorkFilePath { get; set; }
        public bool IsLoaded { get; set; }
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

            IsLoaded = false;
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
            WorkFile = new FAPCFile();
            WorkFile.Load(filename);

            string root = Path.GetDirectoryName(Path.GetFullPath(filename));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xSceneIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
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

            List<FontID> fontIDSorted = xFontList.FontIDTable.OrderBy(o => o.Index).ToList();
            for (int i = 0; i < xFontList.FontIDTable.Count; i++)
            {
                int id = Project.CreateFont(fontIDSorted[i].Name);
                UIFont font = Project.TryGetFont(id);
                foreach (var mapping in xFontList.Fonts[i].CharacterMappings)
                {
                    var sprite = Utilities.FindSpriteIDFromNCPScene((int)mapping.SubImageIndex, xScenes[0].SubImages, texList.Textures);
                    font.Mappings.Add(new Models.CharacterMapping(mapping.SourceCharacter, sprite));
                }
            }

            List<SceneID> xSceneIDSorted = xSceneIDs.OrderBy(o => o.Index).ToList();
            for (int i = 0; i < xScenes.Count; i++)
            {
                Project.Scenes.Add(new UIScene(xScenes[i], xSceneIDSorted[i].Name, texList));
            }

            Project.TextureLists.Add(texList);

            WorkFilePath = filename;
            IsLoaded = !MissingTextures.Any();
        }

        // Very barebones save method which doesn't add anything into the original NCP file, and only changes what's already there
        // It also *may* not save everything, but it's progress...
        public void Save(string path)
        {
            if (path == null) path = WorkFilePath;
            else WorkFilePath = path;

            // TODO: We should create a FACPFile from scratch instead of overwritting the working one

            string root = Path.GetDirectoryName(Path.GetFullPath(WorkFilePath));

            List<Scene> xScenes = WorkFile.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = WorkFile.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = WorkFile.Resources[1].Content.TextureList.Textures;
            FontList xFontList = WorkFile.Resources[0].Content.CsdmProject.Fonts;

            List<SubImage> subImageList = new();
            List<Sprite> spriteList = new();
            BuildSubImageList(ref subImageList, ref spriteList);

            SaveTextures(xTextures);
            SaveFonts(xFontList, spriteList);

            List<System.Numerics.Vector2> Data1 = new();
            TextureList texList = Project.TextureLists[0];
            foreach (Texture tex in texList.Textures)
            {
                Data1.Add(new System.Numerics.Vector2(tex.Width / 1280F, tex.Height / 720F));
            }

            CSDNode rootNode = new();
            SaveScenes(rootNode, subImageList, Data1, spriteList);

            // TODO: REMOVE HACK to use original Data2
            for (int s = 0; s < xScenes.Count; s++)
            {
                rootNode.Scenes[s].AnimationData2List = xScenes[s].AnimationData2List;
            }

            WorkFile.Resources[0].Content.CsdmProject.Root = rootNode;

            WorkFile.Save(path);
        }

        private void BuildSubImageList(ref List<SubImage> subImages, ref List<Sprite> spriteList)
        {
            subImages = new();
            spriteList = new();

            TextureList texList = Project.TextureLists[0];
            foreach (var entry in Project.Sprites)
            {
                Sprite sprite = entry.Value;
                int textureIndex = texList.Textures.IndexOf(sprite.Texture);
                spriteList.Add(sprite);

                SubImage subImage = new();
                subImage.TextureIndex = (uint)textureIndex;
                subImage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, (float)sprite.Y / sprite.Texture.Height);
                subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / sprite.Texture.Width, (float)(sprite.Y + sprite.Height) / sprite.Texture.Height);
                subImages.Add(subImage);
            }
        }

        private void SaveTextures(List<XTexture> xTextures)
        {
            xTextures.Clear();
            TextureList texList = Project.TextureLists[0];
            foreach (Texture texture in texList.Textures)
            {
                XTexture xTexture = new();
                xTexture.Name = texture.Name + ".dds";
                xTextures.Add(xTexture);
            }
        }

        private void SaveFonts(FontList xFontList, List<Sprite> spriteList)
        {
            xFontList.Fonts.Clear();
            xFontList.FontIDTable.Clear();

            TextureList texList = Project.TextureLists[0];
            foreach (var entry in Project.Fonts)
            {
                UIFont uiFont = entry.Value;

                // NOTE: need to sort by name after
                FontID fontID = new();
                fontID.Index = (uint)xFontList.FontIDTable.Count;
                fontID.Name = uiFont.Name;
                xFontList.FontIDTable.Add(fontID);

                Font font = new();
                foreach (var mapping in uiFont.Mappings)
                {
                    // This seems to work fine, but causes different values to be saved in ui_gameplay.xncp. Duplicate subimage entry?
                    XNCPLib.XNCP.CharacterMapping characterMapping = new();
                    characterMapping.SubImageIndex = (uint)spriteList.IndexOf(Project.TryGetSprite(mapping.Sprite));
                    characterMapping.SourceCharacter = mapping.Character;
                    Debug.Assert(characterMapping.SubImageIndex != 0xFFFFFFFF);
                    font.CharacterMappings.Add(characterMapping);
                }
                xFontList.Fonts.Add(font);
            }

            // Sort font names
            xFontList.FontIDTable = xFontList.FontIDTable.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }

        private void SaveScenes(CSDNode xNode, List<SubImage> subImageList, List<System.Numerics.Vector2> Data1, List<Sprite> spriteList)
        {
            // TODO: sub nodes, sort sub node names

            xNode.Scenes.Clear();
            xNode.SceneIDTable.Clear();

            // Save individual scenes
            for (int s = 0; s < Project.Scenes.Count; s++)
            {
                UIScene uiScene = Project.Scenes[s];
                Scene xScene = new();

                // Save scene parameters
                xScene.Field00 = uiScene.Field00;
                xScene.ZIndex = uiScene.ZIndex;
                xScene.AnimationFramerate = uiScene.AnimationFramerate;
                xScene.Field0C = uiScene.Field0C;
                xScene.Field10 = uiScene.Field10;
                xScene.AspectRatio = uiScene.AspectRatio;
                xScene.Data1 = Data1;
                xScene.SubImages = subImageList;

                // Initial AnimationKeyframeData so we can add groups and cast data in it
                foreach (AnimationGroup animGroup in uiScene.Animations)
                {
                    AnimationKeyframeData keyframeData = new();
                    xScene.AnimationKeyframeDataList.Add(keyframeData);

                    AnimationData2 animationData2 = new();
                    xScene.AnimationData2List.Add(animationData2);

                    // Add animation names, NOTE: need to be sorted after
                    AnimationDictionary animationDictionary = new();
                    animationDictionary.Index = (uint)xScene.AnimationDictionaries.Count;
                    animationDictionary.Name = animGroup.Name;
                    xScene.AnimationDictionaries.Add(animationDictionary);

                    // AnimationFrameDataList
                    AnimationFrameData animationFrameData = new();
                    animationFrameData.Field00 = animGroup.Field00;
                    animationFrameData.FrameCount = animGroup.Duration;
                    xScene.AnimationFrameDataList.Add(animationFrameData);
                }

                // Sort animation names
                xScene.AnimationDictionaries = xScene.AnimationDictionaries.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();

                for (int g = 0; g < uiScene.Groups.Count; g++)
                {
                    CastGroup xCastGroup = new();
                    UICastGroup uiCastGroup = uiScene.Groups[g];

                    xCastGroup.Field08 = uiCastGroup.Field08;

                    // Get 1-dimensional UICast list, this will be in order of casts from top to bottom in UI
                    // TODO: the list order is not the same as original!
                    List<UICast> uiCastList = new();
                    GetAllUICastInGroup(uiCastGroup.Casts, uiCastList);
                    SaveCasts(uiCastList, xCastGroup, spriteList);

                    // Save the hierarchy tree for the current group
                    xCastGroup.CastHierarchyTree = new();
                    xCastGroup.CastHierarchyTree.AddRange
                    (
                        Enumerable.Repeat(new CastHierarchyTreeNode(-1, -1), uiCastList.Count)
                    );
                    SaveHierarchyTree(uiCastGroup.Casts, uiCastList, xCastGroup.CastHierarchyTree);

                    // Add cast name to dictionary, NOTE: this need to be sorted after
                    for (int c = 0; c < uiCastList.Count; c++)
                    {
                        CastDictionary castDictionary = new();
                        castDictionary.Name = uiCastList[c].Name;
                        castDictionary.GroupIndex = (uint)g;
                        castDictionary.CastIndex = (uint)c;
                        xScene.CastDictionaries.Add(castDictionary);
                    }
                    xScene.UICastGroups.Add(xCastGroup);

                    // Take this oppotunatity to fill group cast keyframe data
                    for (int a = 0; a < xScene.AnimationKeyframeDataList.Count; a++)
                    {
                        AnimationData2 animationData2 = xScene.AnimationData2List[a];
                        animationData2.GroupList = new();
                        animationData2.GroupList.GroupList = new();
                        animationData2.GroupList.Field00 = 0; // TODO:

                        AnimationKeyframeData animationKeyframeData = xScene.AnimationKeyframeDataList[a];
                        AnimationGroup animation = uiScene.Animations[a];

                        GroupAnimationData2 groupAnimationData2 = new();
                        groupAnimationData2.AnimationData2List = new();
                        groupAnimationData2.AnimationData2List.ListData = new();

                        GroupAnimationData groupAnimationData = new();
                        for (int c = 0; c < uiCastList.Count; c++)
                        {
                            CastAnimationData2 castAnimationData2 = new();
                            castAnimationData2.Data = new();
                            CastAnimationData castAnimationData = new();

                            UICast uiCast = uiCastList[c];
                            for (int t = 0; t < 12; t++)
                            {
                                AnimationType type = (AnimationType)(1u << t);
                                AnimationTrack animationTrack = animation.GetTrack(uiCast, type);
                                if (animationTrack == null) continue;
                                castAnimationData.Flags |= (uint)type;

                                // Initialize if we haven't
                                if (castAnimationData2.Data.SubData == null)
                                {
                                    castAnimationData2.Data.SubData = new();
                                }

                                Data6 data6 = new();
                                data6.Data = new();
                                data6.Data.Data = new();

                                CastAnimationSubData castAnimationSubData = new();
                                castAnimationSubData.Field00 = animationTrack.Field00;
                                foreach (Models.Animation.Keyframe keyframe in animationTrack.Keyframes)
                                {
                                    XNCPLib.XNCP.Animation.Keyframe xKeyframe = new();
                                    xKeyframe.Frame = keyframe.HasNoFrame ? 0xFFFFFFFF : (uint)keyframe.Frame;
                                    xKeyframe.Value = keyframe.KValue;
                                    xKeyframe.Field08 = (uint)keyframe.Field08;
                                    xKeyframe.Offset1 = keyframe.Offset1;
                                    xKeyframe.Offset2 = keyframe.Offset2;
                                    xKeyframe.Field14 = (uint)keyframe.Field14;
                                    castAnimationSubData.Keyframes.Add(xKeyframe);

                                    Data8 data8 = new();
                                    data8.Value = new System.Numerics.Vector3(0, 0, 0); // TODO:
                                    data6.Data.Data.Add(data8);
                                }

                                castAnimationData2.Data.SubData.Add(data6);
                                castAnimationData.SubDataList.Add(castAnimationSubData);
                            }

                            groupAnimationData2.AnimationData2List.ListData.Add(castAnimationData2);
                            groupAnimationData.CastAnimationDataList.Add(castAnimationData);
                        }

                        animationData2.GroupList.GroupList.Add(groupAnimationData2);
                        animationKeyframeData.GroupAnimationDataList.Add(groupAnimationData);
                    }
                }

                // Sort cast names
                xScene.CastDictionaries = xScene.CastDictionaries.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();

                // Add scene name to dictionary, NOTE: this need to sorted after
                SceneID xSceneID = new();
                xSceneID.Name = uiScene.Name;
                xSceneID.Index = (uint)s;
                xNode.SceneIDTable.Add(xSceneID);

                xNode.Scenes.Add(xScene);
            }

            // Sort scene names
            xNode.SceneIDTable = xNode.SceneIDTable.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }

        private void GetAllUICastInGroup(ObservableCollection<UICast> children, List<UICast> uiCastList)
        {
            foreach (UICast uiCast in children)
            {
                uiCastList.Add(uiCast);
                GetAllUICastInGroup(uiCast.Children, uiCastList);
            }
        }

        private void SaveHierarchyTree(ObservableCollection<UICast> children, List<UICast> uiCastList, List<CastHierarchyTreeNode> tree)
        {
            for (int i = 0; i < children.Count; i++)
            {
                UICast uiCast = children[i];

                int currentIndex = uiCastList.IndexOf(uiCast);
                Debug.Assert(currentIndex != -1);
                CastHierarchyTreeNode castHierarchyTreeNode = new(-1, -1);

                if (uiCast.Children.Count > 0)
                {
                    castHierarchyTreeNode.ChildIndex = uiCastList.IndexOf(uiCast.Children[0]);
                    Debug.Assert(castHierarchyTreeNode.ChildIndex != -1);
                }

                if (i + 1 < children.Count)
                {
                    castHierarchyTreeNode.NextIndex = uiCastList.IndexOf(children[i + 1]);
                    Debug.Assert(castHierarchyTreeNode.NextIndex != -1);
                }

                tree[currentIndex] = castHierarchyTreeNode;
                SaveHierarchyTree(uiCast.Children, uiCastList, tree);
            }
        }

        private void SaveCasts(List<UICast> uiCastList, CastGroup xCastGroup, List<Sprite> spriteList)
        {
            foreach (UICast uiCast in uiCastList)
            {
                Cast xCast = new();

                xCast.Field00 = uiCast.Field00;
                xCast.Field04 = (uint)uiCast.Type;
                xCast.IsEnabled = uiCast.IsEnabled ? 1u : 0u;

                xCast.TopLeft = new Vector2(uiCast.TopLeft);
                xCast.TopRight = new Vector2(uiCast.TopRight);
                xCast.BottomLeft = new Vector2(uiCast.BottomLeft);
                xCast.BottomRight = new Vector2(uiCast.BottomRight);

                xCast.Field2C = uiCast.Field2C;
                xCast.Field34 = uiCast.Field34;
                xCast.Field38 = uiCast.Flags;
                xCast.Field3C = uiCast.Field3C;

                xCast.FontCharacters = uiCast.FontCharacters;
                if (uiCast.Type == DrawType.Font)
                {
                    UIFont uiFont = Project.TryGetFont(uiCast.FontID);
                    if (uiFont != null)
                    {
                        xCast.FontName = uiFont.Name;
                    }
                }
                xCast.FontSpacingAdjustment = uiCast.FontSpacingAdjustment;

                xCast.Width = uiCast.Width;
                xCast.Height = uiCast.Height;
                xCast.Field58 = uiCast.Field58;
                xCast.Field5C = uiCast.Field5C;

                xCast.Offset = new Vector2(uiCast.Offset);
                
                xCast.Field68 = uiCast.Field68;
                xCast.Field6C = uiCast.Field6C;
                xCast.Field70 = uiCast.Field70;

                // Cast Info
                xCast.CastInfoData = new();
                xCast.CastInfoData.Field00 = uiCast.InfoField00;
                xCast.CastInfoData.Translation = new Vector2(uiCast.Translation);
                xCast.CastInfoData.Rotation = uiCast.Rotation;
                xCast.CastInfoData.Scale = new(uiCast.Scale.X, uiCast.Scale.Y);
                xCast.CastInfoData.Field18 = uiCast.InfoField18;
                xCast.CastInfoData.Color = uiCast.Color.ToUint();
                xCast.CastInfoData.GradientTopLeft = uiCast.GradientTopLeft.ToUint();
                xCast.CastInfoData.GradientBottomLeft = uiCast.GradientBottomLeft.ToUint();
                xCast.CastInfoData.GradientTopRight = uiCast.GradientTopRight.ToUint();
                xCast.CastInfoData.GradientBottomRight = uiCast.GradientBottomRight.ToUint();
                xCast.CastInfoData.Field30 = uiCast.InfoField30;
                xCast.CastInfoData.Field34 = uiCast.InfoField34;
                xCast.CastInfoData.Field38 = uiCast.InfoField38;

                // Cast Material Info
                xCast.CastMaterialData = new();
                Debug.Assert(uiCast.Sprites.Count == 32);
                for (int index = 0; index < 32; index++)
                {
                    if (uiCast.Sprites[index] == -1)
                    {
                        xCast.CastMaterialData.SubImageIndices[index] = -1;
                        continue;
                    }

                    Sprite uiSprite = Project.TryGetSprite(uiCast.Sprites[index]);
                    xCast.CastMaterialData.SubImageIndices[index] = spriteList.IndexOf(uiSprite);
                }

                xCastGroup.Casts.Add(xCast);
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
            builder.AppendLine("The loaded UI file uses textures that were not found. Saving has been disabled. In order to save, please copy the files listed below into the UI file's directory, and re-open it.\n");
            foreach (var texture in MissingTextures)
                builder.AppendLine(texture);

            MessageBox.Show(builder.ToString(), "Missing Textures", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
