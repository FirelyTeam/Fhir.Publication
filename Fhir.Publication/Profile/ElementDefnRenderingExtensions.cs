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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Hl7.Fhir.Publication
{
    public static class ElementDefnRenderingExtensions
    {
        public static string DescribeCardinality(this Profile.ElementDefinitionComponent defn)
        {
            if (defn.Max == null || defn.Max == "-1")
                return defn.Min.ToString() + "..*";
            else
                return defn.Min.ToString() + ".." + defn.Max;
        }

        public static string DescribeTypeCode(this Profile.ElementDefinitionComponent defn)
        {
            return String.Join(" | ", defn.Type.Select(tr => tr.Code));
        }

        public static string DescribeContext(this Profile.ProfileExtensionDefnComponent ext)
        {
            return String.Join(", ", ext.Context);
        }

        internal static string ForHtml(this Profile.ElementDefinitionBindingComponent binding, ProfileKnowledgeProvider pkp = null)
        {
            if (binding.Reference == null || pkp == null) 
                return binding.Description;

            var reference = binding.Reference is FhirUri ? ((FhirUri)binding.Reference).Value 
                    : ((ResourceReference)binding.Reference).Reference;

            var vs = pkp.GetValueSet(reference);

            if (vs != null)
                return binding.Description +  "<br/>" + conf(binding) + "<a href=\"" + reference + "\">"+ vs.Name + "</a>" +confTail(binding);
            
            if (reference.StartsWith("http:") || reference.StartsWith("https:"))
                return binding.Description + "<br/>" + conf(binding) + " <a href=\""+reference+"\">"+reference+"</a>"+ confTail(binding);
            else
                return binding.Description + "<br/>" + conf(binding)+" ?? Broken Reference to "+reference+" ??" + confTail(binding);

        }


        private static String conf(Profile.ElementDefinitionBindingComponent def) 
        {
            if (def.Conformance == null)
                return "For codes, see ";

            switch (def.Conformance)
            {
                case Profile.BindingConformance.Example:
                    return "For example codes, see ";
                case Profile.BindingConformance.Preferred:
                    return "The codes SHOULD be taken from ";
                case Profile.BindingConformance.Required:
                    return "The codes SHALL be taken from ";
                default:
                    return "??";
            }
        }

        private static String confTail(Profile.ElementDefinitionBindingComponent def) 
        {
            //TODO: Note: I think the Java implmentation assumes a default of "false" here for IsExtensible...
            if (def.Conformance == Profile.BindingConformance.Preferred || (def.Conformance == Profile.BindingConformance.Required && def.IsExtensible.GetValueOrDefault(false)))
                return "; other codes may be used where these codes are not suitable";
            else
             return "";
        }

  


        public static string ForDisplay(this CodeableConcept value)
        {
            var result = value.Text;

            if (value.Coding != null && value.Coding.Count != 0)
            {
                if (!String.IsNullOrEmpty(result)) result += " ";
                result += "{" + String.Join(", ", value.Coding.Select(cod => cod.ForDisplay())) + "}";
            }

            return result;
        }

        public static string ForDisplay(this Coding value)
        {
            var result = value.Code;

            if(value.System != null) result += "@" + value.System;
            if(value.Display != null) result += " (" + value.Display + ")";
            return result;
        }

        public static string ForDisplay(this Identifier value)
        {
            var result = value.Value;

            if(value.System != null) result += "@" + value.System;

            return result;
        }

        public static string ForDisplay(this Period value)
        {
            return String.Format("[{0},{1}]", value.Start ?? "\u221E", value.End ?? "\u221E");
        }


        public static string ForDisplay(this Range value)
        {
            return String.Format("[{0},{1}]", value.Low == null ? "\u221E" : value.Low.ForDisplay(), 
                value.High == null ? "\u221E" : value.High.ForDisplay());
        }

        public static string ForDisplay(this Quantity value)
        {
            string comp = value.Comparator==null ? String.Empty : value.Comparator.ConvertTo<string>();          
            var val = value.Value == null ? "??" : value.Value.ConvertTo<string>();
            var unit = value.Units != null ? (value.Units.Length > 2 ? " " : "") + value.Units : value.Code;

            return comp+val+unit;
        }

        public static string ForDisplay(this Ratio value)
        {
            if (value.Numerator!=null && value.Denominator!=null)
                return value.Numerator.ForDisplay() + " : " + value.Denominator.ForDisplay();
            else
                return String.Empty;
        }

        public static string ForDisplay(this Element value)
        {
            if (value is FhirBoolean) return ((FhirBoolean)value).Value.ConvertTo<string>();
            if (value is Integer) return ((Integer)value).Value.ConvertTo<string>();
            if (value is FhirDecimal) return ((FhirDecimal)value).Value.ConvertTo<string>();
            if (value is Base64Binary) return Convert.ToBase64String(((Base64Binary)value).Value);
            if (value is Instant) return ((Instant)value).Value.ConvertTo<string>();
            if (value is FhirString) return ((FhirString)value).Value;            
            if (value is FhirUri) return ((FhirUri)value).Value;
            if (value is Date) return ((Date)value).Value;
            if (value is FhirDateTime) return ((FhirDateTime)value).Value;
                       
            if (value is Code) return ((Code)value).Value;
            if (value is Oid) return ((Oid)value).Value;
            if (value is Uuid) return ((Uuid)value).Value;
            if (value is Id) return ((Id)value).Value;

            return String.Format("({0} todo)", value.GetType().Name);
        }
    }
}
