using System;
using System.Collections.Generic;
using System.Text;

namespace P2PDiscover.message {
    public class envelope {
        public int number { get; set; }
        public envelopeType type { get; set; }

        public string body { get; set; }
    }

    public enum envelopeType {
        message,
        confirm,
        connect,
        connectTo
    }

    public class connectTo {
        public string ip { get; set; }
        public int port { get; set; }
    }

}
