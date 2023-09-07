using Backend.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Modules.Native
{
    public class NativeItem
    {
        public string Label { get; set; }

        [JsonIgnore]
        public Action<RXPlayer> Action { get; set; } = player => { };

        public NativeItem(string label, Action<RXPlayer> action)
        {
            Label = label;
            Action = action;
        }

        public NativeItem(string label)
        {
            Label = label;
        }
    }

    public class NativeMenu
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<NativeItem> Items { get; set; }

        public NativeMenu(string title, string description, List<NativeItem> items)
        {
            Title = title;
            Description = description;
            Items = items;
        }
    }
}
