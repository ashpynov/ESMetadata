
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class PathAttribute : Attribute
{
    public PathAttribute() { }
}


namespace ESMetadata.Models
{
    public class ESGame
    {
        private readonly string root;

        public ESGame() { }

        public ESGame(XElement node, string root=default)
        {
            this.root = root;
            ReadXml(node);
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
            foreach (PropertyInfo prop in typeof(ESGame).GetProperties())
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
            if (string.IsNullOrEmpty(path)) return default;
            if (string.IsNullOrEmpty(root)) return path;

            return System.IO.Path.Combine(root, path).Replace('/','\\').Replace("\\.\\","\\");
        }

        public string Get(string property)
        {
            return typeof(ESGame).GetProperty(property)?.GetValue(this) as string;
        }
    }
}

