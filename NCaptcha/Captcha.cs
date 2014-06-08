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
using NCaptcha.Utils.Factories;

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
                    .Background(BackgroundFactory.Create(config))
                    .Keygen(KeygenFactory.Create(config))
                    .Drawer(DrawerFactory.Create(config))
                    .Filters(FilterFactory.Create(config))
                    .Build();
            }
        }

        /// <summary>
        /// Initializes a new instance using configuration.
        /// </summary>
        public Captcha(Config config)
            : this (true, config) { }

        /// <summary>
        /// Parses an object as configuration then initializes a new instance.
        /// </summary>
        /// <param name="obj">Object.</param>
        public Captcha(object obj)
            : this (Config.Parse(obj)) { }

        /// <summary>
        /// Initializes a new instance using default configuration.
        /// </summary>
        public Captcha()
            : this (Config.Default) { }

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
                : this (new Captcha(false, null)) { }

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
    }
}