using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RXCommand : Attribute
    {
        public string Name { get; set; }
        public int Permission { get; set; }

        public RXCommand(string command, int min_permission = 0)
        {
            this.Name = command;
            this.Permission = min_permission;
        }
    }
}
