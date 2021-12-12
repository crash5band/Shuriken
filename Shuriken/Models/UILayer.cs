using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using XNCPLib.XNCP;
using Shuriken.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Shuriken.Models
{
    public class UILayer : INotifyPropertyChanged
    {
        [Category("Layer")]
        private string name { get; set; }
        public string Name
        { 
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        [Category("Layer")]
        public uint Field00 { get; set; }

        [Category("Layer")]
        public DrawType Type { get; set; }

        [Category("Layer")]
        public bool IsEnabled { get; set; }

        public Vector2 TopLeft { get; set; }

        public Vector2 BottomLeft { get; set; }

        public Vector2 TopRight { get; set; }

        public Vector2 BottomRight { get; set; }

        public ushort Field2C { get; set; }
        public ushort Field2E { get; set; }
        public uint Field34 { get; set; }
        public ushort Field36 { get; set; }
        public uint Flags { get; set; }
        public uint Field3C { get; set; }

        [Category("Font")]
        public UIFont Font { get; set; }

        [Category("Font")]
        public string FontCharacters { get; set; }
        public uint Field4C { get; set; }

        [Category("Dimensions")]
        public uint Width { get; set; }

        [Category("Dimensions")]
        public uint Height { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }
        public Vector2 Offset { get; set; }
        public float Field68 { get; set; }
        public float Field6C { get; set; }
        public uint Field70 { get; set; }
        public int InfoField00 { get; set; }

        private Vector2 translation;
        public Vector2 Translation
        {
            get { return translation; }
            set { translation = value; NotifyPropertyChanged(); }
        }

        private float rotation;
        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; NotifyPropertyChanged(); }
        }

        private Vector2 scale;
        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; NotifyPropertyChanged(); }
        }

        public float InfoField18 { get; set; }

        private Color color;
        public Color Color
        {
            get { return color; }
            set { color = value; NotifyPropertyChanged(); }
        }

        [Category("Color")]
        public Color GradientTopLeft { get; set; }

        [Category("Color")]
        public Color GradientBottomLeft { get; set; }

        [Category("Color")]
        public Color GradientTopRight { get; set; }

        [Category("Color")]
        public Color GradientBottomRight { get; set; }

        public uint InfoField30 { get; set; }
        public uint InfoField34 { get; set; }
        public uint InfoField38 { get; set; }

        [Category("Sprite")]
        public Sprite[] Sprites { get; set; }

        [Browsable(false)]
        private bool visible;
        public bool Visible
        {
            get => visible;
            set { visible = value; NotifyPropertyChanged(); }
        }

        public UILayer Parent { get; set; }

        [Browsable(false)]
        public ObservableCollection<UILayer> Children { get; set; }

        public UILayer(UICast cast, string name)
        {
            Name = name;
            Field00 = cast.Field00;
            Type = (DrawType)cast.Field04;
            IsEnabled = cast.IsEnabled != 0;
            Visible = true;
            Children = new ObservableCollection<UILayer>();

            TopLeft = new Vector2(cast.TopLeft);
            BottomLeft = new Vector2(cast.BottomLeft);
            TopRight = new Vector2(cast.TopRight);
            BottomRight = new Vector2(cast.BottomRight);

            Field2C = cast.Field2C;
            Field2E = cast.Field2E;
            Field34 = cast.Field34;
            Flags = cast.Field38;
            Field3C = cast.Field3C;

            Font = null;
            FontCharacters = cast.FontCharactersOffset.Offset != 0 ? cast.FontCharactersOffset.Value : "";

            Field4C = cast.Field4C;
            Width = cast.Width;
            Height = cast.Height;
            Field58 = cast.Field58;
            Field5C = cast.Field5C;

            Offset = new Vector2(cast.Offset);

            Field68 = cast.Field68;
            Field6C = cast.Field6C;
            Field70 = cast.Field70;

            InfoField00 = cast.CastInfoData.Field00;
            Translation = new Vector2(cast.CastInfoData.Translation);
            Rotation = cast.CastInfoData.Rotation;
            Scale = new Vector2(cast.CastInfoData.Scale);
            InfoField18 = cast.CastInfoData.Field18;
            Color = new Color(cast.CastInfoData.Color);
            GradientTopLeft = new Color(cast.CastInfoData.GradientTopLeft);
            GradientBottomLeft = new Color(cast.CastInfoData.GradientBottomLeft);
            GradientTopRight = new Color(cast.CastInfoData.GradientTopRight);
            GradientBottomRight = new Color(cast.CastInfoData.GradientBottomRight);
            InfoField30 = cast.CastInfoData.Field30;
            InfoField34 = cast.CastInfoData.Field34;
            InfoField38 = cast.CastInfoData.Field38;

            Sprites = new Sprite[32];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
