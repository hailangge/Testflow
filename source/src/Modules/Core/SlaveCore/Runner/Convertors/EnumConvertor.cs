using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class EnumConvertor
    {
        private readonly TypeConvertor _convertor;

        private delegate bool ConvertFunction(object sourceValue, out object targetValue);

        private readonly Dictionary<string, ConvertFunction> _convertFuncs;

        public EnumConvertor(TypeConvertor convertor)
        {
            this._convertor = convertor;
            this._convertFuncs = new Dictionary<string, ConvertFunction>(15)
            {
                {
                    typeof(decimal).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (decimal) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(double).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (double) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(float).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (float) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(long).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (long) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(ulong).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (ulong) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(int).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (int) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(uint).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = (uint) EnumToIndex(sourceValue.GetType(), sourceValue);
                        return true;
                    }
                },
                {
                    typeof(short).Name, (object sourceValue, out object targetValue) =>
                    {
                        int index = EnumToIndex(sourceValue.GetType(), sourceValue);
                        if (index > short.MaxValue)
                        {
                            targetValue = short.MaxValue;
                            return false;
                        }
                        targetValue = (short) index;
                        return true;
                    }
                },
                {
                    typeof(ushort).Name, (object sourceValue, out object targetValue) =>
                    {
                        int index = EnumToIndex(sourceValue.GetType(), sourceValue);
                        if (index > ushort.MaxValue)
                        {
                            targetValue = ushort.MaxValue;
                            return false;
                        }
                        targetValue = (ushort) index;
                        return true;
                    }
                },
                {
                    typeof(char).Name, (object sourceValue, out object targetValue) =>
                    {
                        int index = EnumToIndex(sourceValue.GetType(), sourceValue);
                        if (index > char.MaxValue)
                        {
                            targetValue = char.MaxValue;
                            return false;
                        }
                        targetValue = (char) index;
                        return true;
                    }
                },
                {
                    typeof(byte).Name, (object sourceValue, out object targetValue) =>
                    {
                        int index = EnumToIndex(sourceValue.GetType(), sourceValue);
                        if (index > byte.MaxValue)
                        {
                            targetValue = byte.MaxValue;
                            return false;
                        }
                        targetValue = (byte) index;
                        return true;
                    }
                },
                {
                    typeof(string).Name, (object sourceValue, out object targetValue) =>
                    {
                        targetValue = EnumToString(sourceValue);
                        return true;
                    }
                }
            };
        }

        public bool TryCastConstantValue(Type targetType, string objStr, object originalValue, out object castValue)
        {
            bool parsePassed = TryFromStringToEnum(targetType, objStr, out castValue);
            if (parsePassed)
            {
                return true;
            }
            int index;
            // 如果未找到同名的枚举项，则检查该字符是否可以转换为整型，如果可以，则根据索引获取其第n个元素，否则抛出异常
            if (!int.TryParse(objStr, out index))
            {
                return false;
            }
            return TryFromIndexToEnum(targetType, index, out castValue);
        }

        public bool TryCastFromEnumToValue(Type targetType, object sourceValue, out object castValue)
        {
            if (!this._convertFuncs.ContainsKey(targetType.Name))
            {
                castValue = null;
                return false;
            }
            return this._convertFuncs[targetType.Name].Invoke(sourceValue, out castValue);
        }

        public bool TryCastFromValueToEnum(Type targetType, object sourceValue, out object castValue)
        {
            if (sourceValue is string)
            {
                return TryFromStringToEnum(targetType, (string)sourceValue, out castValue);
            }
            else
            {
                object indexObj;
                if (!this._convertor.TryCastValue(typeof(int), sourceValue, out indexObj))
                {
                    castValue = null;
                    return false;
                }
                return TryFromIndexToEnum(targetType, (int) indexObj, out castValue);
            }
        }

        public bool IsValidCastTarget(Type targetType)
        {
            return this._convertFuncs.ContainsKey(targetType.Name);
        }

        public bool IsValidCastSource(Type targetType)
        {
            return targetType == typeof(string) || this._convertor.IsValidValueCast(targetType, typeof(int));
        }

        private bool TryFromStringToEnum(Type enumType, string valueString, out object enumValue)
        {
            FieldInfo[] fieldValue =
                enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo fitEnumField = fieldValue.FirstOrDefault(item => item.Name.Equals(valueString));
            if (null == fitEnumField)
            {
                enumValue = null;
                return false;
            }
            enumValue = fitEnumField.GetValue(null);
            return true;
        }

        private bool TryFromIndexToEnum(Type enumType, int index, out object enumValue)
        {
            FieldInfo[] fieldValues =
                enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (fieldValues.Length <= index || index < 0)
            {
                enumValue = null;
                return false;
            }
            enumValue = fieldValues[index].GetValue(null);
            return true;
        }

        private string EnumToString(object enumValue)
        {
            return enumValue.ToString();
        }

        private int EnumToIndex(Type enumType, object enumValue)
        {
            string enumName = enumValue.ToString();
            FieldInfo[] fieldValues =
                enumType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (int i = 0; i < fieldValues.Length; i++)
            {
                if (string.CompareOrdinal(fieldValues[i].Name, enumName) == 0)
                {
                    return i;
                }
            }
            return int.MinValue;
        }
    }
}