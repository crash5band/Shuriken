using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNCPLib.XNCP;
using System.ComponentModel;
using Shuriken.ViewModels;
using Shuriken.Models.Animation;
using Shuriken.Misc;

namespace Shuriken.Models
{
    public class UIScene
    {
        [Category("Scene")]
        public string Name { get; set; }

        [Category("Scene")]
        public uint Field00 { get; set; }

        [Category("Scene")]
        public float ZIndex { get; set; }

        [Category("Scene")]
        public uint Field0C { get; set; }

        [Category("Scene")]
        public float Field10 { get; set; }

        [Category("Scene")]
        public float AspectRatio { get; set; }

        [Category("Animation")]
        public float AnimationFramerate { get; set; }

        [Category("Data1")]
        public List<Vector2> Data1 { get; set; }

        [Browsable(false)]
        public ObservableCollection<UIGroup> Groups { get; set; }

        [Browsable(false)]
        public ObservableCollection<AnimationGroup> Animations { get; set; }

        public UIScene(Scene scene, string sceneName, TextureList texList, IEnumerable<UIFont> fonts)
        {
            Name = sceneName;
            Field00 = scene.Field00;
            ZIndex = scene.ZIndex;
            Field0C = scene.Field0C;
            Field10 = scene.Field10;
            AspectRatio = scene.AspectRatio;
            AnimationFramerate = scene.AnimationFramerate;
            Data1 = new List<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();

            Groups = new ObservableCollection<UIGroup>();

            CreateGroups(scene);
            //CreateSprites(scene);
            CreateLayers(scene, scene.CastDictionaries, texList);
            CreateHierarchyTree(scene);
            CreateAnimations(scene);
        }

        public UIScene(string sceneName)
        {
            Name = sceneName;
            ZIndex = 0;
            AspectRatio = 16.0f / 9.0f;
            AnimationFramerate = 60.0f;
            Groups = new ObservableCollection<UIGroup>();
            Data1 = new List<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();
        }

        private void CreateGroups(Scene scene)
        {
            for (int i = 0; i < scene.GroupCount; ++i)
            {
                UIGroup group = new UIGroup("UIGroup_" + i);
                group.Field08 = scene.UICastGroups[i].Field08;

                Groups.Add(group);
            }
        }

        /*
        private void CreateSprites(Scene scene, ObservableCollection<Texture> textures)
        {
            foreach (var sub in scene.SubImages)
            {
                if ((int)sub.TextureIndex > -1 && (int)sub.TextureIndex < textures.Count)
                {
                    Sprites.Add(new SpriteViewModel(new Sprite(textures[(int)sub.TextureIndex], sub.TopLeft.Y, sub.TopLeft.X, sub.BottomRight.Y, sub.BottomRight.X)));
                }
            }
        }
        */

        private void CreateLayers(Scene scene, List<CastDictionary> castDictionary, TextureList texList)
        {
            for (int g = 0; g < scene.GroupCount; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].CastCount; ++c)
                {
                    int[] castSprites = scene.UICastGroups[g].Casts[c].CastMaterialData.SubImageIndices;
                    UILayer layer = new UILayer(scene.UICastGroups[g].Casts[c], GetLayerName(g, c, castDictionary));

                    for (int index = 0; index < layer.Sprites.Length; ++index)
                    {
                        layer.Sprites[index] = Utilities.FindSpriteFromNCPScene(castSprites[index], scene.SubImages, texList.Textures);
                    }

                    Groups[g].Layers.Add(layer);
                }
            }
        }

        private void CreateHierarchyTree(Scene scene)
        {
            for (int g = 0; g < scene.GroupCount; ++g)
            {
                List<CastHierarchyTreeNode> tree = scene.UICastGroups[g].CastHierarchyTree;
                BuildTree(g, 0, tree);
            }
        }

        private void BuildTree(int g, int c, List<CastHierarchyTreeNode> tree)
        {
            if (tree[c].ChildIndex != -1)
            {
                Groups[g].Layers[tree[c].ChildIndex].Parent = Groups[g].Layers[c];
                BuildTree(g, tree[c].ChildIndex, tree);
            }

            if (tree[c].NextIndex != -1)
            {
                Groups[g].Layers[tree[c].NextIndex].Parent = Groups[g].Layers[c].Parent;
                BuildTree(g, tree[c].NextIndex, tree);
            }
        }

        private void CreateAnimations(Scene scene)
        {
            foreach (var entry in scene.AnimationDictionaries)
            {
                AnimationGroup group = new AnimationGroup(entry.Name.Value)
                {
                    Field00 = scene.AnimationFrameDataList[(int)entry.Index].Field00,
                    Duration = scene.AnimationFrameDataList[(int)entry.Index].FrameCount
                };

                for (int g = 0; g < scene.GroupCount; ++g)
                {
                    XNCPLib.XNCP.Animation.AnimationKeyframeData keyframeData = scene.AnimationKeyframeDataList[(int)entry.Index];
                    for (int c = 0; c < keyframeData.GroupAnimationDataList[g].CastCount; ++c)
                    {
                        XNCPLib.XNCP.Animation.CastAnimationData castAnimData = keyframeData.GroupAnimationDataList[g].CastAnimationDataList[c];
                        List<AnimationTrack> tracks = new List<AnimationTrack>((int)XNCPLib.Utilities.Utilities.CountSetBits(castAnimData.Flags));

                        int castAnimDataIndex = 0;
                        for (int i = 0; i < 8; ++i)
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
                            AnimationList layerAnimationList = new AnimationList(Groups[g].Layers[c], tracks);
                            group.LayerAnimations.Add(layerAnimationList);
                        }
                    }
                }

                Animations.Add(group);
            }
        }

        public string GetLayerName(int groupIndex, int castIndex, List<CastDictionary> castDictionary)
        {
            foreach (var entry in castDictionary)
            {
                if (entry.GroupIndex == groupIndex && entry.CastIndex == castIndex)
                    return entry.Name.Value;
            }

            return "";
        }
    }
}
