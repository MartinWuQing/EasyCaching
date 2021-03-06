namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using Enyim.Caching.Memcached;
    using System;

    /// <summary>
    /// Message pack formatter transcoder.
    /// </summary>
    public class MessagePackFormatterTranscoder : DefaultTranscoder
    {    
        /// <summary>
        /// The serializer.
        /// </summary>
        private readonly IEasyCachingSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.MessagePackFormatterTranscoder"/> class.
        /// </summary>
        /// <param name="serializer">Serializer.</param>
        public MessagePackFormatterTranscoder(IEasyCachingSerializer serializer)
        {
            this._serializer = serializer;
        }        

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        protected override ArraySegment<byte> SerializeObject(object value)
        {            
            return _serializer.SerializeObject(value);            
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        protected override object DeserializeObject(ArraySegment<byte> value)
        {
            return _serializer.DeserializeObject(value);            
        }
    }
}
