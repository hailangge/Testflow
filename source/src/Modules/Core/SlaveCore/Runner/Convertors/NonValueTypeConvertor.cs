﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Testflow.CoreCommon;
using Testflow.CoreCommon.Common;
using Testflow.SlaveCore.Common;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner.Convertors
{
    internal class NonValueTypeConvertor
    {
        private readonly SlaveContext _context;
        private JsonSerializerSettings _settings;

        private const string ArrayDataRegex = @"^\[.*\]$";
        private const string ClassDataRegex = @"^\{.*\}$";

        private Regex _arrayRegex;
        private Regex _classRegex;
        private readonly BindingFlags _instanceBindingFlag;
        private readonly BindingFlags _staticBindingFlag;

        public NonValueTypeConvertor(SlaveContext context)
        {
            this._context = context;
            this._settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateFormatString = CommonConst.GlobalTimeFormat,
                DateParseHandling = DateParseHandling.None
            };
            this._arrayRegex = new Regex(ArrayDataRegex);
            this._classRegex = new Regex(ClassDataRegex);
            this._instanceBindingFlag = BindingFlags.Public | BindingFlags.Instance;
            this._staticBindingFlag = BindingFlags.Public | BindingFlags.Static;
        }

        public object CastConstantValue(Type targetType, string objStr, object originalValue)
        {
            if (targetType.IsInterface || targetType.IsAbstract)
            {
                throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                    this._context.I18N.GetFStr("CastInterface", targetType.Name));
            }
            object castedObject = null;
            
            try
            {
                // 数组转换
                // TODO 暂不考虑List的情况
                if (targetType.IsArray)
                {
                    if (this._arrayRegex.IsMatch(objStr))
                    {
                        castedObject = CastArrayData(targetType, objStr, originalValue);
                    }
                    else if (targetType == typeof(char[]))
                    {
                        castedObject = objStr.ToCharArray();
                    }
                    else
                    {
                        this._context.LogSession.Print(LogLevel.Error, this._context.SessionId,
                            $"Cannot cast value '{objStr}' to type <{targetType.Name}>.");
                        throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                            this._context.I18N.GetFStr("InvalidTypeCast", targetType.Name));
                    }
                }
                // struct
                else if (targetType.IsValueType)
                {
                    castedObject = CastStructData(targetType, objStr, originalValue);
                }
                else 
                {
                    castedObject = CastNormalClass(targetType, objStr, originalValue);
                }
            }
            catch (JsonReaderException ex)
            {
                this._context.LogSession.Print(LogLevel.Error, this._context.SessionId, $"Cast value <{objStr}> failed: {ex.Message}");
                throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                    this._context.I18N.GetFStr("CastValueFailed", targetType.Name), ex);
            }
            return castedObject;
        }

        public string SerializeObject(object value)
        {
            try
            {
                return JsonConvert.SerializeObject(value);
            }
            catch (JsonReaderException ex)
            {
                this._context.LogSession.Print(LogLevel.Debug, this._context.SessionId, ex,
                    $"Serialize type <{value.GetType().Name}> failed.");
                return CoreConstants.SerializationError;
            }
        }

        private object CastArrayData(Type targetType, string objStr, object originalValue)
        {
            object castedObject;
            Type elementType = targetType.GetElementType();
            int rank = targetType.GetArrayRank();
            switch (rank)
            {
                case 1:
                    castedObject = CastOneDimensionalArray(objStr, elementType, originalValue);
                    break;
                case 2:
                    castedObject = CastTwoDimensionalArray(objStr, elementType, originalValue);
                    break;
                case 3:
                    castedObject = CastThreeDimensionalArray(objStr, elementType, originalValue);
                    break;
                default:
                    this._context.LogSession.Print(LogLevel.Error, this._context.SessionId,
                        $"Unable to cast array that have a rank <{rank}>.");
                    throw new TestflowRuntimeException(ModuleErrorCode.UnsupportedTypeCast,
                        this._context.I18N.GetFStr("CastValueFailed", targetType.Name));
                    break;
            }
            return castedObject;
        }

        private Array CastOneDimensionalArray(string objStr, Type elementType, object originalValue)
        {
            string[] datas = JsonConvert.DeserializeObject<string[]>(objStr, this._settings);
            if (elementType == typeof(string))
            {
                Array array = originalValue as Array;
                if (array == null) return datas;
                Array originalArray = array;
                Array.Copy(datas, originalArray, datas.Length);
                return originalArray;
            }
            Array targetInstance = null == originalValue
                ? Array.CreateInstance(elementType, datas.Length)
                : (Array) originalValue;
            
            for (int i = 0; i < datas.Length; i++)
            {
                object elementValue = this._context.Convertor.CastConstantValue(elementType, datas[i]);
                targetInstance.SetValue(elementValue, i);
            }
            return targetInstance;
        }

        private Array CastTwoDimensionalArray(string objStr, Type elementType, object originalValue)
        {
            string[,] datas = JsonConvert.DeserializeObject<string[,]>(objStr, this._settings);
            if (elementType == typeof(string))
            {
                Array array = originalValue as Array;
                if (array == null) return datas;
                Array originalArray = array;
                Array.Copy(datas, originalArray, datas.Length);
                return originalArray;
            }
            Array targetInstance = null == originalValue
                ? Array.CreateInstance(elementType, datas.GetLength(0), datas.GetLength(1))
                : (Array)originalValue;
            for (int i = 0; i < datas.GetLength(0); i++)
            {
                for (int j = 0; j < datas.GetLength(1); j++)
                {
                    object elementValue = this._context.Convertor.CastConstantValue(elementType, datas[i, j]);
                    targetInstance.SetValue(elementValue, i, j);
                }
            }
            return targetInstance;
        }

        private Array CastThreeDimensionalArray(string objStr, Type elementType, object originalValue)
        {
            string[,,] datas = JsonConvert.DeserializeObject<string[,,]>(objStr, this._settings);
            if (elementType == typeof(string))
            {
                Array array = originalValue as Array;
                if (array == null) return datas;
                Array originalArray = array;
                Array.Copy(datas, originalArray, datas.Length);
                return originalArray;
            }
            Array targetInstance = null == originalValue
                ? Array.CreateInstance(elementType, datas.GetLength(0), datas.GetLength(1), datas.GetLength(2))
                : (Array)originalValue;
            for (int i = 0; i < datas.GetLength(0); i++)
            {
                for (int j = 0; j < datas.GetLength(1); j++)
                {
                    for (int k = 0; k < datas.GetLength(2); k++)
                    {
                        object elementValue = this._context.Convertor.CastConstantValue(elementType, datas[i, j, k]);
                        targetInstance.SetValue(elementValue, i, j, k);
                    }
                }
            }
            return targetInstance;
        }

        private object CastStructData(Type targetType, string objStr, object originalValue)
        {
            object targetInstance = originalValue ??
                                    targetType.Assembly.CreateInstance(ModuleUtils.GetTypeFullName(targetType));
            SetValueToStructOrClass(targetType, objStr, ref targetInstance);
            object castedObject = targetInstance;
            return castedObject;
        }

        private object CastNormalClass(Type targetType, string objStr, object originalValue)
        {
            object targetInstance = originalValue;
            if (null == targetInstance)
            {
                ConstructorInfo constructor = targetType.GetConstructor(new Type[] { });
                if (null == constructor)
                {
                    this._context.LogSession.Print(LogLevel.Error, this._context.SessionId,
                        $"Cannot cast string <{objStr}> as target type <{targetType.Name}> has no default constructor.");
                    throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                        this._context.I18N.GetFStr("NoDefaultConstructor", targetType.Name));
                }
                targetInstance = constructor.Invoke(new object[] { });
            }
            SetValueToStructOrClass(targetType, objStr, ref targetInstance);
            object castedObject = targetInstance;
            return castedObject;
        }

        private void SetValueToStructOrClass(Type targetType, string objStr, ref object targetInstance)
        {
            Dictionary<string, string> datas = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                objStr, this._settings);
            foreach (string propertyName in datas.Keys)
            {
                // 在实例属性中查找
                if (FindAndSetValueFromProperty(targetType, targetInstance, propertyName, datas, this._instanceBindingFlag)) continue;
                // 在实例字段中查找
                if (FindAndSetValueFromField(targetType, targetInstance, propertyName, datas, this._instanceBindingFlag)) continue;
                // 在静态属性中查找
                if (FindAndSetValueFromProperty(targetType, targetInstance, propertyName, datas, this._staticBindingFlag)) continue;
                // 在静态字段中查找
                if (FindAndSetValueFromField(targetType, targetInstance, propertyName, datas, this._staticBindingFlag)) continue;
                this._context.LogSession.Print(LogLevel.Error, this._context.SessionId,
                    $"Unable to find property or field <{propertyName}> in type <{ModuleUtils.GetTypeFullName(targetType)}>.");
                throw new TestflowRuntimeException(ModuleErrorCode.UnsupportedTypeCast,
                        this._context.I18N.GetFStr("CastValueFailed", targetType.Name));
            }
        }

        private bool FindAndSetValueFromProperty(Type targetType, object targetInstance, string propertyName,
            Dictionary<string, string> datas, BindingFlags flags)
        {
            PropertyInfo propertyInfo = targetType.GetProperty(propertyName, flags);
            if (null == propertyInfo) return false;
            object originalValue = propertyInfo.GetValue(targetInstance);
            object propertyValue = this._context.Convertor.CastConstantValue(propertyInfo.PropertyType,
                datas[propertyName], originalValue);
            propertyInfo.SetValue(targetInstance, propertyValue);
            return true;
        }

        private bool FindAndSetValueFromField(Type targetType, object targetInstance, string propertyName, Dictionary<string, string> datas, BindingFlags flags)
        {
            FieldInfo fieldInfo = targetType.GetField(propertyName, this._instanceBindingFlag);
            if (null == fieldInfo) return false;
            object originalValue = fieldInfo.GetValue(targetInstance);
            object propertyValue = this._context.Convertor.CastConstantValue(fieldInfo.FieldType,
                datas[propertyName], originalValue);
            fieldInfo.SetValue(targetInstance, propertyValue);
            return true;
        }

        public bool IsNonValueTypeString(Type typeData, ref string valueString)
        {
            // 如果字符串时json的数组类型或者类类型，或者目标类型为byte[]，认为是非值类型的字符
            return this._arrayRegex.IsMatch(valueString) || this._classRegex.IsMatch(valueString) ||
                   ReferenceEquals(typeData, typeof(char[]));
        }
    }
}