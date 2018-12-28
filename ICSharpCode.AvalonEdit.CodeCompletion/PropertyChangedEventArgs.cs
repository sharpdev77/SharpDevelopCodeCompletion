using System;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class PropertyChangedEventArgs : EventArgs
    {
        private readonly string key;
        private readonly object newValue;
        private readonly object oldValue;
        private readonly Properties properties;

        public PropertyChangedEventArgs(Properties properties, string key, object oldValue, object newValue)
        {
            this.properties = properties;
            this.key = key;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <returns>
        /// returns the changed property object
        /// </returns>
        public Properties Properties
        {
            get { return properties; }
        }

        /// <returns>
        /// The key of the changed property
        /// </returns>
        public string Key
        {
            get { return key; }
        }

        /// <returns>
        /// The new value of the property
        /// </returns>
        public object NewValue
        {
            get { return newValue; }
        }

        /// <returns>
        /// The new value of the property
        /// </returns>
        public object OldValue
        {
            get { return oldValue; }
        }
    }
}