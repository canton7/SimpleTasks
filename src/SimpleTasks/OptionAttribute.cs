using System;

namespace SimpleTasks
{
    /// <summary>
    /// Attribute which can be added to method parameters, to give a customised name and description
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the option. If <c>null</c>, the name of the parameter is used
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description for the option, if anys
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="OptionAttribute"/> class
        /// </summary>
        public OptionAttribute() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="OptionAttribute"/> class with the given name
        /// </summary>
        /// <param name="name">Name of the option</param>
        public OptionAttribute(string name) => this.Name = name;

        /// <summary>
        /// Initialises a new instance of the <see cref="OptionAttribute"/> class with the given name
        /// </summary>
        /// <param name="name">Name of the option</param>
        /// <param name="description">Description for the option</param>
        public OptionAttribute(string name, string description) => (this.Name, this.Description) = (name, description);
    }
}
