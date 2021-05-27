using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Testflow.Runtime.Data;
using Testflow.Usr;

namespace Testflow.DataMaintainer
{
    internal class DataModelMapper
    {
        private const string InvalidChar = "'";
        private const string ReplaceChar = "''";

        // 数据类型到对应表格的映射
        private readonly Dictionary<string, string> _typeToTableMapping;
        // 对应表格->每个表格的映射表->表格列名和属性名的正反映射
        private readonly Dictionary<string, Dictionary<string, string>> _tableToColumnPropertyMapping;

        private readonly Dictionary<string, Func<object, string>> _valueToStrConvertor;
        
        private readonly Dictionary<string, Func<object, object>> _rawDataToValueConvertor;

        // 类型转换和字符串转换为对象委托的映射
        private readonly Dictionary<Type, Func<object, string>> _classTypeConvertorMapping;

        // 类型转换和对象转换为字符串委托的映射
        private readonly Dictionary<Type, Func<string, object>> _classTypeParserMapping;

        #region 字符转义相关字段

        // 序列化时需要转义的字符
        private readonly HashSet<char> _escapeChars;

        private readonly HashSet<string> _needEscapeColumns;

        // 字符转义缓存
        private readonly StringBuilder _escapeCache;

        private const string EscapeFormat = "!x{0}!";
        private readonly Regex _escapeRegex;

        #endregion

        public DataModelMapper()
        {
            _classTypeParserMapping = new Dictionary<Type, Func<string, object>>(10);
            _classTypeConvertorMapping = new Dictionary<Type, Func<object, string>>(10);

            // 初始化字符转义相关的字段。其中0x22(双引号)、0x2F(斜杠)、0x5C(反斜杠)虽然是特殊字符，但是无需处理
            this._escapeChars = new HashSet<char>
            {
                '\x0000', '\x0008', '\x0009', '\x000A', '\x000B', '\x000C', '\x000D', '\x000E', '\x000F', '\x0018',
                '\x001B', '\x001C', '\x001F', '\x0026', '\x0027', '\x0060', '\x7F'
            };
            this._escapeRegex = new Regex("!x([0-9A-F]{1,4})!", RegexOptions.Compiled | RegexOptions.RightToLeft);
            this._needEscapeColumns = new HashSet<string>()
            {
                DataBaseItemNames.InstanceNameColumn, DataBaseItemNames.SequenceGroupNameColumn,
                DataBaseItemNames.NameColumn, DataBaseItemNames.DescriptionColumn, DataBaseItemNames.ProjectNameColumn,
                DataBaseItemNames.ProjectDescriptionColumn, DataBaseItemNames.FailedInfoColumn,
                DataBaseItemNames.WatchDataColumn
            };

            this._escapeCache = new StringBuilder(1000);

            //            this._invalidCharRegex = new Regex("(?<=[^'])'(?=[^'])", RegexOptions.Compiled);
            _typeToTableMapping = new Dictionary<string, string>(10)
            {
                {typeof (TestInstanceData).Name, DataBaseItemNames.InstanceTableName},
                {typeof (SessionResultData).Name, DataBaseItemNames.SessionTableName},
                {typeof (SequenceResultData).Name, DataBaseItemNames.SequenceTableName},
                {typeof (RuntimeStatusData).Name, DataBaseItemNames.StatusTableName},
                {typeof (PerformanceStatus).Name, DataBaseItemNames.PerformanceTableName}
            };

            _tableToColumnPropertyMapping = new Dictionary<string, Dictionary<string, string>>(10)
            {
                {DataBaseItemNames.InstanceTableName, new Dictionary<string, string>()},
                {
                    DataBaseItemNames.SessionTableName, new Dictionary<string, string>()
                    {
                        { "Session", DataBaseItemNames.SessionIdColumn },
                        { DataBaseItemNames.SessionIdColumn, "Session" },
                        { "State", DataBaseItemNames.SessionStateColumn},
                        { DataBaseItemNames.SessionStateColumn, "State"}
                    }
                },
                {
                    DataBaseItemNames.SequenceTableName, new Dictionary<string, string>(10)
                    {
                        { "Session", DataBaseItemNames.SessionIdColumn },
                        { DataBaseItemNames.SessionIdColumn, "Session" },
                        { "Result", DataBaseItemNames.SequenceResultColumn },
                        { DataBaseItemNames.SequenceResultColumn, "Result" },
                    }
                },
                {
                    DataBaseItemNames.StatusTableName, new Dictionary<string, string>(10)
                    {
                        { "Session", DataBaseItemNames.SessionIdColumn },
                        { DataBaseItemNames.SessionIdColumn, "Session" },
                        { "Sequence", DataBaseItemNames.SequenceIndexColumn },
                        { DataBaseItemNames.SequenceIndexColumn, "Sequence" },
                        { "Time", DataBaseItemNames.RecordTimeColumn },
                        { DataBaseItemNames.RecordTimeColumn, "Time" },
                        { "Result", DataBaseItemNames.StepResultColumn },
                        { DataBaseItemNames.StepResultColumn, "Result" },
                    }
                },
                {
                    DataBaseItemNames.PerformanceTableName, new Dictionary<string, string>(10)
                    {
                        { "Session", DataBaseItemNames.SessionIdColumn },
                        { DataBaseItemNames.SessionIdColumn, "Session" },
                        { "Index", DataBaseItemNames.StatusIndexColumn },
                        { DataBaseItemNames.StatusIndexColumn, "Index" },
                        { "TimeStamp", DataBaseItemNames.RecordTimeColumn },
                        { DataBaseItemNames.RecordTimeColumn, "TimeStamp" },
                    }
                }
            };

            this._valueToStrConvertor = new Dictionary<string, Func<object, string>>(10);
            this._valueToStrConvertor.Add(typeof (int).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (double).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (uint).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (short).Name, value: new Func<object, string>((value) => value.ToString()));
            this._valueToStrConvertor.Add(typeof (ushort).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (byte).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (char).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (long).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (ulong).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (float).Name, (value) => value.ToString());
            this._valueToStrConvertor.Add(typeof (string).Name, (value) => $"'{value}'");
            this._valueToStrConvertor.Add(typeof (DateTime).Name,
                new Func<object, string>((value) => $"'{((DateTime) value).ToString(CommonConst.GlobalTimeFormat)}'"));

            this._rawDataToValueConvertor = new Dictionary<string, Func<object, object>>(10);
            this._rawDataToValueConvertor.Add(typeof(int).Name, (rawValue) => Convert.ToInt32(rawValue));
            this._rawDataToValueConvertor.Add(typeof(double).Name, (rawValue) => Convert.ToDouble(rawValue));
            this._rawDataToValueConvertor.Add(typeof(uint).Name, (rawValue) => Convert.ToUInt32(rawValue));
            this._rawDataToValueConvertor.Add(typeof(short).Name, (rawValue) => Convert.ToInt16(rawValue));
            this._rawDataToValueConvertor.Add(typeof(ushort).Name, (rawValue) => Convert.ToUInt16(rawValue));
            this._rawDataToValueConvertor.Add(typeof(byte).Name, (rawValue) => Convert.ToByte(rawValue));
            this._rawDataToValueConvertor.Add(typeof(char).Name, (rawValue) => Convert.ToChar(rawValue));
            this._rawDataToValueConvertor.Add(typeof(long).Name, (rawValue) => rawValue);
            this._rawDataToValueConvertor.Add(typeof(ulong).Name, (rawValue) => Convert.ToUInt64(rawValue));
            this._rawDataToValueConvertor.Add(typeof(float).Name, (rawValue) => Convert.ToSingle(rawValue));
            this._rawDataToValueConvertor.Add(typeof(string).Name, (rawValue) => rawValue);
            this._rawDataToValueConvertor.Add(typeof(DateTime).Name, (rawValue) => DateTime.Parse((string)rawValue));
        }

        // 效率较低，后期通过ORM优化
        public TDataType ReadToObject<TDataType>(DbDataReader reader, TDataType dataObj) where TDataType : class
        {
            Type type = dataObj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                string columnName = GetTableColumnName(type.Name, propertyInfo.Name);
                object propertyValue = reader[columnName];
                if (null == propertyValue || DBNull.Value == propertyValue)
                {
                    continue;
                }
                // 回退需要转义的字符，_needEscapeColumns集合中的列都是字符串类型
                if (this._needEscapeColumns.Contains(columnName))
                {
                    RecoverEscapeCharacters(ref propertyValue);
                }

                Type propertyType = propertyInfo.PropertyType;
                if (typeof (DateTime).Name.Equals(propertyType.Name))
                {
                    propertyValue = DateTime.Parse((string)propertyValue);
                }
                else if (propertyType.IsEnum)
                {
                    propertyValue = Enum.Parse(propertyType, (string)propertyValue);
                }
                else if (_rawDataToValueConvertor.ContainsKey(propertyType.Name))
                {
                    propertyValue = _rawDataToValueConvertor[propertyType.Name].Invoke(propertyValue);
                }
                else if (_classTypeParserMapping.ContainsKey(propertyType))
                {
                    propertyValue = _classTypeParserMapping[propertyType].Invoke(propertyValue.ToString());
                }
                propertyInfo.SetValue(dataObj, propertyValue);
            }
            return dataObj;
        }

        public Dictionary<string, string> GetColumnValueMapping<TDataType>(TDataType dataObj) where TDataType : class
        {
            Type type = dataObj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, string> columnValueMapping = new Dictionary<string, string>(properties.Length);
            foreach (PropertyInfo propertyInfo in properties)
            {
                string columnName = GetTableColumnName(type.Name, propertyInfo.Name);
                Type propertyType = propertyInfo.PropertyType;
                object value = propertyInfo.GetValue(dataObj);
                if (null == value) continue;
                string valueStr = string.Empty;
                if (propertyType.IsEnum)
                {
                    valueStr = $"'{value}'";
                }
                else if (_valueToStrConvertor.ContainsKey(propertyType.Name))
                {
                    if(this._needEscapeColumns.Contains(columnName))
                    {
                        // 替换需要转义的字符
                        ReplaceEscapeCharacters(ref value);
                    }
                    valueStr = _valueToStrConvertor[propertyType.Name].Invoke(value);
                }
                else if (_classTypeConvertorMapping.ContainsKey(propertyType))
                {
                    string classValueStr = _classTypeConvertorMapping[propertyType].Invoke(value);
                    // 替换需要转义的字符
                    if (this._needEscapeColumns.Contains(columnName))
                    {
                        ReplaceEscapeCharacters(ref classValueStr);
                    }
                    valueStr = $"'{classValueStr}'";
                }
                columnValueMapping.Add(columnName, valueStr);
            }
            return columnValueMapping;
        }

        private string GetTableColumnName(string typeName, string propertyName)
        {
            string tableName = _typeToTableMapping[typeName];
            if (!_tableToColumnPropertyMapping[tableName].ContainsKey(propertyName))
            {
                return propertyName;
            }
            return _tableToColumnPropertyMapping[tableName][propertyName];
        }

        private string GetPropertyName(string typeName, string tableColumnName)
        {
            string tableName = _typeToTableMapping[typeName];
            if (!_tableToColumnPropertyMapping[tableName].ContainsKey(tableColumnName))
            {
                return tableColumnName;
            }
            return _tableToColumnPropertyMapping[tableName][tableColumnName];
        }

        public void RegisterTypeConvertor(Type type, Func<object, string> toStringFunc, Func<string, object> parseFunc)
        {
            if (_classTypeConvertorMapping.ContainsKey(type))
            {
                _classTypeConvertorMapping[type] = toStringFunc;
                _classTypeParserMapping[type] = parseFunc;
            }
            else
            {
                _classTypeConvertorMapping.Add(type, toStringFunc);
                _classTypeParserMapping.Add(type, parseFunc);
            }
        }

        #region 字符转义处理


        /// <summary>
        /// 替换字符串中的转义字符为!x{value}!的格式
        /// </summary>
        private bool ReplaceEscapeCharacters(ref string source)
        {
            if (!source.Any(item => this._escapeChars.Contains(item)))
            {
                return false;
            }
            this._escapeCache.Clear();
            this._escapeCache.Append(source);
            for (int i = this._escapeCache.Length - 1; i >= 0; i--)
            {
                if (!this._escapeChars.Contains(this._escapeCache[i]))
                {
                    continue;
                }
                string replaceString = ((ushort) this._escapeCache[i]).ToString("X");
                this._escapeCache.Remove(i, 1);
                this._escapeCache.Insert(i, string.Format(EscapeFormat, replaceString));
            }
            source = this._escapeCache.ToString();
            this._escapeCache.Clear();
            return true;
        }

        /// <summary>
        /// 替换字符串中的转义字符为!x{value}!的格式
        /// </summary>
        private bool ReplaceEscapeCharacters(ref object source)
        {
            if (!(source is string) || !((string)source).Any(item => this._escapeChars.Contains(item)))
            {
                return false;
            }
            this._escapeCache.Clear();
            this._escapeCache.Append(source);
            for (int i = this._escapeCache.Length - 1; i >= 0; i--)
            {
                if (!this._escapeChars.Contains(this._escapeCache[i]))
                {
                    continue;
                }
                string replaceString = ((ushort)this._escapeCache[i]).ToString("X");
                this._escapeCache.Remove(i, 1);
                this._escapeCache.Insert(i, string.Format(EscapeFormat, replaceString));
            }
            source = this._escapeCache.ToString();
            this._escapeCache.Clear();
            return true;
        }

        /// <summary>
        /// 替换字符串中的转义字符为原始值
        /// </summary>
        private bool RecoverEscapeCharacters(ref object source)
        {
            MatchCollection matchResults = this._escapeRegex.Matches((string)source);
            if (matchResults.Count == 0)
            {
                return false;
            }
            this._escapeCache.Clear();
            this._escapeCache.Append(source);
            foreach (Match matchResult in matchResults)
            {
                char escapeChar = (char)Convert.ToUInt16(matchResult.Groups[1].Value, 16);
                this._escapeCache.Replace(matchResult.Value, escapeChar.ToString(), matchResult.Index,
                    matchResult.Length);
            }
            source = this._escapeCache.ToString();
            this._escapeCache.Clear();
            return true;
        }

        #endregion

    }
}