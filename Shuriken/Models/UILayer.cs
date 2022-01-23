﻿using System;
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
        private string name { get; set; }
        public string Name
        { 
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        public uint Field00 { get; set; }

        private DrawType type;
        public DrawType Type
        {
            get { return type; }
            set { type = value; NotifyPropertyChanged(); }
        }

        private bool enabled;
        public bool IsEnabled
        {
            get { return enabled; }
            set { enabled = value; NotifyPropertyChanged(); }
        }

        public Vector2 TopLeft { get; set; }

        public Vector2 BottomLeft { get; set; }

        public Vector2 TopRight { get; set; }

        public Vector2 BottomRight { get; set; }

        public uint Field2C { get; set; }

        private uint field34;
        public uint Field34
        {
            get { return field34; }
            set { field34 = value; NotifyPropertyChanged();}
        }

        private uint flags;
        public uint Flags
        {
            get { return flags; }
            set { flags = value; NotifyPropertyChanged(); }
        }

        public uint Field3C { get; set; }

        public UIFont Font { get; set; }

        private string fontChars;
        public string FontCharacters
        {
            get { return fontChars; }
            set { fontChars = value; NotifyPropertyChanged(); }
        }
        public uint Field4C { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }

        private Vector2 offset;
        public Vector2 Offset
        {
            get { return offset; }
            set { offset = value; NotifyPropertyChanged(); }
        }
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

        private Color gradientTopLeft;
        public Color GradientTopLeft
        {
            get { return gradientTopLeft; }
            set { gradientTopLeft = value; NotifyPropertyChanged();}
        }

        private Color gradientBottomLeft;
        public Color GradientBottomLeft
        {
            get { return gradientBottomLeft; }
            set { gradientBottomLeft = value; NotifyPropertyChanged(); }
        }
        
        private Color gradientTopRight;
        public Color GradientTopRight
        {
            get { return gradientTopRight; }
            set { gradientTopRight = value; NotifyPropertyChanged(); }
        }

        private Color gradientBottomRight;
        public Color GradientBottomRight
        {
            get { return gradientBottomRight; }
            set { gradientBottomRight = value; NotifyPropertyChanged(); }
        }

        public uint InfoField30 { get; set; }
        public uint InfoField34 { get; set; }
        public uint InfoField38 { get; set; }

        [Category("Sprite")]
        public ObservableCollection<Sprite> Sprites { get; set; }

        private bool visible;
        public bool Visible
        {
            get => visible;
            set { visible = value; NotifyPropertyChanged(); }
        }

        private int zIndex;
        public int ZIndex
        {
            get => zIndex;
            set { zIndex = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<UILayer> Children { get; set; }

        public UILayer(UICast cast, string name, int index)
        {
            Name = name;
            Field00 = cast.Field00;
            Type = (DrawType)cast.Field04;
            IsEnabled = cast.IsEnabled != 0;
            Visible = true;
            ZIndex = index;
            Children = new ObservableCollection<UILayer>();

            TopLeft = new Vector2(cast.TopLeft);
            BottomLeft = new Vector2(cast.BottomLeft);
            TopRight = new Vector2(cast.TopRight);
            BottomRight = new Vector2(cast.BottomRight);

            Field2C = cast.Field2C;
            Field34 = cast.Field34;
            Flags = cast.Field38;
            Field3C = cast.Field3C;

            Font = null;
            FontCharacters = cast.FontCharacters;

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

            Sprites = new ObservableCollection<Sprite>();
            for (int i = 0; i < 32; ++i)
                Sprites.Add(new Sprite());
        }

        public UILayer()
        {
            Name = "Cast";
            Field00 = 0;
            Type = DrawType.Sprite;
            IsEnabled = true;
            Visible = true;
            ZIndex = 0;
            Children = new ObservableCollection<UILayer>();

            TopLeft = new Vector2();
            BottomLeft = new Vector2();
            TopRight = new Vector2();
            BottomRight = new Vector2();

            Field2C = 0;
            Field34 = 0;
            Flags = 0;
            Field3C = 0;

            Font = null;
            FontCharacters = "";

            Field4C = 0;
            Width = 0;
            Height = 0;
            Field58 = 0;
            Field5C = 0;

            Offset = new Vector2();

            Field68 = 0;
            Field6C = 0;
            Field70 = 0;

            InfoField00 = 0;
            Translation = new Vector2();
            Rotation = 0;
            Scale = new Vector2(1.0f, 1.0f);
            InfoField18 = 0;
            Color = new Color(255, 255, 255, 255);
            GradientTopLeft = new Color(255, 255, 255, 255);
            GradientBottomLeft = new Color(255, 255, 255, 255);
            GradientTopRight = new Color(255, 255, 255, 255);
            GradientBottomRight = new Color(255, 255, 255, 255);
            InfoField30 = 0;
            InfoField34 = 0;
            InfoField38 = 0;

            Sprites = new ObservableCollection<Sprite>();
            for (int i = 0; i < 32; ++i)
                Sprites.Add(new Sprite());
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
