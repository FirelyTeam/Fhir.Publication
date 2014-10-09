using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Location // relative path
    {
        public string Directory { get; set; }
        public bool Absolute
        {
            get
            {
                return Path.IsPathRooted(Directory);
            }

        }

        public static Location Combine(Location one, Location two)
        {
            Location result = new Location();
            result.Directory = Path.Combine(one.Directory, two.Directory);
            return result;
        }
        public Location() 
        {
            Directory = "";
        }

        public Location(string dir)
        {
            this.Directory = dir;
        }
        public static Location RelativeFrom(string basedir, string dir)
        {
            Location location = new Location();
            location.Directory = Disk.RelativePath(basedir, dir);
            return location;
        }
        public static Location RelativeFrom(Location baseloc, string dir)
        {
            return RelativeFrom(baseloc.Directory, dir);
        }
        public static Location RelativeFrom(Location baseloc, Location relative)
        {
            return RelativeFrom(baseloc.Directory, relative.Directory);
        }
        public Location Clone()
        {
            Location clone = new Location();
            clone.Directory = this.Directory;
            return clone;
        }

        public static Location operator + (Location one, Location two)
        {
            return Location.Combine(one, two);
        }

        public override string ToString()
        {
            return Directory;
        }
    }

}
