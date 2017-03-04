﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Trinity.TSL
{
    internal partial class StructDescriptor : AbstractStruct
    {
        static internal StructDescriptor VOID = new StructDescriptor("VOID");

        //TODO
        //internal bool IsEmpty = false;

        internal Dictionary<Field, int> OptionalFieldSequenceMap = new Dictionary<Field, int>();
        internal List<Field> Fields = new List<Field>();
        internal List<Field> InvisibleFields = new List<Field>();
        internal List<Field> ExternFields = new List<Field>(); // Disk Extern Fields

        public int top_level_index = -1;

        internal List<Field> AllFields
        {
            get
            {
                List<Field> all = new List<Field>();
                all.AddRange(Fields);
                all.AddRange(InvisibleFields);
                all.AddRange(ExternFields);
                return all;
            }
        }
        internal StructDescriptor(string name)
        {
            this.Name = name;
        }

        internal StructDescriptor(FieldType type)
        {
            Fields.Add(new Field { Type = type });
        }

        public StructDescriptor(WrappedSyntaxNode node, SpecificationScript script)
        {
            this.Name = node.name;
            WrappedSyntaxTreeHelper.ReadAttributes(this.Attribute, node);
            ReadFields(node, script);
            top_level_index = node.top_level_index;
        }

        private void ReadFields(WrappedSyntaxNode node, SpecificationScript script)
        {
            foreach (WrappedSyntaxNode field in node.children)
            {
                if (field.type == "NField")
                {
                    var CurrentField = new Field(field, script);
                    if (CurrentField.Modifiers.Contains(Modifier.Optional))
                    {
                        OptionalFieldSequenceMap[CurrentField] = OptionalFieldSequenceMap.Count;
                    }
                    if (CurrentField.Modifiers.Contains(Modifier.Invisible))
                    {
                        InvisibleFields.Add(CurrentField);
                    }
                    else if (CurrentField.Modifiers.Contains(Modifier.Extern))
                    {
                        ExternFields.Add(CurrentField);
                    }
                    else
                    {
                        Fields.Add(CurrentField);
                    }
                }
            }
        }

        internal Field GetFieldByName(string name)
        {
            foreach (var field in Fields)
            {
                if (field.Name.Equals(name))
                    return field;
            }

            foreach (var field in ExternFields)
            {
                if (field.Name.Equals(name))
                    return field;
            }

            foreach (var field in InvisibleFields)
            {
                if (field.Name.Equals(name))
                    return field;
            }

            return null;
        }

        internal Field GetFieldByAttribute(string key, string value)
        {
            foreach (var field in Fields)
            {
                string _value;
                if (field.Attribute.AttributeValuePairs.TryGetValue(key, out _value))
                {
                    if (value.Equals(_value))
                        return field;
                }
            }

            foreach (var field in ExternFields)
            {
                string _value;
                if (field.Attribute.AttributeValuePairs.TryGetValue(key, out _value))
                {
                    if (value.Equals(_value))
                        return field;
                }
            }

            foreach (var field in InvisibleFields)
            {
                string _value;
                if (field.Attribute.AttributeValuePairs.TryGetValue(key, out _value))
                {
                    if (value.Equals(_value))
                        return field;
                }
            }

            return null;
        }

        public bool IsFixed()
        {
            if (OptionalFieldSequenceMap.Count != 0)
                return false;
            foreach (var f in Fields)
            {
                if (!(f.Type is FixedFieldType))//Note that this is not equivent to (f.Type is DynamicFieldType), since
                    //UnresolvedFieldType is FixedFieldType AND DynamicFieldType
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Allows only AtomType and substruct with only value type fields
        /// </summary>
        /// <returns></returns>
        public bool ContainsOnlyValueTypeFields()
        {
            if (!IsFixed())
                return false;
            foreach (var f in Fields)
            {
                if (f.Type is AtomType)
                    continue;
                if (f.Type is StructFieldType && (f.Type as StructFieldType).descriptor.ContainsOnlyValueTypeFields())
                    continue;
                return false;
            }
            return true;
        }
    }
}