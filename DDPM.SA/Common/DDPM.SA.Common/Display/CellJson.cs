using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    public class CellJson
    {
        public string Name { get; set; } = "";
        public double x { get; set; }
        public double y { get; set; }
        public double w { get; set; }
        public double h { get; set; }

        public CellJson Clone()
        {
            return new CellJson()
            {
                Name = this.Name,
                x = this.x,
                y = this.y,
                w = this.w,
                h = this.h
            };
        }
    }
}
