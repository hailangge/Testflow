using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Testflow.CoreCommon;
using Testflow.Data;
using Testflow.SlaveCore.Common;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner
{
    internal class EnumConvertor
    {
        private readonly SlaveContext _context;

        private readonly Dictionary<string, Func<object, object>> _convertFuncs;
        private readonly Dictionary<string, Func<Type, object, object>> _reverseConvertFuncs;

        public EnumConvertor(SlaveContext context)
        {
            this._context = context;
            _convertFuncs = new Dictionary<string, Func<object, object>>(15)
            {
                {typeof (decimal).Name, sourceValue => (decimal) (int) sourceValue},
                {typeof (double).Name, sourceValue => (double) (int) sourceValue},
                {typeof (float).Name, sourceValue => (float) (int) sourceValue},
                {typeof (long).Name, sourceValue => (long) (int) sourceValue},
                {typeof (ulong).Name, sourceValue => (ulong) (int) sourceValue},
                {typeof (int).Name, sourceValue => (int) sourceValue},
                {typeof (uint).Name, sourceValue => (uint) (int) sourceValue},
                {typeof (short).Name, sourceValue => (short) (int) sourceValue},
                {typeof (ushort).Name, sourceValue => (ushort) (int) sourceValue},
                {typeof (char).Name, sourceValue => (char) (int) sourceValue},
                {typeof (byte).Name, sourceValue => (byte) (int) sourceValue},
                {typeof (string).Name, sourceValue => sourceValue.ToString()}
            };
            _reverseConvertFuncs = new Dictionary<string, Func<Type, object, object>>(15)
            {
                {typeof (decimal).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (int)sourceValue)},
                {typeof (double).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (int)sourceValue)},
                {typeof (float).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (int)sourceValue)},
                {typeof (long).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (long)sourceValue)},
                {typeof (ulong).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (ulong)sourceValue)},
                {typeof (int).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (int)sourceValue)},
                {typeof (uint).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (uint)sourceValue)},
                {typeof (short).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (short)sourceValue)},
                {typeof (ushort).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (ushort)sourceValue)},
                {typeof (char).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (char)sourceValue)},
                {typeof (byte).Name, (targetType, sourceValue) => Enum.ToObject(targetType, (byte)sourceValue)},
                {typeof (string).Name, (targetType, sourceValue) => Enum.Parse(targetType, (string)sourceValue)}
            };
        }

        public object CastConstantValue(Type targetType, string objStr, object originalValue)
        {
            string[] enumNames = Enum.GetNames(targetType);
            // 获取同名的枚举项名称
            string enumName = enumNames.FirstOrDefault
                (item => item.Equals(objStr, StringComparison.OrdinalIgnoreCase));
            //如果未找到同名的枚举项，则检查该字符是否可以转换为整型，如果可以，则根据索引获取其第n个元素，否则抛出异常
            if (null == enumName)
            {
                int index;
                if (!int.TryParse(objStr, out index))
                {
                    _context.LogSession.Print(LogLevel.Error, _context.SessionId, 
                        $"Cast value <{objStr}> to type <{targetType.Name}> failed.");
                    throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                        _context.I18N.GetFStr("CastValueFailed", targetType.Name));
                }
                enumName = enumNames[index];
            }
            return Enum.Parse(targetType, enumName);
        }

        public object CastFromEnumToValue(Type targetType, object sourceValue)
        {
            if (!_convertFuncs.ContainsKey(targetType.Name))
            {
                _context.LogSession.Print(LogLevel.Error, _context.SessionId,
                        $"Cast value <{sourceValue}> to type <{targetType.Name}> failed.");
                throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                    _context.I18N.GetFStr("CastValueFailed", targetType.Name));
            }
            return _convertFuncs[targetType.Name].Invoke(sourceValue);
        }

        public object CastFromValueToEnum(Type targetType, object sourceValue)
        {
            Type sourceType = sourceValue.GetType();
            if (!_reverseConvertFuncs.ContainsKey(sourceType.Name))
            {
                _context.LogSession.Print(LogLevel.Error, _context.SessionId,
                        $"Cast value <{sourceValue}> to type <{targetType.Name}> failed.");
                throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                    _context.I18N.GetFStr("CastValueFailed", targetType.Name));
            }
            return _reverseConvertFuncs[sourceType.Name].Invoke(targetType, sourceValue);
        }

        public bool IsValidCastTarget(Type targetType)
        {
            return _convertFuncs.ContainsKey(targetType.Name);
        }

        public bool IsValidCastSource(Type targetType)
        {
            return _reverseConvertFuncs.ContainsKey(targetType.Name);
        }
    }
}