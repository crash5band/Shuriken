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

        private UIProject selectedProject;
        public UIProject SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                NotifyPropertyChanged();
            }
        }

        private UIScene selectedScene;
        public UIScene SelectedScene
        {
            get => selectedScene;
            set
            {
                selectedScene = value;
                NotifyPropertyChanged();
            }
        }

        private TextureList selectedTexList;
        public TextureList SelectedTexList
        {
            get => selectedTexList;
            set
            {
                selectedTexList = value;
                NotifyPropertyChanged();
            }
        }

        private Texture selectedTexture;
        public Texture SelectedTexture
        {
            get => selectedTexture;
            set
            {
                selectedTexture = value;
                NotifyPropertyChanged();
            }
        }

        public SceneViewer Viewer { get; set; }
        public ObservableCollection<UIProject> Projects { get; set; }
        public ObservableCollection<string> MissingTextures { get; set; }

        public MainViewModel()
        {
            Viewer = new SceneViewer();
            Projects = new ObservableCollection<UIProject>();
            MissingTextures = new ObservableCollection<string>();
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
            FAPCFile file = new FAPCFile();
            file.Load(filename);

            string root = Path.GetDirectoryName(Path.GetFullPath(filename));

            // TODO: load all project nodes
            UIProject project = new UIProject(file.Resources[0].Content.CsdmProject.ProjectName.Value);

            List<Scene> xScenes = file.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = file.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = file.Resources[1].Content.TextureList.Textures;
            FontList xFontList = file.Resources[0].Content.CsdmProject.Fonts;

            Clear();

            TextureList texList = new TextureList("textures");
            foreach (XTexture texture in xTextures)
            {
                string texPath = Path.Combine(root, texture.Name.Value);
                if (File.Exists(texPath))
                    texList.Textures.Add(new Texture(texPath));
                else
                    MissingTextures.Add(texture.Name.Value);
            }

            if (xScenes.Count > 0)
            {
                // Hack: we load sprites from the first scene only since whatever tool sonic team uses
                // seems to work the same way as SWIF:
                // Sprites belong to textures and layers and fonts reference a specific sprite using the texutre index and sprite index.
                foreach (SubImage subimage in xScenes[0].SubImages)
                {
                    int textureIndex = (int)subimage.TextureIndex;
                    if (textureIndex >= 0 && textureIndex < texList.Textures.Count)
                    {
                        Sprite spr = new Sprite(texList.Textures[textureIndex], subimage.TopLeft.Y, subimage.TopLeft.X,
                            subimage.BottomRight.Y, subimage.BottomRight.X);

                        texList.Textures[textureIndex].Sprites.Add(new SpriteViewModel(spr));
                    }
                }
            }

            foreach (SceneID sceneID in xIDs)
                project.Scenes.Add(new UIScene(xScenes[(int)sceneID.Index], sceneID.Name.Value, texList, project.Fonts));

            foreach (var entry in xFontList.FontIDTable)
            {
                UIFont font = new UIFont(entry.Name.Value);
                foreach (var mapping in xFontList.Fonts[(int)entry.Index].CharacterMappings)
                {
                    var sprite = Utilities.FindSpriteFromNCPScene((int)mapping.SubImageIndex, xScenes[0].SubImages, texList.Textures);
                    font.Mappings.Add(new Models.CharacterMapping(mapping.SourceCharacter, sprite));
                }

                project.Fonts.Add(font);
            }

            project.TextureLists.Add(texList);
            Projects.Add(project);

            if (MissingTextures.Count > 0)
                WarnMissingTextures();

            if (Projects.Count > 0)
            {
                SelectedProject = Projects[0];
            }
        }

        public void Clear()
        {
            Projects.Clear();
            MissingTextures.Clear();
        }

        private void WarnMissingTextures()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("The loaded UI file uses textures that were not found.\n");
            foreach (var texture in MissingTextures)
                builder.AppendLine(texture);

            HandyControl.Controls.MessageBox.Show(builder.ToString(), "Missing Textures", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
