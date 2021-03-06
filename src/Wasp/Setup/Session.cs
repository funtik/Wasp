﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenQA.Selenium;

using Wasp.Extensions;
using Wasp.Interfaces;

namespace Wasp.Setup
{
    public class Session
    {
        public virtual ISettings Settings { get; private set; }

        public virtual IWebDriver Driver { get; private set; }

        public virtual IMonkey Monkey { get; protected set; }

        public bool IsMobile { get; set; }

        public Session(IDriverEnvironment environment) : this(environment, new Settings())
        {
        }

        public Session(IDriverEnvironment environment, ISettings settings)
        {
            this.Settings = settings;
            this.Driver = environment.CreateWebDriver();
            this.IsMobile = false;
        }

        public virtual TBlock NavigateTo<TBlock>(string url) where TBlock : IBlock
        {
            this.Driver.Navigate().GoToUrl(url);
            return this.CurrentBlock<TBlock>();
        }

        public virtual TBlock CurrentBlock<TBlock>(IWebElement tag = null) where TBlock : IBlock
        {
            var type = typeof(TBlock);
            IList<Type> constructorSignature = new List<Type> { typeof(Session) };
            IList<object> constructorArgs = new List<object> { this };

            if (typeof(ISpecificBlock).IsAssignableFrom(typeof(TBlock)))
            {
                constructorSignature.Add(typeof(IWebElement));
                constructorArgs.Add(tag);
            }

            var constructor = type.GetConstructor(constructorSignature.ToArray());

            if (constructor == null)
            {
                throw new ArgumentException(String.Format(
                    "The result type specified ({0}) is not a valid block. It must have a constructor that takes only a session.", type));
            }

            return (TBlock)constructor.Invoke(constructorArgs.ToArray());
        }

        public virtual void End()
        {
            if (this.Driver != null)
            {
                this.Driver.Quit();

                this.Driver.Dispose();

                this.Driver = null;
            }
        }

        public virtual Session CaptureScreen()
        {
            var filename = String.Format("{0}.png", CallStack.GetCallingMethod().GetFullName());
            var path = Path.Combine(this.Settings.ScreenCapturePath, filename);
            return this.CaptureScreen(path);
        }

        public virtual Session CaptureScreen(string path)
        {
            var screenshot = ((ITakesScreenshot)this.Driver).GetScreenshot();

            var extension = Path.GetExtension(path);

            if (String.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
            {
                screenshot.SaveAsFile(path, ScreenshotImageFormat.Png);
            }
            else if ((String.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                     || (String.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)))
            {
                screenshot.SaveAsFile(path, ScreenshotImageFormat.Jpeg);
            }
            else if (String.Equals(extension, ".bmp", StringComparison.OrdinalIgnoreCase))
            {
                screenshot.SaveAsFile(path, ScreenshotImageFormat.Bmp);
            }
            else if (String.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase))
            {
                screenshot.SaveAsFile(path, ScreenshotImageFormat.Gif);
            }
            else
            {
                throw new ArgumentException("Unable to determine image format. The supported formats are BMP, GIF, JPEG and PNG.", "path");
            }

            return this;
        }

        public virtual T ExecuteJavaScript<T>(string script, params object[] args)
        {
            return this.Driver.ExecuteScript<T>(script, args);
        }
    }

    public class Session<TDriverEnvironment> : Session
        where TDriverEnvironment : IDriverEnvironment, new()
    {
        public Session() : base(new TDriverEnvironment())
        {
        }

        public Session(ISettings settings) : base(new TDriverEnvironment(), settings)
        {
        }
    }
}
