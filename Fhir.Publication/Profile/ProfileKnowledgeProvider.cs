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

        public const string DSTU1URL = "http://www.hl7.org/implement/standards/fhir/";

        internal ProfileKnowledgeProvider(string baseName)
        {
            _loader = new StructureLoader(ArtifactResolver.CreateCachedDefault());
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

        internal string GetLinkForTypeDocu(string typename)
        {
            if (typename == "*")
                return SpecUrl() + "datatypes.html#open";
            else if (isDataType(typename) || isPrimitive(typename))
                return SpecUrl() + "datatypes.html#" + typename.ToLower();
            else if (ModelInfo.IsKnownResource(typename))
                return SpecUrl() + typename.ToLower() + ".html";
            else if (typename == "Extension")
                return SpecUrl() + "extensibility.html#Extension";
            else
                throw new NotImplementedException("Don't know how to link to specification page for type " + typename);
        }

        internal bool HasLinkForTypeDocu(string typename)
        {
            return typename == "*" || isDataType(typename) || isPrimitive(typename) || typename == "Extension" || ModelInfo.IsKnownResource(typename);
        }



        public string SpecUrl()
        {
            return DSTU1URL;
            //if (version.StartsWith("0.8") || version == null) return DSTU1URL;

            //throw new NotImplementedException("Do not know the URL to specification version  " + version);
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


        public const string CORE_TYPE_PROFILEREFERENCE_PREFIX = "http://hl7.org/fhir/Profile/";

        internal string GetLinkForProfileReference(Profile profile, string p)
        {
            if (p.StartsWith(CORE_TYPE_PROFILEREFERENCE_PREFIX))
            {
                string rn = p.Substring(CORE_TYPE_PROFILEREFERENCE_PREFIX.Length);
                return GetLinkForTypeDocu(rn);
            }
            else if (p.StartsWith("#"))
                return GetLinkForLocalStructure(profile, p.Substring(1));
            else
                return p;
        }

        internal string GetLabelForProfileReference(Profile profile, string p)
        {
            if (p.StartsWith(CORE_TYPE_PROFILEREFERENCE_PREFIX))
                return p.Substring(CORE_TYPE_PROFILEREFERENCE_PREFIX.Length);
            else
                return p;
        }

        internal string GetLinkForExtensionDefinition(Profile profile, Profile.ProfileExtensionDefnComponent extension)       
        {
            return GetLinkForExtensionDefinition(profile, extension.Code);
        }

        internal string GetLinkForExtensionDefinition(Profile profile, string extensionUrl)
        {
            if (extensionUrl.StartsWith("#"))
            {
                var extension = extensionUrl.Substring(1);
                return GetLinkForProfileDict(profile) + "#extension." + TokenizeName(extension).ToLower();
            }
            else
            {
                return extensionUrl;
            }
        }


        internal string GetLinkForElementDefinition(Profile.ProfileStructureComponent s, Profile profile, Profile.ElementComponent element)
        {
            return GetLinkForProfileDict(profile) + "#" + MakeElementDictAnchor(s,element);
        }

        internal string GetLinkForLocalStructure(Profile profile, Profile.ProfileStructureComponent structure)
        {
            return GetLinkForLocalStructure(profile, structure.Name);
        }

        internal string GetLinkForLocalStructure(Profile profile, string name)
        {
            return GetProfilePageName(profile) + "-" + TokenizeName(name).ToLower() + ".html";
        }



        private string GetProfilePageName(Profile profile)
        {
            return TokenizeName(profile.Name).ToLower();
        }


        public string GetLinkForProfileDict(Profile profile)
        {
            return GetProfilePageName(profile) + "-definition" + ".html";
        }

        public string GetLinkForProfileTable(Profile profile)
        {
            return GetProfilePageName(profile) + ".html";
        }

        public string MakeElementDictAnchor(Profile.ProfileStructureComponent s, Profile.ElementComponent element)
        {
            if (element.Name == null)
                return s.Name + "." + element.Path;

            if (!element.Path.Contains("."))
                return s.Name + "." + element.Name;
            else
                return s.Name + "." + element.Path.Substring(0, element.Path.LastIndexOf(".")) + "." + element.Name;
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



        internal string resolveBinding(Model.Profile.ElementDefinitionBindingComponent binding)
        {
            if (binding.Reference == null)
                return null;

            String reference = binding.Reference is FhirUri ? ((FhirUri)binding.Reference).Value : ((ResourceReference)binding.Reference).Reference;

            if (reference.StartsWith("http://hl7.org/fhir/v3/vs/"))
                return MakeSpecLink("v3/" + reference.Substring(26) + "/index.html");
            else if (reference.StartsWith("http://hl7.org/fhir/vs/"))
                return MakeSpecLink(reference.Substring(23) + ".html");
            else if (reference.StartsWith("http://hl7.org/fhir/v2/vs/"))
                return MakeSpecLink("v2/" + reference.Substring(26) + "/index.html");
            else
                return reference;
        }


        internal string MakeSpecLink(string p)
        {
            return SpecUrl() + p;
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
