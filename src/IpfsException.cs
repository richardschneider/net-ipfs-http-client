using System;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Ipfs.Api
{
    ///<summary>
    ///  The exception that is thrown when a IPFS error is detected.
    ///</summary>
    [Serializable]
    public class IpfsException : Exception
    {
        const string defaultMessage = "An IPFS error occured."; 

        ///<summary>
        ///  Initializes a new instance of the <see cref="IpfsException"/> class.
        ///</summary>
        ///<remarks>
        ///  This constructor initializes the <see cref="Exception.Message"/> property of the new 
        ///  instance to a system-supplied message that describes the error and takes into 
        ///  account the current system culture.
        ///</remarks>
        public IpfsException()
            : base(defaultMessage)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="IpfsException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">
        ///   The error message that explains the reason for the exception.
        /// </param>
        /// <remarks>
        ///   This constructor initializes the <see cref="Exception.Message"/> property of the new 
        ///   instance to the <paramref name="message"/> parameter.
        /// </remarks>
        public IpfsException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="IpfsException"/> class with a specified error message and
        ///   inner <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">
        ///   The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///   The <see cref="Exception"/> that is the cause of the current exception.
        /// </param>
        public IpfsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="IpfsException"/> class with a specified inner <see cref="Exception"/>.
        /// </summary>
        /// <param name="innerException">
        ///   The <see cref="Exception"/> that is the cause of the current exception.
        /// </param>
        public IpfsException(Exception innerException)
            : base(defaultMessage, innerException)
        {
        }

        ///<summary>
        ///  Initializes a new instance of the <see cref="IpfsException"/> class with serialized data.
        ///</summary>
        ///<param name="info">
        ///  The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.
        ///</param>
        ///<param name="context">
        ///  The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
        ///</param>
        ///<remarks>
        ///  This constructor is called during deserialization to reconstitute the 
        ///  exception object transmitted over a stream. 
        ///</remarks>
        protected IpfsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}


