using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNCPLib.XNCP;
using System.ComponentModel;
using Shuriken.Models.Animation;
using Shuriken.Misc;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class UIScene : INotifyPropertyChanged, IComparable<UIScene>
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private uint field00;
        public uint Field00
        {
            get => field00;
            set { field00 = value; NotifyPropertyChanged(); }
        }

        private float zIndex;
        public float ZIndex
        {
            get => zIndex;
            set { zIndex = value; NotifyPropertyChanged(); }
        }

        private uint field0C;
        public uint Field0C
        {
            get => field0C;
            set { field0C = value; NotifyPropertyChanged(); }
        }

        private float field10;
        public float Field10
        {
            get => field10;
            set { field10 = value; NotifyPropertyChanged(); }
        }

        private float aspectRatio;
        public float AspectRatio
        {
            get => aspectRatio;
            set { aspectRatio = value; NotifyPropertyChanged(); }
        }

        private float animationFramerate;
        public float AnimationFramerate
        {
            get => animationFramerate;
            set { animationFramerate = value; NotifyPropertyChanged(); }
        }

        private bool visible;
        public bool Visible
        {
            get => visible;
            set { visible = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<Vector2> TextureSizes { get; set; }

        public ObservableCollection<LayerGroup> Groups { get; set; }

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
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();
            Groups = new ObservableCollection<LayerGroup>();

            foreach (var texSize in scene.Data1)
            {
                TextureSizes.Add(new Vector2(texSize.X, texSize.Y));
            }

            ProcessCasts(scene, texList, fonts);
            Visible = false;
        }

        public UIScene(string sceneName)
        {
            Name = sceneName;
            ZIndex = 0;
            AspectRatio = 16.0f / 9.0f;
            AnimationFramerate = 60.0f;
            Groups = new ObservableCollection<LayerGroup>();
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();

            Visible = false;
        }

        private void ProcessCasts(Scene scene, TextureList texList, IEnumerable<UIFont> fonts)
        {
            // Create groups
            for (int g = 0; g < scene.GroupCount; ++g)
            {
                Groups.Add(new LayerGroup
                {
                    Name = "Group_" + g,
                    Field08 = scene.UICastGroups[g].Field08
                });
            }

            // Pre-process animations
            Dictionary<int, int> entryIndexMap = new Dictionary<int, int>();
            int animIndex = 0;
            foreach (var entry in scene.AnimationDictionaries)
            {
                Animations.Add(new AnimationGroup(entry.Name)
                {
                    Field00 = scene.AnimationFrameDataList[(int)entry.Index].Field00,
                    Duration = scene.AnimationFrameDataList[(int)entry.Index].FrameCount
                });

                entryIndexMap.Add(animIndex++, (int)entry.Index);
            }

            // process group layers
            List<UILayer> tempLyrs = new List<UILayer>();
            for (int g = 0; g < Groups.Count; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].CastCount; ++c)
                {
                    UILayer layer = new UILayer(scene.UICastGroups[g].Casts[c], GetCastName(g, c, scene.CastDictionaries), c);

                    // sprite
                    if (layer.Type == DrawType.Sprite)
                    {
                        int[] castSprites = scene.UICastGroups[g].Casts[c].CastMaterialData.SubImageIndices;
                        for (int index = 0; index < layer.Sprites.Length; ++index)
                        {
                            layer.Sprites[index] = Utilities.FindSpriteFromNCPScene(castSprites[index], scene.SubImages, texList.Textures);
                        }
                    }
                    else if (layer.Type == DrawType.Font)
                    {
                        foreach (var font in fonts)
                        {
                            if (font.Name == scene.UICastGroups[g].Casts[c].FontName)
                                layer.Font = font;
                        }
                    }

                    tempLyrs.Add(layer);
                }

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
                            AnimationList layerAnimationList = new AnimationList(tempLyrs[c], tracks);
                            Animations[entry.Key].LayerAnimations.Add(layerAnimationList);
                        }
                    }
                }

                // build hierarchy tree
                CreateHierarchyTree(g, scene.UICastGroups[g].CastHierarchyTree, tempLyrs);

                tempLyrs.Clear();
            }
        }

        private void CreateHierarchyTree(int group, List<CastHierarchyTreeNode> tree, List<UILayer> lyrs)
        {
            Groups[group].Layers.Add(lyrs[0]);
            BuildTree(0, tree, lyrs, null);
        }

        private void BuildTree(int c, List<CastHierarchyTreeNode> tree, List<UILayer> lyrs, UILayer parent)
        {
            int childIndex = tree[c].ChildIndex;
            if (childIndex != -1)
            {
                UILayer child = lyrs[childIndex];
                lyrs[c].Children.Add(child);

                BuildTree(childIndex, tree, lyrs, lyrs[c]);
            }

            int siblingIndex = tree[c].NextIndex;
            if (siblingIndex != -1)
            {
                UILayer sibling = lyrs[siblingIndex];
                if (parent != null)
                    parent.Children.Add(sibling);

                BuildTree(siblingIndex, tree, lyrs, parent);
            }
        }

        /// <summary>
        /// Gets the cast name from a cast dictionary provided its index and group index.
        /// If the cast is not found, an empty string is returned.
        /// </summary>
        /// <param name="groupIndex">The index of the group in which the cast belongs</param>
        /// <param name="castIndex">The index of the cast</param>
        /// <param name="castDictionary">A dictionary containing cast names, group indices and cast indices.</param>
        /// <returns></returns>
        public string GetCastName(int groupIndex, int castIndex, List<CastDictionary> castDictionary)
        {
            foreach (var entry in castDictionary)
            {
                if (entry.GroupIndex == groupIndex && entry.CastIndex == castIndex)
                    return entry.Name;
            }

            return String.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(UIScene other)
        {
            return (int)(zIndex - other.zIndex);
        }
    }
}
