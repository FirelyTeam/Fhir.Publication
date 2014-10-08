using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication.Experimental
{
    public class Context
    {
        public Root Root;
        public Location Location;
        public Location Source
        {
            get
            {
                return Location.Combine(Root.Source, Location);
            }
        }
        public Location Target
        {
            get
            {
                return Location.Combine(Root.Target, Location);
            }
        }
        public void EnsureTarget()
        {
            Directory.CreateDirectory(Target.Directory);
        }

        public void MoveTo(string dir)
        {
            this.Location += new Location(dir); 
        }

        public Context(Root root, Location location = null)
        {
            this.Root = root;
            this.Location = location ?? new Location();
        }
        public static Context CreateFromSource(Root root, string path)
        {
            path = Path.GetDirectoryName(path);
            var location = Location.RelativeFrom(root.Source, path);
            return new Context(root, location);
        }
        public Context Clone()
        {
            Context context = new Context(this.Root);
            context.Location = this.Location.Clone();
            return context;
        }

        public override string ToString()
        {
            string s = Location.Directory;
            if (string.IsNullOrWhiteSpace(s)) s = "(root)";
            return s;
        }
    }
}
