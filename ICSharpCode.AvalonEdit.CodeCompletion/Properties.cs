using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Description of PropertyGroup.
    /// </summary>
    public class Properties
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public string this[string property]
        {
            get { return Convert.ToString(Get(property), CultureInfo.InvariantCulture); }
            set { Set(property, value); }
        }

        public string[] Elements
        {
            get
            {
                lock (properties)
                {
                    var ret = new List<string>();
                    foreach (var property in properties)
                        ret.Add(property.Key);
                    return ret.ToArray();
                }
            }
        }

        public int Count
        {
            get
            {
                lock (properties)
                {
                    return properties.Count;
                }
            }
        }

        public object Get(string property)
        {
            lock (properties)
            {
                object val;
                properties.TryGetValue(property, out val);
                return val;
            }
        }

        public void Set<T>(string property, T value)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            if (value == null)
                throw new ArgumentNullException("value");
            T oldValue = default(T);
            lock (properties)
            {
                if (!properties.ContainsKey(property))
                {
                    properties.Add(property, value);
                }
                else
                {
                    oldValue = Get(property, value);
                    properties[property] = value;
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(this, property, oldValue, value));
        }

        public bool Contains(string property)
        {
            lock (properties)
            {
                return properties.ContainsKey(property);
            }
        }

        public bool Remove(string property)
        {
            lock (properties)
            {
                return properties.Remove(property);
            }
        }

        public override string ToString()
        {
            lock (properties)
            {
                var sb = new StringBuilder();
                sb.Append("[Properties:{");
                foreach (var entry in properties)
                {
                    sb.Append(entry.Key);
                    sb.Append("=");
                    sb.Append(entry.Value);
                    sb.Append(",");
                }
                sb.Append("}]");
                return sb.ToString();
            }
        }

        public static Properties ReadFromAttributes(XmlReader reader)
        {
            var properties = new Properties();
            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    // some values are frequently repeated (e.g. type="MenuItem"),
                    // so we also use the NameTable for attribute values
                    // (XmlReader itself only uses it for attribute names)
                    string val = reader.NameTable.Add(reader.Value);
                    properties[reader.Name] = val;
                }
                reader.MoveToElement(); //Moves the reader back to the element node.
            }
            return properties;
        }

        internal void ReadProperties(XmlReader reader, string endElement)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == endElement)
                        {
                            return;
                        }
                        break;
                    case XmlNodeType.Element:
                        string propertyName = reader.LocalName;
                        if (propertyName == "Properties")
                        {
                            propertyName = reader.GetAttribute(0);
                            var p = new Properties();
                            p.ReadProperties(reader, "Properties");
                            properties[propertyName] = p;
                        }
                        else if (propertyName == "Array")
                        {
                            propertyName = reader.GetAttribute(0);
                            properties[propertyName] = ReadArray(reader);
                        }
                        else if (propertyName == "SerializedValue")
                        {
                            propertyName = reader.GetAttribute(0);
                            properties[propertyName] = new SerializedValue(reader.ReadInnerXml());
                        }
                        else
                        {
                            properties[propertyName] = reader.HasAttributes ? reader.GetAttribute(0) : null;
                        }
                        break;
                }
            }
        }

        private ArrayList ReadArray(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return new ArrayList(0);
            var l = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "Array")
                        {
                            return l;
                        }
                        break;
                    case XmlNodeType.Element:
                        l.Add(reader.HasAttributes ? reader.GetAttribute(0) : null);
                        break;
                }
            }
            return l;
        }

        public void WriteProperties(XmlWriter writer)
        {
            lock (properties)
            {
                var sortedProperties = new List<KeyValuePair<string, object>>(properties);
                sortedProperties.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Key, b.Key));
                foreach (var entry in sortedProperties)
                {
                    object val = entry.Value;
                    if (val is Properties)
                    {
                        writer.WriteStartElement("Properties");
                        writer.WriteAttributeString("name", entry.Key);
                        ((Properties) val).WriteProperties(writer);
                        writer.WriteEndElement();
                    }
                    else if (val is Array || val is ArrayList)
                    {
                        writer.WriteStartElement("Array");
                        writer.WriteAttributeString("name", entry.Key);
                        foreach (object o in (IEnumerable) val)
                        {
                            writer.WriteStartElement("Element");
                            WriteValue(writer, o);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    else if (TypeDescriptor.GetConverter(val).CanConvertFrom(typeof (string)))
                    {
                        writer.WriteStartElement(entry.Key);
                        WriteValue(writer, val);
                        writer.WriteEndElement();
                    }
                    else if (val is SerializedValue)
                    {
                        writer.WriteStartElement("SerializedValue");
                        writer.WriteAttributeString("name", entry.Key);
                        writer.WriteRaw(((SerializedValue) val).Content);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteStartElement("SerializedValue");
                        writer.WriteAttributeString("name", entry.Key);
                        var serializer = new XmlSerializer(val.GetType());
                        serializer.Serialize(writer, val, null);
                        writer.WriteEndElement();
                    }
                }
            }
        }

        private void WriteValue(XmlWriter writer, object val)
        {
            if (val != null)
            {
                if (val is string)
                {
                    writer.WriteAttributeString("value", val.ToString());
                }
                else
                {
                    TypeConverter c = TypeDescriptor.GetConverter(val.GetType());
                    writer.WriteAttributeString("value", c.ConvertToInvariantString(val));
                }
            }
        }

        public void Save(string fileName)
        {
            var writer = new XmlTextWriter(fileName, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            Save(writer);
        }

        public void Save(XmlWriter writer)
        {
            using (writer)
            {
                writer.WriteStartElement("Properties");
                WriteProperties(writer);
                writer.WriteEndElement();
            }
        }

        //		public void BinarySerialize(BinaryWriter writer)
        //		{
        //			writer.Write((byte)properties.Count);
        //			foreach (KeyValuePair<string, object> entry in properties) {
        //				writer.Write(AddInTree.GetNameOffset(entry.Key));
        //				writer.Write(AddInTree.GetNameOffset(entry.Value.ToString()));
        //			}
        //		}

        public static Properties Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            var reader = new XmlTextReader(fileName);
            return Load(reader);
        }

        public static Properties Load(XmlReader reader)
        {
            using (reader)
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "Properties":
                                var properties = new Properties();
                                properties.ReadProperties(reader, "Properties");
                                return properties;
                        }
                    }
                }
            }
            return null;
        }

        public T Get<T>(string property, T defaultValue)
        {
            lock (properties)
            {
                object o;
                if (!properties.TryGetValue(property, out o))
                {
                    properties.Add(property, defaultValue);
                    return defaultValue;
                }

                if (o is string && typeof (T) != typeof (string))
                {
                    TypeConverter c = TypeDescriptor.GetConverter(typeof (T));
                    try
                    {
                        o = c.ConvertFromInvariantString(o.ToString());
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o; // store for future look up
                }
                else if (o is ArrayList && typeof (T).IsArray)
                {
                    var list = (ArrayList) o;
                    Type elementType = typeof (T).GetElementType();
                    Array arr = Array.CreateInstance(elementType, list.Count);
                    TypeConverter c = TypeDescriptor.GetConverter(elementType);
                    try
                    {
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            if (list[i] != null)
                            {
                                arr.SetValue(c.ConvertFromInvariantString(list[i].ToString()), i);
                            }
                        }
                        o = arr;
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o; // store for future look up
                }
                else if (!(o is string) && typeof (T) == typeof (string))
                {
                    TypeConverter c = TypeDescriptor.GetConverter(typeof (T));
                    if (c.CanConvertTo(typeof (string)))
                    {
                        o = c.ConvertToInvariantString(o);
                    }
                    else
                    {
                        o = o.ToString();
                    }
                }
                else if (o is SerializedValue)
                {
                    try
                    {
                        o = ((SerializedValue) o).Deserialize<T>();
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o; // store for future look up
                }
                try
                {
                    return (T) o;
                }
                catch (NullReferenceException)
                {
                    // can happen when configuration is invalid -> o is null and a value type is expected
                    return defaultValue;
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Nested type: SerializedValue

        /// <summary> Needed for support of late deserialization </summary>
        private class SerializedValue
        {
            private readonly string content;

            public SerializedValue(string content)
            {
                this.content = content;
            }

            public string Content
            {
                get { return content; }
            }

            public T Deserialize<T>()
            {
                var serializer = new XmlSerializer(typeof (T));
                return (T) serializer.Deserialize(new StringReader(content));
            }
        }

        #endregion
    }
}