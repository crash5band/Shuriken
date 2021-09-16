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
        public ObservableCollection<SpriteViewModel> Sprites { get; set; }

        [Browsable(false)]
        public ObservableCollection<AnimationGroup> Animations { get; set; }

        public UIScene(Scene scene, string sceneName, List<CastDictionary> castDictionary, ObservableCollection<Texture> textures,
            ObservableCollection<UIFont> fonts)
        {
            Name = sceneName;
            Field00 = scene.Field00;
            ZIndex = scene.ZIndex;
            Field0C = scene.Field0C;
            Field10 = scene.Field10;
            AspectRatio = scene.AspectRatio;
            AnimationFramerate = scene.AnimationFramerate;
            Data1 = new List<Vector2>();
            Sprites = new ObservableCollection<SpriteViewModel>();
            Animations = new ObservableCollection<AnimationGroup>();

            Groups = new ObservableCollection<UIGroup>();

            CreateGroups(scene);
            CreateSprites(scene, textures);
            CreateLayers(scene, castDictionary);
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
            Sprites = new ObservableCollection<SpriteViewModel>();
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

        private void CreateLayers(Scene scene, List<CastDictionary> castDictionary)
        {
            for (int g = 0; g < scene.GroupCount; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].CastCount; ++c)
                {
                    UILayer layer = new UILayer(scene.UICastGroups[g].Casts[c], GetLayerName(g, c, castDictionary));
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
                        List<AnimationTrack> anims = new List<AnimationTrack>((int)XNCPLib.Utilities.Utilities.CountSetBits(castAnimData.Flags));

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

                                anim.Keyframes.Capacity = (int)castAnimData.SubDataList[castAnimDataIndex].KeyframeCount;
                                foreach (var key in castAnimData.SubDataList[castAnimDataIndex].Keyframes)
                                {
                                    anim.Keyframes.Add(new Keyframe(key));
                                }

                                anims.Add(anim);
                                ++castAnimDataIndex;
                            }
                        }

                        if (anims.Count > 0)
                            group.LayerAnimations.Add(Groups[g].Layers[c], anims);
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
