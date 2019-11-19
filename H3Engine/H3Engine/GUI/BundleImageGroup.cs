using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.GUI
{
    public class BundleImageGroup
    {
        public BundleImageGroup()
        {
            this.Frames = new List<BundleImageFrame>();

        }

        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public List<BundleImageFrame> Frames
        {
            get;set;
        }
    }
}
