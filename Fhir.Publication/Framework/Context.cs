using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{
    public class Context
    {
        public string RootDir;
        public string TargetRootDir;
        public string FullPath;
        public string TargetExt;

        /// <summary>
        /// Input filename without path, including extension
        /// </summary>
        public string FileName
        {
            get
            {
                return Path.GetFileName(FullPath);
            }
        }

        public string RelativeDir
        {
            get
            {
                return FileUtils.RelativePath(RootDir, CurrentDir);
            }
        }

        public string CurrentDir
        {
            get 
            {
                if (FullPath != null)
                {
                    return Path.GetDirectoryName(FullPath);
                }
                else
                {
                    return RootDir;
                }
                
            }
        }

        public string TargetDir
        {
            get
            {
                return Path.Combine(TargetRootDir, RelativeDir);
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(FullPath);
            }
        }

        /// <summary>
        /// Input filename, without path and extension
        /// </summary>
        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public void EnsureTargetDir()
        {
            FileUtils.EnsurePath(TargetDir);
        }
        
        public string TargetFileName
        {
            get
            {
                if (TargetExt != null)
                {
                    return Path.ChangeExtension(FileName, TargetExt);
                }
                else
                {
                    return FileName;
                }
            }
        }

        public string TargetFullPath
        {
            get 
            {
                return Path.Combine(TargetDir, TargetFileName);
            }
        }

        public void Activate()
        {
            this.EnsureTargetDir();
            Directory.SetCurrentDirectory(this.CurrentDir);
        }
        public static Context Root(string source, string target)
        {
            Context context = new Context();
            context.RootDir = source;
            context.TargetRootDir = target;
            return context;

        }

        public Context Clone()
        {
            Context context = new Context();
            context.RootDir = this.RootDir;
            context.TargetRootDir = this.TargetRootDir;
            return context;
        }

        public Context Clone(string filename)
        {
            Context context = this.Clone();
            context.FullPath = filename;
            return context;
        }

        public Context Clone(string filename, string targetext)
        {
            Context context = this.Clone();
            context.FullPath = filename;
            context.TargetExt = targetext;
            return context;
        }

        public override string ToString()
        {
            return string.Join("\\", RelativeDir, FileName);
        }
    }
}
