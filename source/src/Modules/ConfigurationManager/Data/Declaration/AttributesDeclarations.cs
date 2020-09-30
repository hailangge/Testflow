using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Testflow.ConfigurationManager.Data.Declaration
{
    [Serializable]
    public class AttributesDeclarations
    {
        [XmlAttribute]
        public string AttributeVersion { get; set; }

        [XmlArray]
        [XmlArrayItem(ElementName = "AttributeTarget")]
        public List<AttributeTarget> AttributeTargets { get; set; }
    }
}