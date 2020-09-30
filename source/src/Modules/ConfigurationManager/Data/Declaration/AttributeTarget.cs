using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Testflow.ConfigurationManager.Data.Declaration
{
    [Serializable]
    public class AttributeTarget
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Description { get; set; }

        [XmlArray]
        [XmlArrayItem(ElementName = "Attribute")]
        public List<AttributeDeclaration> Attributes { get; set; }
    }
}