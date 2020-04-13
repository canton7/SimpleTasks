using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTasks
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public OptionAttribute() { }
        public OptionAttribute(string name) => this.Name = name;
        public OptionAttribute(string name, string description) => (this.Name, this.Description) = (name, description);
    }
}
