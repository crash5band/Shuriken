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
using System.Reflection;

namespace Shuriken.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private RelayCommand createSpriteCmd;
        public RelayCommand CreateSpriteCmd
        {
            get => createSpriteCmd ?? new RelayCommand(CreateSprite, () => Scenes.Count > 0);
            set
            {
                createSpriteCmd = value;
                NotifyPropertyChanged();
            }
        }

        public ScenesManagerViewModel SceneManager { get; set; }
        public ObservableCollection<UIScene> Scenes { get; set; }
        public ObservableCollection<Texture> Textures { get; set; }
        public ObservableCollection<UIFont> UIFonts { get; set; }
        public ObservableCollection<string> MissingTextures { get; set; }

        public MainViewModel()
        {
            Scenes = new ObservableCollection<UIScene>();
            Textures = new ObservableCollection<Texture>();
            UIFonts = new ObservableCollection<UIFont>();
            MissingTextures = new ObservableCollection<string>();

            SceneManager = new ScenesManagerViewModel();
#if DEBUG
            //LoadTestXNCP();
#endif
        }

        public void LoadTestXNCP()
        {
            Load("Test/ui_gameplay.xncp");
        }

        public void CreateSprite()
        {
            SpriteViewModel sp = new SpriteViewModel(new Sprite());
            Scenes[0].Sprites.Add(sp);
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

            List<Scene> xScenes = file.Resources[0].Content.CsdmProject.Root.Scenes;
            List<SceneID> xIDs = file.Resources[0].Content.CsdmProject.Root.SceneIDTable;
            List<XTexture> xTextures = file.Resources[1].Content.TextureList.Textures;
            FontList xFontList = file.Resources[0].Content.CsdmProject.Fonts;

            Clear();

            foreach (XTexture texture in xTextures)
            {
                string texPath = Path.Combine(root, texture.Name.Value);
                if (File.Exists(texPath))
                    Textures.Add(new Texture(texPath));
                else
                    MissingTextures.Add(texture.Name.Value);
            }

            foreach (SceneID sceneID in xIDs)
                Scenes.Add(new UIScene(xScenes[(int)sceneID.Index], sceneID.Name.Value, xScenes[(int)sceneID.Index].CastDictionaries, Textures, UIFonts));

            foreach (var entry in xFontList.FontIDTable)
            {
                UIFont font = new UIFont(entry.Name.Value);
                foreach (var mapping in xFontList.Fonts[(int)entry.Index].CharacterMappings)
                    font.Mappings.Add(new Models.CharacterMapping(mapping.SourceCharacter, (int)mapping.SubImageIndex));

                UIFonts.Add(font);
            }

            if (MissingTextures.Count > 0)
                WarnMissingTextures();
        }

        public void Clear()
        {
            Scenes.Clear();
            Textures.Clear();
            UIFonts.Clear();
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
