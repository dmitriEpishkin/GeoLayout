﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GeoLayout.Effects {

    public class ColorToneEffect : ShaderEffect {

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ColorToneEffect), 0);
        public static readonly DependencyProperty DesaturationProperty = DependencyProperty.Register("Desaturation", typeof(double), typeof(ColorToneEffect), new UIPropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty TonedProperty = DependencyProperty.Register("Toned", typeof(double), typeof(ColorToneEffect), new UIPropertyMetadata(((double)(0.5D)), PixelShaderConstantCallback(1)));
        public static readonly DependencyProperty LightColorProperty = DependencyProperty.Register("LightColor", typeof(Color), typeof(ColorToneEffect), new UIPropertyMetadata(Color.FromArgb(255, 255, 255, 0), PixelShaderConstantCallback(2)));
        public static readonly DependencyProperty DarkColorProperty = DependencyProperty.Register("DarkColor", typeof(Color), typeof(ColorToneEffect), new UIPropertyMetadata(Color.FromArgb(255, 0, 0, 128), PixelShaderConstantCallback(3)));
        public ColorToneEffect() {
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/GeoLayout;component/Effects/ColorTone.ps", UriKind.Relative);
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(DesaturationProperty);
            this.UpdateShaderValue(TonedProperty);
            this.UpdateShaderValue(LightColorProperty);
            this.UpdateShaderValue(DarkColorProperty);
        }
        public Brush Input {
            get {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set {
                this.SetValue(InputProperty, value);
            }
        }
        /// <summary>The amount of desaturation to apply.</summary>
        public double Desaturation {
            get {
                return ((double)(this.GetValue(DesaturationProperty)));
            }
            set {
                this.SetValue(DesaturationProperty, value);
            }
        }
        /// <summary>The amount of color toning to apply.</summary>
        public double Toned {
            get {
                return ((double)(this.GetValue(TonedProperty)));
            }
            set {
                this.SetValue(TonedProperty, value);
            }
        }
        /// <summary>The first color to apply to input. This is usually a light tone.</summary>
        public Color LightColor {
            get {
                return ((Color)(this.GetValue(LightColorProperty)));
            }
            set {
                this.SetValue(LightColorProperty, value);
            }
        }
        /// <summary>The second color to apply to the input. This is usuall a dark tone.</summary>
        public Color DarkColor {
            get {
                return ((Color)(this.GetValue(DarkColorProperty)));
            }
            set {
                this.SetValue(DarkColorProperty, value);
            }
        }

    }
}
