using System;

namespace controller
{
    internal class ActionDescAttribute : Attribute
    {
        public string Description { get; set; }
    }
}