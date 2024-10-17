
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ESMetadata.Extensions;


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class PathAttribute : Attribute
{
    public PathAttribute() { }
}

namespace ESMetadata.Models.Gamelist
{
    public class GamelistGame
    {
        private class FieldValue
        {
            public GamelistField Field;
            public string Value;
            public FieldValue(GamelistField field, string value = null)
            {
                Field = field;
                Value = value;
            }
        }
        private readonly List<FieldValue> FieldValues = new List<FieldValue>();

        private readonly string root;

        public GamelistGame() { }

        public GamelistGame(XElement node, string root = default)
        {
            this.root = root;
            ReadXml(node);
        }

        static bool IsPath(GamelistField field) => field >= GamelistField.Path;

        public void ReadXml(XElement node)
        {
            foreach(GamelistField field in Enum.GetValues(typeof(GamelistField)).Cast<GamelistField>())
            {
                if (node.Element(field.ToString().ToLower())?.Value is string value)
                {
                    Set(field, IsPath(field) ? AbsPath(value) : value);
                }
            }
        }


        private string AbsPath(string path)
        {
            if (path.IsNullOrEmpty()) return default;
            if (root.IsNullOrEmpty()) return path;

            return System.IO.Path.Combine(root, path).Replace('/','\\').Replace("\\.\\","\\");
        }

        public string Get(GamelistField field, string fallback=default)
        {
            return FieldValues.FirstOrDefault(t => t.Field == field)?.Value ?? fallback;
        }

        public void Set(GamelistField field, string value)
        {
            if ( FieldValues.FirstOrDefault(t => t.Field == field) is FieldValue item )
            {
                item.Value = value;
            }
            else
            {
                FieldValues.Add(new FieldValue(field, value));
            }
        }

    }
}

