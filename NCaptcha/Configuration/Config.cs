//
// Config.cs
//
// Author:
//       Sergey Khabibullin <sergey@khabibullin.com>
//
// Copyright (c) 2014 Sergey Khabibullin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using NCaptcha.Utils;

namespace NCaptcha.Configuration
{
    public class Config
    {
        int imageWidth = 100;
        public int ImageWidth { get; protected set; }

        int imageHeight = 50;
        public int ImageHeight { get; protected set; }

        int? keyLength;
        public int KeyLength { get; protected set; }

        bool overlayEnabled = false;
        public bool OverlayEnabled { get; protected set; }

        bool wavesFilterEnabled = false;
        public bool WavesFilterEnabled { get; protected set; }

        Color foregroundColor = Color.Empty;
        public Color ForegroundColor { get; protected set; }

        Color backgroundColor = Color.Empty;
        public Color BackgroundColor { get; protected set; }

        string fontPath = null;
        public BitmapFont BitmapFont { get; protected set; }

        readonly INumberGenerator random;

        protected Config(INumberGenerator random)
        {
            this.random = random;
        }

        protected Config() : this(NumberGenerator.Instance)
        {
        }

        public static Config Default
        {
            get
            {
                var config = new Config();
                config.InitValues();
                return config;
            }
        }

        public static Config Parse(object obj)
        {
            var config = new Config();

            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Can't parse null object");
            }

            PropertyInfo[] props = obj.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                object val = prop.GetValue(obj, null);
                string type = prop.PropertyType.FullName;
                string name = prop.Name.ToLower();

                switch (name)
                {
                    case "keylength":
                        config.keyLength = ParseInt(name, type, val);
                        break;

                    case "width":
                        config.imageWidth = ParseInt(name, type, val);
                        break;

                    case "height":
                        config.imageHeight = ParseInt(name, type, val);
                        break;

                    case "foreground":
                        config.foregroundColor = ParseColor(name, type, val);
                        break;

                    case "background":
                        config.backgroundColor = ParseColor(name, type, val);
                        break;

                    case "overlay":
                        config.overlayEnabled = ParseBool(name, type, val);
                        break;

                    case "waves":
                        config.wavesFilterEnabled = ParseBool(name, type, val);
                        break;

                    case "font":
                        config.fontPath = ParseString(name, type, val);
                        break;
                }
            }

            config.InitValues();
            return config;
        }

        public void InitValues()
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            KeyLength = keyLength ?? random.NextInt(5, 7);
            OverlayEnabled = overlayEnabled;
            WavesFilterEnabled = wavesFilterEnabled;

            if (foregroundColor != Color.Empty)
            {
                ForegroundColor = foregroundColor;

                // if only foreground color configured
                if (backgroundColor == Color.Empty)
                {
                    BackgroundColor = GetDerivedColor(foregroundColor);
                }
                // else both colors specifed; do nothing
            }
            else if (backgroundColor != Color.Empty) // if only background is specifed
            {
                ForegroundColor = GetDerivedColor(backgroundColor);
                BackgroundColor = backgroundColor;
            }
            else
            {
                ForegroundColor = Color.FromArgb(
                    random.NextInt(0x20, 0x40),
                    random.NextInt(0x20, 0x40),
                    random.NextInt(0x20, 0x40));

                BackgroundColor = GetDerivedColor(ForegroundColor);
            }

            using (var stream = GetBitmapFontStream(fontPath))
            {
                BitmapFont = BitmapFont.FromStream(stream);
            }
        }

        Color GetDerivedColor(Color baseColor)
        {
            int r = random.NextInt(0x80, 0xA0),
                g = random.NextInt(0x80, 0xA0),
                b = random.NextInt(0x80, 0xA0);

            if (baseColor.GetBrightness() > 0.5)
            {
                // reverse
                r = -r; g = -g; b = -b;
            }

            // get derived rgb
            r += baseColor.R;
            g += baseColor.G;
            b += baseColor.B;

            // rgb normalization
            return Color.FromArgb(
                r > 255 ? 255 : r > 0 ? r : 0,
                g > 255 ? 255 : g > 0 ? g : 0,
                b > 255 ? 255 : b > 0 ? b : 0);
        }

        Stream GetBitmapFontStream(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                if (Directory.Exists("Fonts")) // retrieve fonts from the default directory
                {
                    string[] files = Directory.GetFiles("Fonts", "*.font.png");
                    if (files.Length > 0)
                    {
                        return File.OpenRead(files[random.NextInt(0, files.Length)]);
                    }
                }

                // retrieve the embedded fonts
                Assembly assmebly = Assembly.GetExecutingAssembly();
                string[] fonts = assmebly.GetManifestResourceNames();

                return assmebly.GetManifestResourceStream(fonts[random.NextInt(0, fonts.Length)]);
            }
            else if (Directory.Exists(path)) // retrieve fonts from the specified directory
            {
                string[] files = Directory.GetFiles(path, "*.font.png");
                if (files.Length > 0)
                {
                    return File.OpenRead(files[random.NextInt(0, files.Length)]);
                }

                throw new FileNotFoundException("The specified directory doesn't contain font files", path);
            }
            else if (File.Exists(path)) // retrieve a font from the specified file
            {
                return File.OpenRead(path);
            }

            throw new FileNotFoundException("Can't find the specified file or directory", path);
        }

        static bool ParseBool(string name, string type, object val)
        {
            switch (type)
            {
                case "System.Boolean":
                    return (bool) val;
                default:
                    throw new Config.Exception(name, "Incorrect value type");
            }
        }

        static int ParseInt(string name, string type, object val)
        {
            switch (type)
            {
                // only integers
                case "System.Int16":
                case "System.Int32":
                    return (int) val;
                default:
                    throw new Config.Exception(name, "Incorrect value type");
            }
        }

        static string ParseString(string name, string type, object val)
        {
            switch (type)
            {
                case "System.String":
                    return (string) val;
                default:
                    throw new Config.Exception(name, "Non-string filename for the font");
            }
        }

        static Color ParseColor(string name, string type, object val)
        {
            switch (type)
            {
                case "System.Drawing.Color":
                    return (Color) val;
                case "System.String": // html color string #RRGGBB
                    try
                    {
                        return ColorTranslator.FromHtml((string) val);
                    }
                    catch
                    {
                        throw new Config.Exception(name, "Incorrect html color value");
                    }
                default:
                    throw new Config.Exception(name, "Incorrect value type");
            }
        }

        public class Exception : System.Exception
        {
            public readonly string Parameter;

            public Exception (string parameter, string message) : base (message)
            {
                Parameter = parameter;
            }
        }
    }
}

