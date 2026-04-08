using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine.Core.Constants;

namespace H3Engine.Core.Messages
{
    /// <summary>
    ///
    /// </summary>
    public class QueryReply : NetMessage
    {
        public int Answer
        {
            get; set;
        }


    }


}


