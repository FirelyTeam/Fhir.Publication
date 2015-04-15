/*
Copyright (c) 2011+, HL7, Inc
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.
 * Neither the name of HL7 nor the names of its contributors may be used to 
   endorse or promote products derived from this software without specific 
   prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Publication.Profile
{
    public static class ElementDefnRenderingExtensions
    {
        public static string DescribeCardinality(this ElementDefinition defn)
        {
            if (defn.Max == null || defn.Max == "-1")
                return defn.Min.ToString() + "..*";
            else
                return defn.Min.ToString() + ".." + defn.Max;
        }

        public static string DescribeTypeCode(this ElementDefinition defn)
        {
            return String.Join(" | ", defn.Type.Select(tr => tr.Code));
        }

        //public static string DescribeContext(this ElementDefinition ext)
        //{
        //    return String.Join(", ", ext.Context);
        //}
      

        internal static string ForHtml(this ElementDefinition.ElementDefinitionBindingComponent binding, ProfileKnowledgeProvider pkp = null)
        {
            if (binding.ValueSet == null || pkp == null)
                return binding.Description;

            var reference = binding.ValueSet is FhirUri ? ((FhirUri)binding.ValueSet).Value
                    : ((ResourceReference)binding.ValueSet).Reference;

            var vs = pkp.GetValueSet(reference);

            if (vs != null)
                return binding.Description + "<br/>" + conf(binding) + "<a href=\"" + reference + "\">" + vs.Name + "</a>" + confTail(binding);

            if (reference.StartsWith("http:") || reference.StartsWith("https:"))
                return binding.Description + "<br/>" + conf(binding) + " <a href=\"" + reference + "\">" + reference + "</a>" + confTail(binding);
            else
                return binding.Description + "<br/>" + conf(binding) + " ?? Broken Reference to " + reference + " ??" + confTail(binding);

        }

        public static string bindingStrengthToCode(this ElementDefinition.ElementDefinitionBindingComponent def)
        {
            if (def.Strength == null) return "?";

            return def.Strength.ToString().ToLower();
        }

        public static string bindingStrengthToDefinition(this ElementDefinition.ElementDefinitionBindingComponent def)
        {
            if (def.Strength == null) return "?";

            switch (def.Strength)
            {
                case ElementDefinition.BindingStrength.Required: return "To be conformant, instances of this element SHALL include a code from the specified value set.";
                case ElementDefinition.BindingStrength.Extensible: return "To be conformant, instances of this element SHALL include a code from the specified value set if any of the codes within the value set can apply to the concept being communicated.  If the valueset does not cover the concept (based on human review), alternate codings (or, data type allowing, text) may be included instead.";
                case ElementDefinition.BindingStrength.Preferred: return "Instances are encouraged to draw from the specified codes for interoperability purposes but are not required to do so to be considered conformant.";
                case ElementDefinition.BindingStrength.Example: return "Instances are not expected or even encouraged to draw from the specified value set.  The value set merely provides examples of the types of concepts intended to be included.";
                default: return "?";
            }
        }


        private static String conf(ElementDefinition.ElementDefinitionBindingComponent def)
        {
            if (def.Strength == null)
                return "For codes, see ";

            switch (def.Strength)
            {
                case ElementDefinition.BindingStrength.Example:
                    return "For example codes, see ";
                case ElementDefinition.BindingStrength.Preferred:
                    return "The codes SHOULD be taken from ";
                case ElementDefinition.BindingStrength.Required:
                    return "The codes SHALL be taken from ";
                default:
                    return "??";
            }
        }

        private static String confTail(ElementDefinition.ElementDefinitionBindingComponent def)
        {
            if (def.Strength == ElementDefinition.BindingStrength.Extensible)
                return "; other codes may be used where these codes are not suitable";
            else
                return "";
        }

        public static string EncodeValue(this Element element, bool format=false)
        {
            if (element == null)
                return null;
            if (element is Primitive)
                return PrimitiveTypeConverter.ConvertTo<string>(((Primitive)element).ObjectValue);

            return buildJson(element, format);
        }

        private static String buildJson(Element value, bool format)
        {
            if (value is Primitive)
                return Hl7.Fhir.Serialization.PrimitiveTypeConverter.GetValueAsString((Primitive)value);

            var json = Hl7.Fhir.Serialization.FhirSerializer.SerializeToJson(value, root: "removeme");

            //HACK: the previous serializer call has (wrongly) generated json with a "resourceType:removeme"
            //member. Remove this member to get the kind of json we want to display in the tree
            var jsonObject = JObject.Parse(json);
            jsonObject.Remove("resourceType");

            return jsonObject.ToString(formatting:  format ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
        }


        public static bool IsNullOrEmpty(this IList list)
        {
            if (list == null) return true;

            return list.Count == 0;
        }
    }
}
