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
    public class UIScene : INotifyPropertyChanged
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

        [Category("Data1")]
        public ObservableCollection<Vector2> TextureSizes { get; set; }

        [Browsable(false)]
        public ObservableCollection<LayerGroup> Groups { get; set; }

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
            TextureSizes = new ObservableCollection<Vector2>();
            Animations = new ObservableCollection<AnimationGroup>();
            Groups = new ObservableCollection<LayerGroup>();

            foreach (var texSize in scene.Data1)
            {
                TextureSizes.Add(new Vector2(texSize.X, texSize.Y));
            }

            ProcessCasts(scene, texList);
            //CreateGroups(scene);
            //CreateSprites(scene);
            //CreateLayers(scene, scene.CastDictionaries, texList);
            //CreateAnimations(scene);
            //CreateHierarchyTree(scene);

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

        private void CreateGroups(Scene scene)
        {
            for (int i = 0; i < scene.GroupCount; ++i)
            {
                Groups.Add(new LayerGroup
                {
                    Name = "Group_" + i,
                    Field08 = scene.UICastGroups[i].Field08
                });
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

        private void ProcessCasts(Scene scene, TextureList texList)
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

            // process group layers
            List<UILayer> tempLyrs = new List<UILayer>();
            for (int g = 0; g < Groups.Count; ++g)
            {
                for (int c = 0; c < scene.UICastGroups[g].CastCount; ++c)
                {
                    int[] castSprites = scene.UICastGroups[g].Casts[c].CastMaterialData.SubImageIndices;
                    UILayer layer = new UILayer(scene.UICastGroups[g].Casts[c], GetLayerName(g, c, scene.CastDictionaries));

                    for (int index = 0; index < layer.Sprites.Length; ++index)
                    {
                        layer.Sprites[index] = Utilities.FindSpriteFromNCPScene(castSprites[index], scene.SubImages, texList.Textures);
                    }

                    tempLyrs.Add(layer);
                }

                // build hierarchy tree
                CreateHierarchyTree(g, scene.UICastGroups[g].CastHierarchyTree, tempLyrs);

                tempLyrs.Clear();
            }
        }

        private void CreateHierarchyTree(int group, List<CastHierarchyTreeNode> tree, List<UILayer> tempLyrs)
        {
            Groups[group].Layers.Add(tempLyrs[0]);
            BuildTree(0, tree, tempLyrs, null);
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
