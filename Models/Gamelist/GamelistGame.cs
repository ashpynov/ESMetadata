
using System;
using System.Linq;
using System.Reflection;
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
        private readonly string root;

        public GamelistGame() { }

        public GamelistGame(XElement node, string root = default)
        {
            this.root = root;
            ReadXml(node);
        }
        public GamelistGame Clone(GamelistGame from)
        {
            foreach(PropertyInfo prop in typeof(GamelistGame).GetProperties())
            {
                prop.SetValue(this, prop.GetValue(from));
            }
            return this;
        }

        public GamelistGame Extend(GamelistGame from)
        {
            foreach(PropertyInfo prop in typeof(GamelistGame).GetProperties().Where(p=>(p.GetValue(this) as string).IsNullOrEmpty()))
            {
                prop.SetValue(this, prop.GetValue(from));
            }
            return this;
        }


        [Path]
        public string Path { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        [Path]
        public string Video { get; set; }
        [Path]
        public string Image { get; set; }
        [Path]
        public string Marquee { get; set; }
        [Path]
        public string Thumbnail { get; set; }
        [Path]
        public string Bezel { get; set; }
        [Path]
        public string Fanart { get; set; }
        [Path]
        public string Manual { get; set; }
        [Path]
        public string Boxback { get; set; }
        public string Rating { get; set; }
        public string ReleaseDate { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Genre { get; set; }
        public string Region { get; set; }
        public string Favorite { get; set; }

        public XmlSchema GetSchema() => null;
        public void WriteXml(XmlWriter writer) { }

        public void ReadXml(XElement node)
        {
            foreach (PropertyInfo prop in typeof(GamelistGame).GetProperties())
            {
                var element = node.Element(prop.Name.ToLower());
                if (element != null)
                {
                    if (prop.GetCustomAttribute<PathAttribute>() != null)
                    {
                        prop.SetValue(this, AbsPath(element.Value));
                    }
                    else
                    {
                        prop.SetValue(this, element.Value);
                    }
                }
            }
        }


        private string AbsPath(string path)
        {
            if (path.IsNullOrEmpty()) return default;
            if (root.IsNullOrEmpty()) return path;

            return System.IO.Path.Combine(root, path).Replace('/','\\').Replace("\\.\\","\\");
        }

        public string Get(string property)
        {
            return typeof(GamelistGame).GetProperty(property)?.GetValue(this) as string;
        }
    }
}

