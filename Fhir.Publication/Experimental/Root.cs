using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class Root
    {
        public Location Source;
        public Location Target;
        public Root(string sourcedir, string targetdir)
        {
            this.Source = new Location(sourcedir);
            this.Target = new Location(targetdir);
        }
        public Context Context()
        {
            return new Context(this);

        }

        public override string ToString()
        {
            return Source.ToString();
        }

    }
}
