using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Source;

namespace Hl7.Fhir.Publication
{
    internal class ProfileKnowledgeProvider
    {
        private StructureLoader _loader;

        internal ProfileKnowledgeProvider(string specUrl)
        {
            _loader = new StructureLoader(ArtifactResolver.CreateCachedDefault());
            _specUrl = specUrl;
        }

        internal Model.Profile.ProfileExtensionDefnComponent getExtensionDefinition(Model.Profile profile, string url)
        {
            if (url.StartsWith("#"))
            {
                var extAnchor = url.Substring(1);
                return profile.ExtensionDefn.Where(ext => ext.Code == extAnchor).FirstOrDefault();
            }
            else
                return _loader.LocateExtension(new Uri(url));
        }

        internal Model.ValueSet GetValueSet(string url)
        {
            return _loader.ArtifactSource.ReadResourceArtifact(new Uri(url)) as ValueSet;
        }

        string _specUrl;

        internal string getLinkFor(string typename)
        {
            if (typename == "*") typename = "open";

            //TODO: Make this dependent on the DSTU1/2 etc website
            //TODO: There are more flavours (like narrative, extension, etc.)
            if (isDataType(typename) || isPrimitive(typename))
                return _specUrl + "datatypes.html#" + typename.ToLower();
            else if(ModelInfo.IsKnownResource(typename))
                return _specUrl + typename.ToLower() + ".html";
            else if(typename == "Extension")
                return _specUrl + "extensibility.html#Extension";
            else
                return "todo.html";
        }


        //TODO: Determine dynamically based on core profiles?
        internal bool isDataType(String value)
        {
            return new[] { "Identifier", "HumanName", "Address", "ContactPoint", "Timing", "Quantity", "Attachment", "Range",
                  "Period", "Ratio", "CodeableConcept", "Coding", "SampledData", "Age", "Distance", "Duration", "Count", "Money" }.Contains(value);
        }

        internal bool isReference(String value)
        {
            return value == "ResourceReference";
        }

        internal bool isPrimitive(String value)
        {
            return new[] { "boolean", "integer", "decimal", "base64Binary", "instant", "string", "date", "dateTime", "code", "oid", "uuid", "id" }.Contains(value);
        }



        internal string getLinkForExtension(Model.Profile profile, string url)
        {
            return "todo.html";
            //String fn;
            //String code;
            //if (url.StartsWith("#"))
            //{
            //    code = url.Substring(1);
            //}
            //else
            //{
            //    String[] path = url.Split("#");
            //    code = path[1];
            //    profile = definitions.getProfileByURL(path[0]);
            //}

            //if (profile != null)
            //{
            //    fn = (String)profile.getTag("filename");
            //    return Utilities.changeFileExt(fn, ".html");
            //}
            //return null;
        }

        internal string getLinkForProfile(Model.Profile profile, string p)
        {
            return "todo.html" + "|" + profile.Name;
            //        String fn;
            //if (!url.startsWith("#")) {
            //  String[] path = url.split("#");
            //  profile = definitions.getProfileByURL(path[0]);
            //  if (profile == null && url.startsWith("Profile/"))
            //    return "hspc-"+url.substring(8)+".html|"+url.substring(8);
            //}
            //if (profile != null) {
            //  fn = profile.getTag("filename")+"|"+profile.getNameSimple();
            //  return Utilities.changeFileExt(fn, ".html");
            //}
            //return null;
        }




        internal string getLinkForExtensionDefinition(Profile profile, Profile.ProfileExtensionDefnComponent extension)       
        {
            return GetLinkForProfileDict(profile) + "#extension." + TokenizeName(extension.Code).ToLower();
        }


        internal string getLinkForElementDefinition(Profile profile, Profile.ElementComponent element)
        {
            return GetLinkForProfileDict(profile) + "#" + MakeElementDictAnchor(element);
        }

        internal string getLinkForStructure(Profile profile, Profile.ProfileStructureComponent structure)
        {
            return getProfilePageName(profile) + "-" + TokenizeName(structure.Name).ToLower() + ".html";
        }



        private string getProfilePageName(Profile profile)
        {
            return TokenizeName(profile.Name).ToLower();
        }


        public string GetLinkForProfileDict(Profile profile)
        {
            return getProfilePageName(profile) + "-definition" + ".html";
        }

        public string GetLinkForProfileTable(Profile profile)
        {
            return getProfilePageName(profile) + ".html";
        }

        public string MakeElementDictAnchor(Profile.ElementComponent element)
        {
            if (element.Name == null)
                return element.Path;

            if (!element.Path.Contains("."))
                return element.Name;
            else
                return element.Path.Substring(0, element.Path.LastIndexOf(".")) + "." + element.Name;
        }



        internal static String TokenizeName(String cs)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < cs.Length; i++)
            {
                char c = cs[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_')
                    s.Append(c);
                else if (c != ' ')
                    s.Append("." + c.ToString());
            }

            return s.ToString();
        }



        internal string resolveBinding(Model.Profile.ElementDefinitionBindingComponent elementDefinitionBindingComponent)
        {
            return "todo.html";
            //  if (binding.getReference() == null)
            //    return null;
            //  if (binding.getReference() instanceof UriType) {
            //    String ref = ((UriType) binding.getReference()).getValue();
            //    if (ref.startsWith("http://hl7.org/fhir/v3/vs/"))
            //      return "v3/"+ref.substring(26)+"/index.html";
            //    else
            //      return ref;
            //  } else {
            //    String ref = ((Reference) binding.getReference()).getReferenceSimple();
            //    if (ref.startsWith("ValueSet/")) {
            //      ValueSet vs = definitions.getValuesets().get(ref.substring(8));
            //      if (vs == null)
            //        return ref.substring(9)+".html";
            //      else
            //        return (String) vs.getTag("filename");
            //    } else if (ref.startsWith("http://hl7.org/fhir/vs/")) {
            //      if (new File(Utilities.path(folders.dstDir, "valueset-"+ref.substring(23)+".html")).exists())
            //        return "valueset-"+ref.substring(23)+".html";
            //      else
            //        return ref.substring(23)+".html";
            //    }  else if (ref.startsWith("http://hl7.org/fhir/v3/vs/"))
            //      return "v3/"+ref.substring(26)+"/index.html"; 
            //    else
            //      return ref;
            //  } 
        }

        internal bool hasLinkFor(string typeRefCode)
        {
            return isDataType(typeRefCode) || isPrimitive(typeRefCode) || typeRefCode == "Extension" || ModelInfo.IsKnownResource(typeRefCode);
        }


        internal string MakeSpecRef(string p)
        {
            return _specUrl + p;
        }


        public const string V2_SYSTEM_PREFIX = "http://hl7.org/fhir/v2/";
        public const string V3_SYSTEM_PREFIX = "http://hl7.org/fhir/v3/";
        public const string FHIR_SYSTEM_PREFIX = "http://hl7.org/fhir/";

        internal ValueSet GetValueSetForSystem(string system)
        {
            string valuesetUri = null;

            if (system.StartsWith(V2_SYSTEM_PREFIX))
                valuesetUri = V2_SYSTEM_PREFIX + "vs/" + system.Substring(V2_SYSTEM_PREFIX.Length);
            else if (system.StartsWith(V3_SYSTEM_PREFIX))
                valuesetUri = V3_SYSTEM_PREFIX + "vs/" + system.Substring(V3_SYSTEM_PREFIX.Length);
            else if (system.StartsWith(FHIR_SYSTEM_PREFIX))
                valuesetUri = FHIR_SYSTEM_PREFIX + "vs/" + system.Substring(FHIR_SYSTEM_PREFIX.Length);

            if (valuesetUri != null)
                return GetValueSet(valuesetUri);
            else
                return null;
        }
    }

}
