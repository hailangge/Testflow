using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Testflow.Data.Attributes;
using Testflow.SequenceManager.Common;

namespace Testflow.SequenceManager.SequenceElements
{
    [Serializable]
    public class StepAttribute : IStepAttribute
    {
        public StepAttribute()
        {
            this.Index = -1;
            this.Target = string.Empty;
            this.Type = string.Empty;
            this.ParameterValues = new FlexibleList<string>();
            this.Name = string.Empty;
            this.Value = null;
        }

        public StepAttribute(SerializationInfo info, StreamingContext context)
        {
            ModuleUtils.FillDeserializationInfo(info, this, typeof(StepAttribute));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Index", this.Index);
            info.AddValue("Target", this.Target);
            info.AddValue("Type", this.Type);
            info.AddValue("Condition", this.Condition);
            info.AddValue("ParameterValues", this.ParameterValues);
        }

        public string Name { get; set; }
        public int Index { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }

        public string FullType
        {
            get { return $"{Target}.{Type}"; }
            set
            {
                // ignore:为了兼容性保留
            }
        }

        public string Condition { get; set; }
        public IList<string> ParameterValues { get; set; }
        public string Value { get; set; }

        public IStepAttribute Clone()
        {
            StepAttribute stepAttribute = new StepAttribute()
            {
                Index = -1,
                Target = this.Target,
                Type = this.Type,
                Condition = this.Condition,
                ParameterValues = new FlexibleList<string>(this.ParameterValues),
            };
            return stepAttribute;
        }
    }
}