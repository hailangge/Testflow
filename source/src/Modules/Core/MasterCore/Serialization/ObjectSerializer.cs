using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testflow.MasterCore.Common;
using Testflow.Modules;
using Testflow.Usr;
using Testflow.Utility.I18nUtil;

namespace Testflow.MasterCore.Serialization
{
    internal class ObjectSerializer
    {
        private readonly I18N _i18n;
        public ObjectSerializer(I18N i18n)
        {
            this._i18n = i18n;
        }

        public string Serialize(object sourceValue)
        {
            if (null == sourceValue)
            {
                return CommonConst.NullValue;
            }
            Type valueType = sourceValue.GetType();
            if (valueType == typeof(string))
            {
                return (string)sourceValue;
            }
            if (valueType.IsEnum)
            {
                return sourceValue.ToString();
            }
            if (valueType.IsValueType)
        }

        public TDataType Deserialize<TDataType>(string valueString)
        {

        }
    }
}