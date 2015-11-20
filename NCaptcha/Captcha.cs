//
// Captcha.cs
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
using NCaptcha.Configuration;
using NCaptcha.Utils;

namespace NCaptcha
{
    /// <summary>
    /// CAPTCHA.
    /// </summary>
    public sealed class Captcha
    {
        /// <summary>
        /// Generated key.
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Generated image.
        /// </summary>
        public Image Image { get; private set; }

        Captcha(bool createBuilder, Config config)
        {
            // the createBuilder is false when the object has been created from the builder
            // and true when a public constructor is used instead
            if (createBuilder)
            {
                if (config == null)
                {
                    throw new ArgumentNullException("config", "Configuration object can't be null");
                }

                new Builder(this)
                    .Background(CreateDefaultBackground(config))
                    .Keygen(CreateDefaultKeygen(config))
                    .Drawer(CreateDefaultDrawer(config))
                    .Filters(CreateDefaultFilters(config))
                    .Build();
            }
        }

        /// <summary>
        /// Initializes a new instance using configuration.
        /// </summary>
        public Captcha(Config config)
            : this (createBuilder: true, config: config) { }

        /// <summary>
        /// Parses an object as configuration then initializes a new instance.
        /// </summary>
        /// <param name="obj">Object.</param>
        public Captcha(object config)
            : this (config: Config.Parse(config)) { }

        /// <summary>
        /// Initializes a new instance using default configuration.
        /// </summary>
        public Captcha()
            : this (config: Config.Default) { }

        /// <summary>
        /// CAPTCHA builder. Provides advanced and complex way to construct CAPTCHA.
        /// </summary>
        public sealed class Builder
        {
            Captcha captcha;

            Image background;

            IKeygen keygen;
            IDrawer drawer;

            IFilter[] filters;

            internal Builder(Captcha captcha)
            {
                this.captcha = captcha;
            }

            /// <summary>
            /// Initializes builder.
            /// </summary>
            public Builder()
                : this (new Captcha(createBuilder: false, config: null)) { }

            /// <summary>
            /// Set background image.
            /// </summary>
            public Builder Background(Image background)
            {
                this.background = background;
                return this;
            }

            /// <summary>
            /// Set keygen.
            /// </summary>
            public Builder Keygen(IKeygen keygen)
            {
                this.keygen = keygen;
                return this;
            }

            /// <summary>
            /// Set drawer.
            /// </summary>
            public Builder Drawer(IDrawer drawer)
            {
                this.drawer = drawer;
                return this;
            }

            /// <summary>
            /// Set filters to apply.
            /// </summary>
            public Builder Filters(params IFilter[] filters)
            {
                this.filters = filters;
                return this;
            }

            /// <summary>
            /// Build and return Captcha object.
            /// </summary>
            public Captcha Build()
            {
                if (background == null)
                {
                    throw new NullReferenceException("Background is not specified");
                }

                if (keygen == null)
                {
                    throw new NullReferenceException("Keygen is not specified");
                }

                if (drawer == null)
                {
                    throw new NullReferenceException("Drawer is not specified");
                }

                Image image = (Image) background.Clone();

                captcha.Key = keygen.GenerateKey();
                image = drawer.DrawKey(image, captcha.Key);

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        image = filter.Apply(image);
                    }
                }

                captcha.Image = image;

                return captcha;
            }
        }

        static Image CreateDefaultBackground(Config config)
        {
            var bg = new Bitmap(config.ImageWidth, config.ImageHeight);

            using (Graphics graphics = Graphics.FromImage(bg))
            using (SolidBrush brush = new SolidBrush(config.BackgroundColor))
            {
                graphics.FillRectangle(brush, 0, 0, config.ImageWidth, config.ImageHeight);
            }

            return bg;
        }

        static IKeygen CreateDefaultKeygen(Config config)
        {
            return new Keygen(NumberGenerator.Instance)
            {
                KeyLength = config.KeyLength,
                Alphabet = config.BitmapFont.Alphabet
            };
        }

        static IDrawer CreateDefaultDrawer(Config config)
        {
            return new Drawer(NumberGenerator.Instance)
            {
                Font = config.BitmapFont,
                FontColor = config.ForegroundColor,
                OverlayEnabled = config.OverlayEnabled,
                OverlayPixels = 0
            };
        }

        static IFilter[] CreateDefaultFilters(Config config)
        {
            if (config.WavesFilterEnabled)
            {
                var random = NumberGenerator.Instance;

                // 0..2PI phase
                var xWave = new WavesFilter.Wave(0.15f, 2, random.NextInt(0, 628) / 100f);
                var bigYWave = new WavesFilter.Wave(0.040f, random.NextInt(6, 8), random.NextInt(0, 628) / 100f);
                var smallYWave = new WavesFilter.Wave(0.1f, random.NextInt(2, 4), random.NextInt(0, 628) / 100f);

                return new IFilter[]
                {
                    new WavesFilter(
                        new WavesFilter.Wave[] { xWave },
                        new WavesFilter.Wave[] { bigYWave, smallYWave })
                };
            }
            else
            {
                return new IFilter[0];
            }
        }

    }
}