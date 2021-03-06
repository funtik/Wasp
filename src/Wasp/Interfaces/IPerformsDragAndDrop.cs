﻿using OpenQA.Selenium;

namespace Wasp.Interfaces
{
    public interface IPerformsDragAndDrop
    {
        void DragAndDrop(IWebElement drag, IWebElement drop);
        void DragAndDrop(IWebElement drag, int xDrop, int yDrop);
    }
}
