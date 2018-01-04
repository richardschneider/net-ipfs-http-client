using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Api
{
    /// <inheritdoc />
    public class Block : IDataBlock
    {
        byte[] dataBytes;

        /// <inheritdoc />
        public MultiHash Hash { get; set; }

        /// <inheritdoc />
        public byte[] DataBytes
        {
            get
            {
                return dataBytes;
            }
            set
            {
                dataBytes = value;
            }
        }

        /// <inheritdoc />
        public Stream DataStream
        {
            get
            {
                return new MemoryStream(DataBytes, false);
            }
        }
    }

}
