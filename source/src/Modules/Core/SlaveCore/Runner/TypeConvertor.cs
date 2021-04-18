using System;
using System.Collections.Generic;
using Testflow.CoreCommon;
using Testflow.CoreCommon.Common;
using Testflow.Data;
using Testflow.SlaveCore.Common;
using Testflow.SlaveCore.Runner.Convertors;
using Testflow.Usr;

namespace Testflow.SlaveCore.Runner
{
    internal class TypeConvertor
    {
        private readonly SlaveContext _context;
        private readonly Dictionary<string, ValueConvertorBase> _convertors;
        // 非值类型转换器，仅用于静态入参类型的转换
        private readonly NonValueTypeConvertor _nonValueConvertor;
        private readonly EnumConvertor _enumConvertor;
        private readonly ValueConvertorBase _strConvertor;

        public TypeConvertor(SlaveContext context)
        {
            _context = context;
            string numericFormat = context.GetPropertyString("NumericFormat");
            _convertors = new Dictionary<string, ValueConvertorBase>(20)
            {
                {typeof (decimal).Name, new DecimalConvertor(numericFormat)},
                {typeof (double).Name, new DoubleConvertor(numericFormat)},
                {typeof (float).Name, new FloatConvertor(numericFormat)},
                {typeof (long).Name, new LongConvertor()},
                {typeof (ulong).Name, new ULongConvertor()},
                {typeof (int).Name, new IntConvertor()},
                {typeof (uint).Name, new UIntConvertor()},
                {typeof (short).Name, new ShortConvertor()},
                {typeof (ushort).Name, new UShortConvertor()},
                {typeof (char).Name, new CharConvertor()},
                {typeof (byte).Name, new ByteConvertor()},
                {typeof (bool).Name, new BoolConvertor()},
                {typeof (string).Name, new StringConvertor()},
                {typeof (DateTime).Name, new DateTimeConvertor()}
            };
            _strConvertor = _convertors[typeof (string).Name];
            _nonValueConvertor = new NonValueTypeConvertor(_context);
            _enumConvertor = new EnumConvertor(_context);
        }

        /// <summary>
        /// 运行时转换对象类型
        /// </summary>
        public object CastValue(ITypeData targetType, object sourceValue)
        {
            if (null == sourceValue)
            {
                _context.LogSession.Print(LogLevel.Warn, _context.SessionId, "Cannot cast null value.");
                return null;
            }
            Type targetRealType = _context.TypeInvoker.GetType(targetType);
            return CastValue(targetRealType, sourceValue);
        }

        private bool IsNeedNoConvert(Type sourceType, ITypeData targetType)
        {
            if (sourceType.Name.Equals(targetType.Name) && sourceType.Namespace.Equals(targetType.Namespace))
            {
                return true;
            }
            Type targetRealType = _context.TypeInvoker.GetType(targetType);
            return ModuleUtils.IsNeedNoConvert(sourceType, targetRealType);
        }

        /// <summary>
        /// 运行时转换对象类型
        /// </summary>
        public object CastValue(Type targetType, object sourceValue)
        {
            if (null == sourceValue)
            {
                _context.LogSession.Print(LogLevel.Warn, _context.SessionId, "Cannot cast null value.");
                return null;
            }
            Type sourceType = sourceValue.GetType();
            if (ModuleUtils.IsNeedNoConvert(sourceType, targetType))
            {
                return sourceValue;
            }
            if (IsValidValueCastExceptEnum(sourceType, targetType))
            {
                return _convertors[sourceType.Name].CastValue(targetType, sourceValue);
            }
            else if (sourceType.IsEnum)
            {
                // 如果目标也是枚举类型，则将源转换为int类型后再进行转换
                if (targetType.IsEnum)
                {
                    sourceValue = (int) sourceValue;
                }
                return _enumConvertor.CastFromEnumToValue(targetType, sourceValue);
            }
            else if (targetType.IsEnum)
            {
                return _enumConvertor.CastFromValueToEnum(targetType, sourceValue);
            }
            else
            {
                _context.LogSession.Print(LogLevel.Error, _context.SessionId,
                    $"Unsupported type cast from type <{sourceType.Name}> to type <{targetType.Name}>.");
                throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                    _context.I18N.GetFStr("InvalidValueTypeCast", sourceType.Name, targetType.Name));
            }
        }

        /// <summary>
        /// 字符串常量转换为值类型，测试生成时调用
        /// </summary>
        public object CastConstantValue(Type targetType, string sourceValue, object originalValue = null)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                return sourceValue;
            }
            else if (targetType.IsEnum)
            {
                return _enumConvertor.CastConstantValue(targetType, sourceValue, originalValue);
            }
            else if (_strConvertor.IsValidCastTarget(targetType))
            {
                return _strConvertor.CastValue(targetType, sourceValue);
            }
            else if (_nonValueConvertor.IsNonValueTypeString(targetType, ref sourceValue))
            {
                return _nonValueConvertor.CastConstantValue(targetType, sourceValue, originalValue);
            }
            throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                _context.I18N.GetFStr("InvalidTypeCast", targetType.Name));
        }

        /// <summary>
        /// 属性值类型转换
        /// </summary>
        public object FillPropertyAndFieldValues(Type targetType, string sourceValue, object originalValue)
        {
            if (targetType == typeof(string))
            {
                return sourceValue;
            }
            else if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, sourceValue);
            }
            else if (_strConvertor.IsValidCastTarget(targetType))
            {
                return _strConvertor.CastValue(targetType, sourceValue);
            }
            else if (_nonValueConvertor.IsNonValueTypeString(targetType, ref sourceValue))
            {
                return _nonValueConvertor.CastConstantValue(targetType, sourceValue, originalValue);
            }
            throw new TestflowDataException(ModuleErrorCode.UnsupportedTypeCast,
                _context.I18N.GetFStr("InvalidTypeCast", targetType.Name));
        }

        public object GetDefaultValue(ITypeData type)
        {
            return _convertors.ContainsKey(type.Name) ? _convertors[type.Name].GetDefaultValue() : null;
        }

        public bool IsValidValueCast(Type sourceType, Type targetType)
        {
            if (!sourceType.IsEnum && !targetType.IsEnum)
            {
                return _convertors.ContainsKey(sourceType.Name) &&
                    _convertors[sourceType.Name].IsValidCastTarget(targetType);
            }
            if (sourceType.IsEnum && targetType.IsEnum)
            {
                return true;
            }
            return sourceType.IsEnum
                ? _enumConvertor.IsValidCastTarget(targetType)
                : _enumConvertor.IsValidCastSource(sourceType);
        }

        private bool IsValidValueCastExceptEnum(Type sourceType, Type targetType)
        {
            if (sourceType.IsEnum || targetType.IsEnum)
            {
                return false;
            }
            return IsValidValueCast(sourceType, targetType);
        }
    }
}