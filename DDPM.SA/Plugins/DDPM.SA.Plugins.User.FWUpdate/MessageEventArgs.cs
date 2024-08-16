namespace DDPM.SA.Plugins.User.FWUpdate
{
    using System;

    [Serializable]
    public class MessageEventArgs
    {
        private readonly byte[] _Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MessageEventArgs(byte[] message)
        {
            this._Message = message;
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public byte[] Message
        {
            get
            {
                return this._Message;
            }
        }
    }
}