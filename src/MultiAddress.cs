using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ipfs.Api
{

    /// <summary>
    ///   A standard way to represent addresses that support any standard network protocols.
    /// </summary>
    /// <remarks>
    ///   A multi address is represented as a series of protocol codes and values pairs.  For example,
    ///   an IPFS file at a sepcific address over ipv4 and tcp is 
    ///   "/ip4/10.1.10.10/tcp/29087/ipfs/QmVcSqVEsvm5RR9mBLjwpb2XjFVn5bPdPL69mL8PH45pPC".
    ///   <para>
    ///   Protocol codes are case sensitive.
    ///   </para>
    /// </remarks>
    public class MultiAddress
    {
        const char separator = '/';
 
        public class Part
        {
            public string Protocol { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Protocol + separator + Value;
            }
        }

        public MultiAddress()
        {
            Parts = new List<Part>();
        }

        public MultiAddress(string s) : this()
        {
            Parse(s);
        }

        /// <summary>
        ///   The components of the <b>MultiAddress</b>.
        /// </summary>
        public List<Part> Parts { get; private set; }

        /// <summary>
        ///   The protocols used by the <b>MultiAddress</b>
        /// </summary>
        public IEnumerable<string> Protocols
        {
            get
            {
                return Parts.Select(p => p.Protocol);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var s = new StringBuilder();
            foreach (var part in Parts)
            {
                s.Append(separator);
                s.Append(part.Protocol);
                s.Append(separator);
                s.Append(part.Value);
            }
            return s.ToString();
        }

        void Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException("A MultiAddress string cannot be empty.");
            if (s[0] != separator)
                throw new FormatException("A MultiAddress string must start with a '" + separator + "'.");
            var parts = s.Substring(1).Split(separator);
            if (parts.Length == 0 || parts.Length % 2 != 0)
                throw new FormatException("The MultiAddress string is invalid.");
            for (int i = 0; i < parts.Length; i += 2)
            {
                Parts.Add(new Part { Protocol = parts[i], Value = parts[i + 1] });
            }
        }
    }
}
