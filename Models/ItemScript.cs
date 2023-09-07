using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    [AttributeUsage(AttributeTargets.Method)]
    class ItemScript : Attribute
    {
        public string Script { get; set; }

        public ItemScript(string script)
        {
            this.Script = script;
        }
    }
}
