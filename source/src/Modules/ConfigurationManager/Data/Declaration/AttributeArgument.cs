using System;
using System.Xml.Serialization;
using Testflow.Data.Attributes;

namespace Testflow.ConfigurationManager.Data.Declaration
{
    [Serializable]
    public class AttributeArgument : IAttributeArgument
    {
        [XmlAttribute]
        public string ArgumentName { get; set; }
        [XmlAttribute]
        public int ArgumentIndex { get; set; }
        [XmlAttribute]
        public AttributeArgumentType Type { get; set; }
        [XmlAttribute]
        public string ExtraInfo { get; set; }
    }
}