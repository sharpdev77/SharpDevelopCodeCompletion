using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public static class PropertyService
    {
        private static string propertyFileName;
        private static string propertyXmlRootNodeName;

        private static string configDirectory;
        private static string dataDirectory;

        private static Properties properties;

        public static bool Initialized
        {
            get { return properties != null; }
        }

        public static string ConfigDirectory
        {
            get { return configDirectory; }
        }

        public static string DataDirectory
        {
            get { return dataDirectory; }
        }

        public static void InitializeServiceForUnitTests()
        {
            properties = null;
            InitializeService(null, null, null);
        }

        public static void InitializeService(string configDirectory, string dataDirectory, string propertiesName)
        {
            if (properties != null)
                throw new InvalidOperationException("Service is already initialized.");
            properties = new Properties();
            PropertyService.configDirectory = configDirectory;
            PropertyService.dataDirectory = dataDirectory;
            propertyXmlRootNodeName = propertiesName;
            propertyFileName = propertiesName + ".xml";
            properties.PropertyChanged += PropertiesPropertyChanged;
        }

        public static string Get(string property)
        {
            return properties[property];
        }

        public static T Get<T>(string property, T defaultValue)
        {
            return properties.Get(property, defaultValue);
        }

        public static void Set<T>(string property, T value)
        {
            properties.Set(property, value);
        }

        public static void Load()
        {
            if (properties == null)
                throw new InvalidOperationException("Service is not initialized.");
            if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(propertyXmlRootNodeName))
                throw new InvalidOperationException("No file name was specified on service creation");
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            if (!LoadPropertiesFromStream(Path.Combine(configDirectory, propertyFileName)))
            {
                LoadPropertiesFromStream(Path.Combine(DataDirectory, "options", propertyFileName));
            }
        }

        public static bool LoadPropertiesFromStream(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            try
            {
                using (LockPropertyFile())
                {
                    using (var reader = new XmlTextReader(fileName))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                if (reader.LocalName == propertyXmlRootNodeName)
                                {
                                    properties.ReadProperties(reader, propertyXmlRootNodeName);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
            }
            return false;
        }

        public static void Save()
        {
            if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(propertyXmlRootNodeName))
                throw new InvalidOperationException("No file name was specified on service creation");
            using (var ms = new MemoryStream())
            {
                var writer = new XmlTextWriter(ms, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement(propertyXmlRootNodeName);
                properties.WriteProperties(writer);
                writer.WriteEndElement();
                writer.Flush();

                ms.Position = 0;
                string fileName = Path.Combine(configDirectory, propertyFileName);
                using (LockPropertyFile())
                {
                    using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }
        }

        /// <summary>
        /// Acquires an exclusive lock on the properties file so that it can be opened safely.
        /// </summary>
        public static IDisposable LockPropertyFile()
        {
            var mutex = new Mutex(false, "PropertyServiceSave-30F32619-F92D-4BC0-BF49-AA18BF4AC313");
            mutex.WaitOne();
            return new CallbackOnDispose(
                delegate
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    });
        }

        private static void PropertiesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(null, e);
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;
    }
}