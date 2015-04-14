using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification.Source;

namespace Hl7.Fhir.Publication.Profile
{
    internal class ProfileKnowledgeProvider
    {
        public const string DSTUURL = "http://hl7-fhir.github.io/";

        private string baseName;

        private IArtifactSource _source;

        public ProfileKnowledgeProvider(string baseName, string imageOutputDirectory, string genImageUrl, string dist, bool standAlone, IArtifactSource source)
        {
            this.baseName = verifyEndSlash(baseName);  
            ImageOutputDirectory = imageOutputDirectory;
            GenImageUrl = verifyEndSlash(genImageUrl);
            DistUrl = verifyEndSlash(dist);
            StandAlone = standAlone;
            _source = source;
        }

        public bool IsResource(string typename)
        {
            return ModelInfo.IsKnownResource(typename);
        }

        //TODO: Determine dynamically based on core profiles?
        public bool isDataType(String value)
        {
            return new[] { "Identifier", "HumanName", "Address", "ContactPoint", "Timing", "Quantity", "Attachment", "Range",
                  "Period", "Ratio", "CodeableConcept", "Coding", "SampledData", "Age", "Distance", "Duration", "Count", "Money" }.Contains(value);
        }

        //TODO: Determine based on core profiles
        public bool isPrimitive(String value)
        {
            return new[] { "boolean", "integer", "string", "decimal", "uri", "base64Binary", "instant", "date", "dateTime", "time", "code", "oid", "id", "unsignedInt", "positiveInt" }.Contains(value);
        }


        public bool isReference(String value)
        {
            return value == "Reference";
        }


        public string GetLinkForElementDefinition(StructureDefinition structure, ElementDefinition element)
        {
            return GetLinkForProfileDict(structure) + "#" + MakeElementDictAnchor(element);
        }

        public static string GetLinkForProfileDict(StructureDefinition structure)
        {
            return GetProfilePageName(structure) + "-definition" + ".html";
        }

        private static string GetProfilePageName(StructureDefinition structure)
        {
            return TokenizeName(structure.Name).ToLower();
        }


        public string MakeElementDictAnchor(ElementDefinition element)
        {
            return element.Name ?? element.Path;
        }


        public StructureDefinition GetExtensionDefinition(string url)
        {
            var cr = _source.ReadConformanceResource(url) as StructureDefinition; 
            if(cr != null && cr.Type == StructureDefinition.StructureDefinitionType.Extension)
            {
                if(cr.Snapshot == null)
                    throw new NotImplementedException("No snapshot representation for StructureDefinition (extension) at url " + url);

                return cr;
            }
            else
                return null;
        }

        public StructureDefinition GetConstraintDefinition(string url)
        {
            var cr = _source.ReadConformanceResource(url) as StructureDefinition;
            if (cr != null && cr.Type == StructureDefinition.StructureDefinitionType.Constraint)
            {
                if (cr.Snapshot == null)
                    throw new NotImplementedException("No snapshot representation for StructureDefinition (constraint) at url " + url);

                return cr;
            }
            else
                return null;

        }

        public string GetLinkForExtensionDefinition(string extensionUrl)
        {
            var extd = GetExtensionDefinition(extensionUrl);

            if (extd != null) return extd.Url;

            return extensionUrl;
        }


        public string GetLinkForBinding(ElementDefinition.ElementDefinitionBindingComponent binding)
        {
            if (binding.ValueSet == null)
                return null;

            String reference = binding.ValueSet is FhirUri ? 
                    ((FhirUri)binding.ValueSet).Value : ((ResourceReference)binding.ValueSet).Reference;

            return GetLinkForValueSet(reference);
        }

        public string GetLinkForValueSet(String reference)
        {
            if (reference.StartsWith("http://hl7.org/fhir/v3/vs/"))
                return MakeSpecLink("v3/" + reference.Substring(26) + "/index.html");
            else if (reference.StartsWith("http://hl7.org/fhir/vs/"))
                return MakeSpecLink(reference.Substring(23) + ".html");
            else if (reference.StartsWith("http://hl7.org/fhir/v2/vs/"))
                return MakeSpecLink("v2/" + reference.Substring(26) + "/index.html");
            else
                return reference + ".html";
        }

        public string MakeSpecLink(string p)
        {
            return SpecUrl() + p;
        }


        public string SpecUrl()
        {
            return DSTUURL;
        }


        public const string CORE_TYPE_STRUCTDEF_PREFIX = "http://hl7.org/fhir/StructureDefinition";

        public string GetLinkForProfileReference(string uri)
        {
            if (uri.StartsWith(CORE_TYPE_STRUCTDEF_PREFIX))
            {
                var rn = new ResourceIdentity(uri).Id;

                if (HasLinkForCoreTypeDocu(rn))
                    return GetLinkForCoreTypeDocu(rn);
                else
                    return uri;
            }
            else
                return uri;
        }


        public string GetLabelForProfileReference(string uri)
        {
            if (uri.StartsWith(CORE_TYPE_STRUCTDEF_PREFIX))
                return new ResourceIdentity(uri).Id;
            else
                return uri;
        }



        public string GetLinkForCoreTypeDocu(string typename)
        {
            var link = generateCoreTypeLink(typename);

            if (link != null)
                return link;
            else
                throw new NotImplementedException("Don't know how to link to specification page for type " + typename);

        }

        private string generateCoreTypeLink(string typename)
        {
            if (typename == "*")
                return MakeSpecLink("datatypes.html#open");
            else if (isDataType(typename) || isPrimitive(typename))
                return MakeSpecLink("datatypes.html#" + typename.ToLower());
            else if (typename == "Any")
                return MakeSpecLink("resourcelist.html");
            else if (IsResource(typename))
                return MakeSpecLink(typename.ToLower() + ".html");
            else if (typename == "Extension")
                return MakeSpecLink("extensibility.html#Extension");
            else if (typename == "Meta")
                return MakeSpecLink("resource.html#Meta");
            else if (typename == "Narrative")
                return MakeSpecLink("narrative.html#Narrative");
            else if (typename == "Resource")
                return MakeSpecLink("resource.html");
            else if (typename == "Reference")
                return MakeSpecLink("references.html#Reference");
            else
                return null;
        }


        public bool HasLinkForCoreTypeDocu(string typename)
        {
            return generateCoreTypeLink(typename) != null;
        }

        //internal model.valueset getvalueset(string url)
        //{
        //    if (!url.startswith("http")) url = "http://local/" + url;
        //    return _loader.artifactsource.readresourceartifact(new uri(url)) as valueset;
        //}





        //internal string GetLinkForExtensionDefinition(Profile profile, Profile.ProfileExtensionDefnComponent extension)       
        //{
        //    return GetLinkForExtensionDefinition(profile, extension.Code);
        //}

       
        //internal string GetLinkForElementDefinition(Profile.ProfileStructureComponent s, Profile profile, Profile.ElementComponent element)
        //{
        //    return GetLinkForProfileDict(profile) + "#" + MakeElementDictAnchor(s,element);
        //}

        //internal string GetLinkForLocalStructure(Profile profile, Profile.ProfileStructureComponent structure)
        //{
        //    return GetLinkForLocalStructure(profile, structure.Name);
        //}

        //internal string GetLinkForLocalStructure(Profile profile, string name)
        //{
        //    return GetProfilePageName(profile) + "-" + TokenizeName(name).ToLower() + ".html";
        //}






        //public string GetLinkForProfileTable(Profile profile)
        //{
        //    return GetProfilePageName(profile) + ".html";
        //}




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

        //public const string V2_SYSTEM_PREFIX = "http://hl7.org/fhir/v2/";
        //public const string V3_SYSTEM_PREFIX = "http://hl7.org/fhir/v3/";
        //public const string FHIR_SYSTEM_PREFIX = "http://hl7.org/fhir/";

        //internal ValueSet GetValueSetForSystem(string system)
        //{
        //    string valuesetUri = null;

        //    if (system.StartsWith(V2_SYSTEM_PREFIX))
        //        valuesetUri = V2_SYSTEM_PREFIX + "vs/" + system.Substring(V2_SYSTEM_PREFIX.Length);
        //    else if (system.StartsWith(V3_SYSTEM_PREFIX))
        //        valuesetUri = V3_SYSTEM_PREFIX + "vs/" + system.Substring(V3_SYSTEM_PREFIX.Length);
        //    else if (system.StartsWith(FHIR_SYSTEM_PREFIX))
        //        valuesetUri = FHIR_SYSTEM_PREFIX + "vs/" + system.Substring(FHIR_SYSTEM_PREFIX.Length);

        //    if (valuesetUri != null)
        //        return GetValueSet(valuesetUri);
        //    else
        //        return null;
        //}

        public string ImageOutputDirectory { get; set; }
        public string GenImageUrl { get; set; }
        public string DistUrl { get; set; }
        
        /**
         * There are circumstances where the table has to present in the absence of a stable supporting infrastructure.
         * and the file paths cannot be guaranteed. For these reasons, you can tell the builder to inline all the graphics
         * (all the styles are inlined anyway, since the table fbuiler has even less control over the styling
         *  
         */
        public bool InlineGraphics { get; set; }

        public string GetGenImagePath(string p)
        {
            return Path.Combine(ImageOutputDirectory, p);
        }

        public string GetGenImageLink(string p)
        {
            return GenImageUrl + p;
        }

        public string GetDistImageLink(string filename)
        {
            return  DistUrl + "images/" + filename;
        }

        private static string verifyEndSlash(string filename)
        {
            if (filename.EndsWith("/")) return filename;

            return filename + "/";
        }

        public bool StandAlone { get; set; }

        public IEnumerable<ConceptMap> GetConceptMaps()
        {
            //Note: we assume the ArtifactSource caches the conceptmaps. Otherwise this is expensive.

            var conceptMapUrls = _source.ListConformanceResources().Where(info => info.Type == ResourceType.ConceptMap).Select(info => info.Url);

            return conceptMapUrls.Select(url => (ConceptMap)_source.ReadConformanceResource(url));
        }

        public IEnumerable<ConceptMap> GetConceptMapsForSource(string uri)
        {
            return GetConceptMaps().Where(cm => cm.SourceAsString() == uri);
        }

        public IEnumerable<ConceptMap> GetConceptMapsForSource(ValueSet source)
        {
            return GetConceptMapsForSource(source.Url);
        }

        public IEnumerable<ConceptMap> GetConceptMapsForSource(StructureDefinition source)
        {
            return GetConceptMapsForSource(source.Url);
        }

        public ValueSet GetValueSet(string url)
        {
            return _source.ReadConformanceResource(url) as ValueSet;
        }
    }

}
