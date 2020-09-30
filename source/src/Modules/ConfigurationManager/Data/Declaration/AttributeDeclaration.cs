using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Testflow.Data.Attributes;

namespace Testflow.ConfigurationManager.Data.Declaration
{
    [Serializable]
    public class AttributeDeclaration : IAttributeDeclaration
    {
        [XmlIgnore]
        public string FullName => GetTypeFullName(Target, Type);

        [XmlIgnore]
        public string Target { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlIgnore]
        public IList<IAttributeArgument> Arguments { get; internal set; }

        [XmlArray]
        [XmlText(DataType = "Arguments")]
        [XmlArrayItem(ElementName = "Argument")]
        public List<AttributeArgument> ArgumentDefinitions
        {
            get { return null;}
            set
            {
                Arguments = new List<IAttributeArgument>(value.Count);
                foreach (AttributeArgument argument in value)
                {
                    Arguments.Add(argument);
                }
            }
        }

        internal static string GetTypeFullName(string target, string type)
        {
            return $"{target}.{type}";
        }
    }
}