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
using System.Net;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Navigation;
using Hl7.Fhir.Support;
using MarkdownDeep;

namespace Hl7.Fhir.Publication
{
    internal class ValueSetGenerator
    {
        ProfileKnowledgeProvider _pkp;

        protected string OutputDir;
        protected bool InlineGraphics;

        private StringBuilder xhtml = new StringBuilder();

        internal ValueSetGenerator(String outputDirectory, ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
            OutputDir = outputDirectory;
        }


        public XElement generate(ValueSet vs)
        {
            var result = new XElement(XmlNs.XHTMLNS + "div");

            if (vs.Expansion != null)
            {
                if (vs.Define == null && vs.Compose == null)
                {
                    throw new NotImplementedException("Expansion HTML generation not yet supported");
                    //generateExpansion(x, vs);
                }
                else
                    throw new Exception("Error: should not encounter value set expansion at this point");
            }

            bool hasExtensions = false;

            if (vs.Define != null)
                hasExtensions = generateDefinition(result, vs);

            if (vs.Compose != null)
                hasExtensions = generateComposition(result, vs) || hasExtensions;

            //inject(vs, result, hasExtensions ? NarrativeStatus.EXTENSIONS :  NarrativeStatus.GENERATED);

            return result;
        }


        private bool generateDefinition(XElement x, ValueSet vs)
        {
            bool hasExtensions = false;
            var mymaps = new Dictionary<ConceptMap, String>();

            //TODO: Add ConceptMap infrastructure
            //for (AtomEntry<ConceptMap> a : context.getMaps().values()) 
            //{
            //    if (((Reference) a.getResource().getSource()).getReference().equals(vs.getIdentifier())) 
            //    {
            //        String url = "";

            //        if (context.getValueSets().containsKey(((Reference) a.getResource().getTarget()).getReference()))
            //            url = context.getValueSets().get(((Reference) a.getResource().getTarget()).getReference()).getLinks().get("path");
            //        mymaps.put(a.getResource(), url);
            //    }
            //}

            var langs = new List<String>();

            x.Add(new XElement(XmlNs.XHTMLNS + "h2"), new XText(vs.Name));
            var p = new XElement(XmlNs.XHTMLNS + "p");
            smartAddText(p, vs.Description);
            x.Add(p);

            if (vs.Copyright != null) generateCopyright(x, vs);

            x.Add(new XElement(XmlNs.XHTMLNS + "p", "This value set defines its own terms in the system " + vs.Define.System));

            var t = new XElement(XmlNs.XHTMLNS + "table", new XAttribute("class", "codes"));
            x.Add(t);

            bool commentS = false;
            bool deprecated = false;

            if (vs.Define.Concept != null)
            {
                //    foreach (var c in vs.Define.Concept)
                //    {
                //      commentS = commentS || conceptsHaveComments(c);
                //      deprecated = deprecated || conceptsHaveDeprecated(c);
                //      scanLangs(c, langs);
                //    }


                //addMapHeaders(addTableHeaderRowStandard(t, commentS, deprecated), mymaps); replaced by next line:
                addTableHeaderRowStandard(t, commentS, deprecated);

                foreach (var c in vs.Define.Concept)
                    hasExtensions = addDefineRowToTable(t, c, 0, commentS, deprecated, mymaps) || hasExtensions;
            }

            //TODO: Add language infrastructure
            //if (langs.size() > 0) 
            //{
            //    Collections.sort(langs);
            //    x.addTag("p").addTag("b").addText("Additional Language Displays");
            //    t = x.addTag("table").setAttribute("class", "codes");
            //    XhtmlNode tr = t.addTag("tr");
            //    tr.addTag("td").addTag("b").addText("Code");

            //    for (String lang : langs)
            //        tr.addTag("td").addTag("b").addText(lang);

            //    for (ConceptDefinitionComponent c : vs.getDefine().getConcept()) 
            //    {
            //        addLanguageRow(c, t, langs);
            //    }
            //}    

            return hasExtensions;
        }


        private XElement addTableHeaderRowStandard(XElement t, bool comments, bool deprecated)
        {
            var tr = new XElement(XmlNs.XHTMLNS + "tr"); t.Add(tr);

            var td = new XElement(XmlNs.XHTMLNS + "td", new XElement(XmlNs.XHTMLNS + "b", new XText("Code")));
            tr.Add(tr);

            td = new XElement(XmlNs.XHTMLNS + "td", new XElement(XmlNs.XHTMLNS + "b", new XText("Display")));
            tr.Add(tr);

            td = new XElement(XmlNs.XHTMLNS + "td", new XElement(XmlNs.XHTMLNS + "b", new XText("Definition")));
            tr.Add(tr);

            if (deprecated)
            {
                td = new XElement(XmlNs.XHTMLNS + "td", new XElement(XmlNs.XHTMLNS + "b", new XText("Deprecated")));
                tr.Add(tr);
            }

            if (comments)
            {
                td = new XElement(XmlNs.XHTMLNS + "td", new XElement(XmlNs.XHTMLNS + "b", new XText("Comments")));
                tr.Add(tr);
            }

            return tr;
        }



        private bool addDefineRowToTable(XElement t, ValueSet.ValueSetDefineConceptComponent c, int i, bool comment, bool deprecated, Dictionary<ConceptMap, String> maps)
        {
            bool hasExtensions = false;

            XElement tr = new XElement(XmlNs.XHTMLNS + "tr"); t.Add(tr);

            XElement td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);
            var indent = new String('.', i * 2);
            td.Add(new XText(indent + c.Code));
            td.Add(new XElement(XmlNs.XHTMLNS + "a", new XAttribute("name", nmtokenize(c.Code)), new XText(" ")));

            td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);
            if (c.Display != null) td.Add(new XText(c.Display));

            td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);
            if (c.Definition != null) smartAddText(td, c.Definition);

            if (deprecated)
            {
                td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);
                var s = c.GetDeprecated();

                if (s != null)
                {
                    smartAddText(td, s);
                    hasExtensions = true;
                }
            }

            if (comment)
            {
                td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);
                var s = c.GetComment();

                if (s != null)
                {
                    smartAddText(td, s);
                    hasExtensions = true;
                }
            }


            //for (ConceptMap m : maps.keySet()) 
            //{
            //    td = tr.addTag("td");
            //    List<ConceptMapElementMapComponent> mappings = findMappingsForCode(c.getCode(), m);
            //    boolean first = true;
            //    for (ConceptMapElementMapComponent mapping : mappings) 
            //    {
            //        if (!first)
            //            td.addTag("br");
            //        first = false;
            //        XhtmlNode span = td.addTag("span");
            //        span.setAttribute("title", mapping.getEquivalence().toString());
            //        span.addText(getCharForEquivalence(mapping));
            //        a = td.addTag("a");
            //        a.setAttribute("href", prefix+maps.get(m)+"#"+mapping.getCode());
            //        a.addText(mapping.getCode());
            //        if (!Utilities.noString(mapping.getComments()))
            //          td.addTag("i").addText("("+mapping.getComments()+")");
            //    }
            //}

            foreach (var e in c.GetSubsumes())
            {
                hasExtensions = true;
                tr = new XElement(XmlNs.XHTMLNS + "tr"); t.Add(tr);
                td = new XElement(XmlNs.XHTMLNS + "td"); tr.Add(td);

                indent = new String('.', i * 2);
                td.Add(new XText(indent));

                var a = new XElement(XmlNs.XHTMLNS + "a");
                a.Add(new XAttribute("href", "#" + nmtokenize(e.Value)));
                a.Add(new XText(c.Code));
                td.Add(a);
            }

            foreach (var cc in c.Concept)
            {
                hasExtensions = addDefineRowToTable(t, cc, i + 1, comment, deprecated, maps) || hasExtensions;
            }

            return hasExtensions;
        }



        public static String nmtokenize(String cs)
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


        private bool generateComposition(XElement x, ValueSet vs)
        {
            bool hasExtensions = false;

            if (vs.Define == null)
            {
                var h = new XElement(XmlNs.XHTMLNS + "h2", new XText(vs.Name));
                var p = new XElement(XmlNs.XHTMLNS + "p");
                smartAddText(p, vs.Description);
                x.Add(h, p);

                if (vs.Copyright != null)
                    generateCopyright(x, vs);

                x.Add(new XElement(XmlNs.XHTMLNS + "p", "This value set includes codes defined in other code systems, using the following rules:"));
            }
            else
            {
                x.Add(new XElement(XmlNs.XHTMLNS + "p", "In addition, this value set includes codes defined in other code systems, using the following rules:"));
            }

            var ul = new XElement(XmlNs.XHTMLNS + "ul");
            x.Add(ul);

            XElement li;

            if (vs.Compose.Import != null)
            {
                foreach (var imp in vs.Compose.Import)
                {
                    li = new XElement(XmlNs.XHTMLNS + "li");
                    ul.Add(li);

                    li.Add(new XText("Import all the codes that are part of "));
                    AddVsRef(imp, li);
                }
            }

            if (vs.Compose.Include != null)
            {
                foreach (var inc in vs.Compose.Include)
                    hasExtensions = genInclude(ul, inc, "Include") || hasExtensions;
            }

            if (vs.Compose.Exclude != null)
            {
                foreach (var exc in vs.Compose.Exclude)
                    hasExtensions = genInclude(ul, exc, "Exclude") || hasExtensions;
            }

            return hasExtensions;
        }


       private bool genInclude(XElement ul, ValueSet.ConceptSetComponent inc, String type) 
       {
            bool hasExtensions = false;
            var li = new XElement(XmlNs.XHTMLNS + "li");  ul.Add(li);

            var e = _pkp.GetValueSet(inc.System);

        //    AtomEntry<? extends Resource> e = context.getCodeSystems().get(inc.getSystem());
    
            if ( (inc.Code == null || !inc.Code.Any()) &&  inc.Filter == null || !inc.Filter.Any())
            { 
                li.Add(new XText(type+" all codes defined in "));
                addCsRef(inc, li, e);
            } 
            else 
            { 
                if (inc.CodeElement != null && inc.CodeElement.Any())
                {
                    li.Add(new XText(type+" these codes as defined in "));
                    addCsRef(inc, li, e);
      
                    var t = new XElement(XmlNs.XHTMLNS+"table");  li.Add(t);
                    bool hasComments = false;
                    
                    foreach (var c in inc.CodeElement) 
                    {
                        hasComments = hasComments || c.GetExtension(ToolingExtensions.EXT_COMMENT) != null;
                    }
        
                    if (hasComments)
                        hasExtensions = true;
        
                    addTableHeaderRowStandard(t, hasComments, false);

                    foreach(var c in inc.CodeElement) 
                    {
                        var tr = new XElement(XmlNs.XHTMLNS + "tr"); t.Add(tr);
                        tr.Add(new XElement(XmlNs.XHTMLNS+"td", new XText(c.Value)));
                        
                        ValueSet.ValueSetDefineConceptComponent cc = getConceptForCode(e, c.Value, inc.System);
          
                        XElement td = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);
                        if (cc != null && !String.IsNullOrEmpty(cc.Display))
                            td.Add(new XText(cc.Display));
                        
                        //if (!Utilities.noString(c.getDisplay()))  DSTU2
                        //    td.addText(c.getDisplay());
                        //else if (cc != null && !Utilities.noString(cc.getDisplay()))
                        //    td.addText(cc.getDisplay());
          
                        td = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);

                        if (c.GetExtension(ToolingExtensions.EXT_DEFINITION) != null)
                            smartAddText(td, ToolingExtensions.ReadStringExtension(c, ToolingExtensions.EXT_DEFINITION));
                        else if (cc != null && !String.IsNullOrEmpty(cc.Definition))
                            smartAddText(td, cc.Definition);
                        else
                            ; // No else in the java code!!

                        if (c.GetExtension(ToolingExtensions.EXT_COMMENT) != null) 
                        {
                            var tdn = new XElement(XmlNs.XHTMLNS+"td"); tr.Add(td);
                            smartAddText(tdn, "Note: "+ ToolingExtensions.ReadStringExtension(c, ToolingExtensions.EXT_COMMENT));
                        }
                    }
                }

                foreach (var f in inc.Filter) 
                {                    
                    li.Add(new XText(type+" codes from "));
                    addCsRef(inc, li, e);

                    // TODO: Java code does not allow for f.Op to be null, but it is optional
                    li.Add(new XText(" where "+f.Property+" "+describe(f.Op.GetValueOrDefault())+" "));
                    if (e != null && codeExistsInValueSet(e, f.Value)) 
                    {
                        li.Add(new XElement(XmlNs.XHTMLNS+"a",
                            new XText(f.Value), new XAttribute("href", prefix+getCsRef(inc.System)+"#"+nmtokenize(f.Value))));
                    } 
                    else
                        li.Add(new XText(f.Value));
                
                    String disp = f.getDisplayHint();
                    if (disp != null)
                        li.Add(new XText(" ("+disp+")"));
                }
            }
    
           return hasExtensions;
       }

       private void addCsRef(ValueSet.ConceptSetComponent inc, XElement li, ValueSet cs)
       {
            String reference = null;
    
           if (cs != null) 
           {
               reference = inc.System;
           }       

           if (cs != null && reference != null) 
           {
            if (!String.IsNullOrEmpty(prefix) && reference.StartsWith("http://hl7.org/fhir/"))
                reference = reference.Substring(20)+"/index.html";
        
               XElement a =new XElement(XmlNs.XHTMLNS+"a"); li.Add(a);
                a.Add(new XAttribute("href", prefix+reference.Replace("\\", "/")));
      
               a.Add(new XText(inc.System));
           }
           else 
            li.Add(new XText(inc.System));
       }
       

       private string getCsRef(string p)
       {
           //TODO: do something smart here
           return p;
       }


       private ValueSet.ValueSetDefineConceptComponent getConceptForCode(ValueSet vs, String code, String system)
       {
           //TODO: Terminologie services oproepen als de valueset onbekend is
           //if (e == null) {
           //  if (context.getTerminologyServices() != null)
           //    return context.getTerminologyServices().getCodeDefinition(system, code);
           //  else
           //    return null;
           //}    
           //ValueSet vs = (ValueSet) e.getResource();

           if (vs.Define == null)
               return null;

           foreach (var c in vs.Define.Concept)
           {
               var v = getConceptForCode(c, code);

               if (v != null)
                   return v;
           }

           return null;
       }


       private ValueSet.ValueSetDefineConceptComponent getConceptForCode(ValueSet.ValueSetDefineConceptComponent c, String code)
       {
           if (code == c.Code)
               return c;

           if (c.Concept == null) return null;

           foreach (var cc in c.Concept)
           {
               var v = getConceptForCode(cc, code);
               if (v != null)
                   return v;
           }
           return null;
       }


       private bool codeExistsInValueSet(ValueSet vs, String code)
       {
           if (vs.Define == null || vs.Define.Concept == null) return false;

           foreach (var c in vs.Define.Concept)
           {
               if (inConcept(code, c))
                   return true;
           }
           return false;
       }


       private bool inConcept(String code, ValueSet.ValueSetDefineConceptComponent c)
       {
           if (c.Code == code)
               return true;

           foreach (var g in c.Concept)
           {
               if (inConcept(code, g))
                   return true;
           }

           return false;
       }

       private string prefix = "http://nos.nl";

        private void generateCopyright(XElement x, ValueSet vs)
        {
            var p = new XElement(XmlNs.XHTMLNS + "p", new XElement(XmlNs.XHTMLNS + "b", new XText("Copyright Statement:")));
            smartAddText(p, " " + vs.Copyright);
            x.Add(p);
        }

        private String describe(ValueSet.FilterOperator opSimple)
        {
            switch (opSimple)
            {
                case ValueSet.FilterOperator.Equal: return " = ";
                case ValueSet.FilterOperator.IsA: return " is-a ";
                case ValueSet.FilterOperator.IsNotA: return " is-not-a ";
                case ValueSet.FilterOperator.Regex: return " matches (by regex) ";
                case ValueSet.FilterOperator.In: return " in ";
                case ValueSet.FilterOperator.NotIn: return " not in ";
            }
            return null;
        }


        private void smartAddText(XElement p, String text)
        {
            if (text == null)
                return;

            String[] lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0) p.Add(new XElement(XmlNs.XHTMLNS + "br"));
                p.Add(new XText(lines[i]));
            }
        }

        private void AddVsRef(String value, XElement li)
        {
            var a = new XElement(XmlNs.XHTMLNS + "a");
            a.Add(new XAttribute("href", value), new XText(value));
            li.Add(a);

            //AtomEntry<? extends Resource> vs = context.getValueSets().get(value);
            //if (vs == null) 
            //  vs = context.getCodeSystems().get(value); 
            //if (vs != null) {
            //  String ref= vs.getLinks().get("path");
            //  XhtmlNode a = li.addTag("a");
            //  a.setAttribute("href", prefix+ref.replace("\\", "/"));
            //  a.addText(value);
            //} else if (value.equals("http://snomed.info/sct") || value.equals("http://snomed.info/id")) {
            //  XhtmlNode a = li.addTag("a");
            //  a.setAttribute("href", value);
            //  a.addText("SNOMED-CT");      
            //}
            //else 
            //  li.addText(value);
        }
    }
}

#if ORIGINAL_JAVA_CODE
 /**
   * This generate is optimised for the FHIR build process itself in as much as it 
   * generates hyperlinks in the narrative that are only going to be correct for
   * the purposes of the build. This is to be reviewed in the future.
   *  
   * @param vs
   * @param codeSystems
   * @throws Exception
   */
  public void generate(ValueSet vs) throws Exception {
    XhtmlNode x = new XhtmlNode(NodeType.Element, "div");
    if (vs.getExpansion() != null) {
      if (vs.getDefine() == null && vs.getCompose() == null)
        generateExpansion(x, vs);
      else
        throw new Exception("Error: should not encounter value set expansion at this point");
    }
    boolean hasExtensions = false;
    if (vs.getDefine() != null)
      hasExtensions = generateDefinition(x, vs);
    if (vs.getCompose() != null) 
      hasExtensions = generateComposition(x, vs) || hasExtensions;
    inject(vs, x, hasExtensions ? NarrativeStatus.EXTENSIONS :  NarrativeStatus.GENERATED);
  }

	private boolean generateComposition(XhtmlNode x, ValueSet vs) throws Exception {
	  boolean hasExtensions = false;
    if (vs.getDefine() == null) {
      XhtmlNode h = x.addTag("h2");
      h.addText(vs.getName());
      XhtmlNode p = x.addTag("p");
      smartAddText(p, vs.getDescription());
      if (vs.getCopyright() != null)
        generateCopyright(x, vs);
      p = x.addTag("p");
      p.addText("This value set includes codes defined in other code systems, using the following rules:");
    } else {
      XhtmlNode p = x.addTag("p");
      p.addText("In addition, this value set includes codes defined in other code systems, using the following rules:");

    }
    XhtmlNode ul = x.addTag("ul");
    XhtmlNode li;
    for (UriType imp : vs.getCompose().getImport()) {
      li = ul.addTag("li");
      li.addText("Import all the codes that are part of ");
      AddVsRef(imp.getValue(), li);
    }
    for (ConceptSetComponent inc : vs.getCompose().getInclude()) {
      hasExtensions = genInclude(ul, inc, "Include") || hasExtensions;      
    }
    for (ConceptSetComponent exc : vs.getCompose().getExclude()) {
      hasExtensions = genInclude(ul, exc, "Exclude") || hasExtensions;      
    }
    return hasExtensions;
  }

	private void smartAddText(XhtmlNode p, String text) {
	  if (text == null)
	    return;
	  
    String[] lines = text.split("\\r\\n");
    for (int i = 0; i < lines.length; i++) {
      if (i > 0)
        p.addTag("br");
      p.addText(lines[i]);
    }
  }


#endif
