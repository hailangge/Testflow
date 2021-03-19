using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Testflow.MasterCore.Common;
using Testflow.Modules;
using Testflow.Usr;
using Testflow.Utility.I18nUtil;

namespace Testflow.MasterCore.Serialization
{
    internal class ObjectSerializer
    {
        private readonly I18N _i18n;
        private HashSet<string> _rawValueTypeNames;
        public ObjectSerializer(I18N i18n)
        {
            this._i18n = i18n;
            this._rawValueTypeNames = new HashSet<string>
            {
                nameof(String), nameof(Double), nameof(Single), nameof(Decimal), nameof(Int64), nameof(UInt64),
                nameof(Int32), nameof(UInt32), nameof(Int32), nameof(UInt32), nameof(Int16), nameof(UInt16),
                nameof(Char), nameof(Byte)
            };
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
            else if (valueType.IsArray)
            {
                Array sourceArray = sourceValue as Array;
                string[] result = new string[sourceArray.Length];
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    result[i] = sourceArray.GetValue(i)?.ToString() ?? CommonConst.NullValue;
                }
                return JsonConvert.SerializeObject(result);
            }
            else if (valueType.IsEnum || this._rawValueTypeNames.Contains(valueType.Name))
            {
                return sourceValue.ToString();
            }
            else
            {
                return JsonConvert.SerializeObject(sourceValue);
            }
        }
    }
}
