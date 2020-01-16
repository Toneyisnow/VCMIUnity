using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.Messages
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
