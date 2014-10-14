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
using MarkdownDeep;

namespace Hl7.Fhir.Publication
{
    internal class DictHtmlGenerator
    {
        ProfileKnowledgeProvider _pkp;

        protected string OutputDir;
        protected bool InlineGraphics;

        private StringBuilder xhtml = new StringBuilder();

        internal DictHtmlGenerator(String outputDirectory, ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
            OutputDir = outputDirectory;
        }


        public XElement generate(Profile profile, string profileUrl)
        {
            write("<div xmlns=\"" + Hl7.Fhir.Support.XmlNs.XHTML + "\">");

            if (profile.ExtensionDefn != null && profile.ExtensionDefn.Any())
            {
                write("<p><a name=\"i0\"><b>Extensions</b></a></p>\r\n");
                write("<table class=\"dict\">\r\n");

                foreach (var e in profile.ExtensionDefn)
                {
                    generateExtension(profile, e);
                }

                write("</table>\r\n");
            }

            //if(profile.Structure != null && profile.Structure.Any())
            int i = 1;

            foreach (var s in profile.Structure)
            {
                generateStructure(profile, profileUrl, i, s);
                i++;
            }

            write("</div>");
            return XElement.Parse(xhtml.ToString());
        }


        private void write(string s)
        {
            xhtml.Append(s);
        }

        private void generateExtension(Profile profile, Profile.ProfileExtensionDefnComponent e)
        {
            write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"extension." + e.Code + "\"> </a><b>Extension " + e.Code + "</b></td></tr>\r\n");
            generateElementInner(profile, e.Definition);

            // DSTU2
            //       if (e.Element.size() > 1) {
            //for (int i = 1; i < e.Element.size(); i++) {
            //  ElementComponent ec = e.Element.get(i);
            //  write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"extension."+ec.getPath+"\"> </a><b>&nbsp;"+ec.getPath+"</b></td></tr>\r\n");
            //  generateElementInner(profile, ec.Definition);
            //      }
        }

        private void generateElementInner(Profile profile, Profile.ElementDefinitionComponent d)
        {
            tableRowMarkdown("Definition", d.Formal);
            tableRow("Control", _pkp.MakeSpecRef("conformance-rules.html#conformance"), d.DescribeCardinality() + summariseConditions(d.Condition));
            tableRowNE("Binding", "terminologies.html", describeBinding(d));
            if (d.NameReference != null)
                tableRow("Type", null, "See " + d.NameReference);
            else
                tableRowNE("Type", "datatypes.html", describeTypes(d.Type));
            tableRow("Is Modifier", "conformance-rules.html#ismodifier", displayBoolean(d.IsModifier));
            tableRow("Must Support", "conformance-rules.html#mustSupport", displayBoolean(d.MustSupport));
            tableRowMarkdown("Requirements", d.Requirements);
            tableRow("Aliases", null, d.Synonym != null ? String.Join(", ", d.Synonym) : null);
            tableRowMarkdown("Comments", d.Comments);
            tableRow("Max Length", null, d.MaxLength == null ? null : d.MaxLength.ToString());
            tableRow("Fixed Value", null, d.Value != null ? d.Value.ForDisplay() : null);
            tableRow("Example", null, d.Example != null ? d.Example.ForDisplay() : null);
            tableRowNE("Invariants", null, describeInvariants(d.Constraint));
            tableRow("LOINC Code", null, getMapping(profile, d, LOINC_MAPPING));
            tableRow("SNOMED-CT Code", null, getMapping(profile, d, SNOMED_MAPPING));
        }

        internal const string RIM_MAPPING = "http://hl7.org/v3";
        internal const string v2_MAPPING = "http://hl7.org/v2";
        internal const string LOINC_MAPPING = "http://loinc.org";
        internal const string SNOMED_MAPPING = "http://snomed.info";

        private string summariseConditions(IEnumerable<string> conditions)
        {
            // TODO: Not implemented yet
            if (conditions == null || !conditions.Any())
                return string.Empty;
            else
                return " ?";
        }

        private String describeBinding(Profile.ElementDefinitionComponent d)
        {
            if (d.Binding == null)
                return null;
            else
                return d.Binding.ForHtml(_pkp);
        }

        private String describeTypes(List<Profile.TypeRefComponent> types)
        {
            if (types == null || !types.Any()) return null;

            if (types.Count == 1)
                return describeType(types[0]);
            else
            {
                return "Choice of: " +
                    String.Join(", ", types.Select(t => describeType(t)));
            }
        }

        private string describeType(Profile.TypeRefComponent t)
        {
            StringBuilder b = new StringBuilder();

            b.Append("<a href=\"");
            b.Append(_pkp.getLinkFor(t.Code));
            b.Append("\">");
            b.Append(t.Code);
            b.Append("</a>");

            if (t.Profile != null)
            {
                b.Append("<a href=\"todo.html\">");
                b.Append("(Profile = " + t.Profile + ")");
                b.Append("</a>");
            }

            return b.ToString();
        }

        private String displayBoolean(bool? value)
        {
            if (value.HasValue && value == true)
                return "true";
            else
                return null;
        }

        private String describeInvariants(List<Profile.ElementDefinitionConstraintComponent> constraints)
        {
            if (constraints == null || !constraints.Any())
                return null;

            StringBuilder s = new StringBuilder();

            if (constraints.Count > 0)
            {
                s.Append("<b>Defined on this element</b><br/>\r\n");

                var b = false;

                foreach (var inv in constraints.OrderBy(constr => constr.Key))
                {
                    if (b)
                        s.Append("<br/>");
                    else
                        b = true;

                    s.Append("<b title=\"Formal Invariant Identifier\">Inv-" + inv.Key + "</b>: " + WebUtility.HtmlEncode(inv.Human) +
                            " (xpath: " + WebUtility.HtmlEncode(inv.Xpath) + ")");
                }
            }

            return s.ToString();
        }


        private String getMapping(Profile profile, Profile.ElementDefinitionComponent d, String uri)
        {
            if (profile.Mapping == null) return null;
            if (d.Mapping == null) return null;

            String id = profile.Mapping.Where(map => map.Uri == uri).Select(map => map.Identity).FirstOrDefault();
            if (id == null) return null;

            return d.Mapping.Where(map => map.Identity == id).Select(map => map.Map).FirstOrDefault();
        }

        private void generateStructure(Profile profile, string profileUrl, int i, Profile.ProfileStructureComponent s)
        {
            write("<p><a name=\"i" + i.ToString() + "\"><b>" + s.Name + "</b></a></p>\r\n");
            write("<table class=\"dict\">\r\n");

            foreach (var ec in s.Element)
            {
                if (isProfiledExtension(ec))
                {
                    String name = s.Name + "." + makePathLink(ec);
                    String title = ec.Path + " (" + (ec.Definition.Type[0].Profile.StartsWith("#") ? profileUrl : "")
                            + ec.Definition.Type[0].Profile + ")";
                    write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"" + name + "\"> </a><b>" + title + "</b></td></tr>\r\n");

                    var profExtDefn = _pkp.getExtensionDefinition(profile, ec.Definition.Type[0].Profile);
                    var extDefn = ec.Definition;
                    if (profExtDefn != null) extDefn = profExtDefn.Definition;
                  
                    generateElementInner(profile, extDefn);
                }
                else
                {
                    String name = s.Name + "." + makePathLink(ec);
                    String title = ec.Path + (ec.Name == null ? "" : "(" + ec.Name + ")");
                    write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"" + name + "\"> </a><b>" + title + "</b></td></tr>\r\n");
                    generateElementInner(profile, ec.Definition);
                }
            }

            write("</table>\r\n");
        }

        private bool isProfiledExtension(Profile.ElementComponent ec)
        {
            return ec.Definition.Type != null && ec.Definition.Type.Count == 1 && 
                ec.Definition.Type[0].Code == "Extension" && 
                ec.Definition.Type[0].Profile != null;
        }

        private String makePathLink(Profile.ElementComponent element)
        {
            if (element.Name == null)
                return element.Path;
            if (!element.Path.Contains("."))
                return element.Name;
            return element.Path.Substring(0, element.Path.LastIndexOf(".")) + "." + element.Name;
        }

        private void tableRowMarkdown(String name, String value)
        {
            String text;

            if (value == null)
                text = "";
            else
            {
                text = value.Replace("||", "\r\n\r\n");
                while (text.Contains("[[["))
                {
                    String left = text.Substring(0, text.IndexOf("[[["));
                    String linkText = text.Substring(text.IndexOf("[[[") + 3, text.IndexOf("]]]"));
                    String right = text.Substring(text.IndexOf("]]]") + 3);
                    String[] parts = linkText.Split('#');

                    var url = _pkp.getLinkForProfile(null, parts[0]);

                    if (url == null)
                    {
                        url = _pkp.getLinkFor(linkText.ToLower());
                    }

                    text = left + "[" + linkText + "](" + url + ")" + right;
                }
            }

            var mark = new Markdown();

            //set preferences of your markdown
            mark.SafeMode = true;
            mark.ExtraMode = true;

            var formatted = mark.Transform(WebUtility.HtmlEncode(text));
            write("  <tr><td>" + name + "</td><td>" + formatted + "</td></tr>\r\n");
        }

        private void tableRow(String name, String defRef, String value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (defRef != null)
                    write("  <tr><td><a href=\"" + defRef + "\">" + name + "</a></td><td>" + WebUtility.HtmlEncode(value) + "</td></tr>\r\n");
                else
                    write("  <tr><td>" + name + "</td><td>" + WebUtility.HtmlEncode(value) + "</td></tr>\r\n");
            }
        }


        private void tableRowNE(String name, String defRef, String value)
        {
            if (!String.IsNullOrEmpty(value))
                if (defRef != null)
                    write("  <tr><td><a href=\"" + defRef + "\">" + name + "</a></td><td>" + value + "</td></tr>\r\n");
                else
                    write("  <tr><td>" + name + "</td><td>" + value + "</td></tr>\r\n");
        }
    }
}


#if ORIGINAL_JAVA_CODE
package org.hl7.fhir.definitions.generators.specification;
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
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import org.hl7.fhir.definitions.model.BindingSpecification;
import org.hl7.fhir.definitions.model.Definitions;
import org.hl7.fhir.definitions.model.ElementDefn;
import org.hl7.fhir.definitions.model.Invariant;
import org.hl7.fhir.definitions.model.TypeRef;
import org.hl7.fhir.instance.formats.XmlComposer;
import org.hl7.fhir.instance.model.AtomEntry;
import org.hl7.fhir.instance.model.IdType;
import org.hl7.fhir.instance.model.Profile;
import org.hl7.fhir.instance.model.Profile.ElementComponent;
import org.hl7.fhir.instance.model.Profile.ElementDefinitionComponent;
import org.hl7.fhir.instance.model.Profile.ElementDefinitionConstraintComponent;
import org.hl7.fhir.instance.model.Profile.ElementDefinitionMappingComponent;
import org.hl7.fhir.instance.model.Profile.ProfileExtensionDefnComponent;
import org.hl7.fhir.instance.model.Profile.ProfileMappingComponent;
import org.hl7.fhir.instance.model.Profile.ProfileStructureComponent;
import org.hl7.fhir.instance.model.Profile.TypeRefComponent;
import org.hl7.fhir.instance.model.StringType;
import org.hl7.fhir.instance.model.Type;
import org.hl7.fhir.tools.publisher.PageProcessor;
import org.hl7.fhir.utilities.CommaSeparatedStringBuilder;
import org.hl7.fhir.utilities.Utilities;

import com.github.rjeschke.txtmark.Processor;

public class DictHTMLGenerator  extends OutputStreamWriter {

	private Definitions definitions;
	private PageProcessor page;
	
	public DictHTMLGenerator(OutputStream out, PageProcessor page) throws UnsupportedEncodingException {
		super(out, "UTF-8");
		this.definitions = page.getDefinitions();
		this.page = page;
	}

  public void generate(Profile profile) throws Exception {
    if (!profile.getExtensionDefn().isEmpty()) {
      write("<p><a name=\"i0\"><b>Extensions</b></a></p>\r\n");
      write("<table class=\"dict\">\r\n");
      
      for (ProfileExtensionDefnComponent e : profile.getExtensionDefn()) {
         generateExtension(profile, e);
      }
      write("</table>\r\n");
      
    }
    int i = 1;
    for (ProfileStructureComponent s : profile.getStructure()) {
      write("<p><a name=\"i"+Integer.toString(i)+"\"><b>"+s.getNameSimple()+"</b></a></p>\r\n");
      write("<table class=\"dict\">\r\n");
      
      for (ElementComponent ec : s.getSnapshot().getElement()) {
        if (isProfiledExtension(ec)) {
          String name = s.getNameSimple()+"."+ makePathLink(ec);
          String title = ec.getPathSimple() + " ("+(ec.getDefinition().getType().get(0).getProfileSimple().startsWith("#") ? profile.getUrlSimple() : "")+ec.getDefinition().getType().get(0).getProfileSimple()+")";
          write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\""+name+"\"> </a><b>"+title+"</b></td></tr>\r\n");
          ElementDefinitionComponent extDefn = getExtensionDefinition(profile, ec.getDefinition().getType().get(0).getProfileSimple(), ec.getDefinition());
          generateElementInner(profile, extDefn);
        } else {
          String name = s.getNameSimple()+"."+ makePathLink(ec);
          String title = ec.getPathSimple() + (ec.getNameSimple() == null ? "" : "(" +ec.getNameSimple() +")");
          write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\""+name+"\"> </a><b>"+title+"</b></td></tr>\r\n");
          generateElementInner(profile, ec.getDefinition());
        }
      }
      write("</table>\r\n");
      i++;      
    }
    flush();
    close();
  }

  private String makePathLink(ElementComponent element) {
    if (element.getName() == null)
      return element.getPathSimple();
    if (!element.getPathSimple().contains("."))
      return element.getNameSimple();
    return element.getPathSimple().substring(0, element.getPathSimple().lastIndexOf("."))+"."+element.getNameSimple();
  }
  
  private ElementDefinitionComponent getExtensionDefinition(Profile context, String url, ElementDefinitionComponent defaultDefn) {
    String code;
    if (url.startsWith("#")) {
      code = url.substring(1);
    } else {
      String[] parts = url.split("\\#");
      code = parts[1];
      context = definitions.getProfileByURL(parts[0]);
    }
    
    for (ProfileExtensionDefnComponent ext : context.getExtensionDefn()) {
      if (ext.getCodeSimple().equals(code))
        return ext.getElement().get(0).getDefinition();
    }

    return defaultDefn;
  }

  private boolean isProfiledExtension(ElementComponent ec) {
    return ec.getDefinition().getType().size() == 1 && ec.getDefinition().getType().get(0).getCodeSimple().equals("Extension") && ec.getDefinition().getType().get(0).getProfile() != null;
  }

  private void generateExtension(Profile profile, ProfileExtensionDefnComponent e) throws Exception {
    write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"extension."+e.getCodeSimple()+"\"> </a><b>Extension "+e.getCodeSimple()+"</b></td></tr>\r\n");
    generateElementInner(profile, e.getElement().get(0).getDefinition());
    if (e.getElement().size() > 1) {
      for (int i = 1; i < e.getElement().size(); i++) {
        ElementComponent ec = e.getElement().get(i);
        write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\"extension."+ec.getPathSimple()+"\"> </a><b>&nbsp;"+ec.getPathSimple()+"</b></td></tr>\r\n");
        generateElementInner(profile, ec.getDefinition());
      }
    }
  }
    
  private void generateElementInner(Profile profile, ElementDefinitionComponent d) throws Exception {
    tableRowMarkdown("Definition", d.getFormalSimple());
    tableRow("Control", "conformance-rules.html#conformance", describeCardinality(d) + summariseConditions(d.getCondition()));
    tableRowNE("Binding", "terminologies.html", describeBinding(d));
    if (d.getNameReference() != null)
      tableRow("Type", null, "See "+d.getNameReferenceSimple());
    else
      tableRowNE("Type", "datatypes.html", describeTypes(d.getType()));
    tableRow("Is Modifier", "conformance-rules.html#ismodifier", displayBoolean(d.getIsModifierSimple()));
    tableRow("Must Support", "conformance-rules.html#mustSupport", displayBoolean(d.getMustSupportSimple()));
    tableRowMarkdown("Requirements", d.getRequirementsSimple());
    tableRow("Aliases", null, describeAliases(d.getSynonym()));
    tableRowMarkdown("Comments", d.getCommentsSimple());
    tableRow("Max Length", null, d.getMaxLength() == null ? null : Integer.toString(d.getMaxLengthSimple()));
    tableRow("Fixed Value", null, encodeValue(d.getValue()));
    tableRow("Example", null, encodeValue(d.getExample()));
    tableRowNE("Invariants", null, invariants(d.getConstraint()));
    tableRow("LOINC Code", null, getMapping(profile, d, Definitions.LOINC_MAPPING));
    tableRow("SNOMED-CT Code", null, getMapping(profile, d, Definitions.SNOMED_MAPPING));
  }

  private String encodeValue(Type value) throws Exception {
    if (value == null)
      return null;
    ByteArrayOutputStream b = new ByteArrayOutputStream();
    new XmlComposer().compose(b, value);
    return b.toString();
  }

  private String describeTypes(List<TypeRefComponent> types) throws Exception {
    if (types.isEmpty())
      return null;
    StringBuilder b = new StringBuilder();
    if (types.size() == 1)
      describeType(b, types.get(0));
    else {
      boolean first = true;
      b.append("Choice of: ");
      for (TypeRefComponent t : types) {
        if (first)
          first = false;
        else
          b.append(", ");
        describeType(b, t);
      }
    }
    return b.toString();
  }

  private void describeType(StringBuilder b, TypeRefComponent t) throws Exception {
    b.append("<a href=\"");
    b.append(GeneratorUtils.getSrcFile(t.getCodeSimple(), false));
    b.append(".html#");
    String type = t.getCodeSimple();
    if (type.equals("*"))
      b.append("open");
    else 
      b.append(t.getCodeSimple());
    b.append("\">");
    b.append(t.getCodeSimple());
    b.append("</a>");
    if (t.getProfileSimple() != null) {
      b.append("<a href=\"todo.html\">");
      b.append("(Profile = "+t.getProfileSimple()+")");
      b.append("</a>");
      
    }
  }

  private String invariants(List<ElementDefinitionConstraintComponent> constraints) {
    if (constraints.isEmpty())
      return null;
    StringBuilder s = new StringBuilder();
    if (constraints.size() > 0) {
      s.append("<b>Defined on this element</b><br/>\r\n");
      List<String> ids = new ArrayList<String>();
      for (ElementDefinitionConstraintComponent id : constraints)
        ids.add(id.getKeySimple());
      Collections.sort(ids);
      boolean b = false;
      for (String id : ids) {
        ElementDefinitionConstraintComponent inv = getConstraint(constraints, id);
        if (b)
          s.append("<br/>");
        else
          b = true;
        s.append("<b title=\"Formal Invariant Identifier\">Inv-"+id+"</b>: "+Utilities.escapeXml(inv.getHumanSimple())+" (xpath: "+Utilities.escapeXml(inv.getXpathSimple())+")");
      }
    }
    
    return s.toString();
  }

  private ElementDefinitionConstraintComponent getConstraint(List<ElementDefinitionConstraintComponent> constraints, String id) {
    for (ElementDefinitionConstraintComponent c : constraints)
      if (c.getKeySimple().equals(id))
        return c;
    return null;
  }

  private String describeAliases(List<StringType> synonym) {
    CommaSeparatedStringBuilder b = new CommaSeparatedStringBuilder();
    for (StringType s : synonym) 
      b.append(s.getValue());
    return b.toString();
  }

  private String getMapping(Profile profile, ElementDefinitionComponent d, String uri) {
    String id = null;
    for (ProfileMappingComponent m : profile.getMapping()) {
      if (m.getUriSimple().equals(uri))
        id = m.getIdentitySimple();
    }
    if (id == null)
      return null;
    for (ElementDefinitionMappingComponent m : d.getMapping()) {
      if (m.getIdentitySimple().equals(id))
        return m.getMapSimple();
    }
    return null;
  }

  private String summariseConditions(List<IdType> conditions) {
    if (conditions.isEmpty())
      return "";
    else
      return " ?";
  }

  private String describeCardinality(ElementDefinitionComponent d) {
    if (d.getMax() == null)
      return Integer.toString(d.getMinSimple()) + "..?";
    else
      return Integer.toString(d.getMinSimple()) + ".." + d.getMaxSimple();
  }

  public void generate(ElementDefn root) throws Exception
	{
		write("<table class=\"dict\">\r\n");
		writeEntry(root.getName(), "1..1", "", "", root);
		for (ElementDefn e : root.getElements()) {
		   generateElement(root.getName(), e);
		}
		write("</table>\r\n");
		write("\r\n");
		flush();
		close();
	}

	private void generateElement(String name, ElementDefn e) throws Exception {
		writeEntry(name+"."+e.getName(), e.describeCardinality(), describeType(e), e.getBindingName(), e);
		for (ElementDefn c : e.getElements())	{
		   generateElement(name+"."+e.getName(), c);
		}
	}

	private void writeEntry(String path, String cardinality, String type, String conceptDomain, ElementDefn e) throws Exception {
		write("  <tr><td colspan=\"2\" class=\"structure\"><a name=\""+path.replace("[", "_").replace("]", "_")+"\"> </a><b>"+path+"</b></td></tr>\r\n");
		tableRow("Definition", null, e.getDefinition());
		tableRow("Control", "conformance-rules.html#conformance", cardinality + (e.hasCondition() ? ": "+  e.getCondition(): ""));
		tableRowNE("Binding", "terminologies.html", describeBinding(e));
		if (!Utilities.noString(type) && type.startsWith("@"))
		  tableRowNE("Type", null, "<a href=\"#"+type.substring(1)+"\">See "+type.substring(1)+"</a>");
		else
		  tableRowNE("Type", "datatypes.html", type);
		tableRow("Is Modifier", "conformance-rules.html#ismodifier", displayBoolean(e.isModifier()));
		tableRowNE("Requirements", null, page.processMarkdown(e.getRequirements()));
    tableRow("Aliases", null, toSeperatedString(e.getAliases()));
    if (e.isSummaryItem())
      tableRow("Summary", "search.html#summary", Boolean.toString(e.isSummaryItem()));
    tableRow("Comments", null, e.getComments());
    tableRowNE("Invariants", null, invariants(e.getInvariants(), e.getStatedInvariants()));
    tableRow("LOINC Code", null, e.getMapping(Definitions.LOINC_MAPPING));
    tableRow("SNOMED-CT Code", null, e.getMapping(Definitions.SNOMED_MAPPING));
		tableRow("To Do", null, e.getTodo());
		if (e.getTasks().size() > 0) {
	    tableRowNE("gForge Tasks", null, tasks(e.getTasks()));
		}
	}
	
  private String tasks(List<String> tasks) {
    StringBuilder b = new StringBuilder();
    boolean first = true;
    for (String t : tasks) {
      if (first)
        first = false;
      else
        b.append(", ");
      b.append("<a href=\"http://gforge.hl7.org/gf/project/fhir/tracker/?action=TrackerItemEdit&amp;tracker_item_id=");
      b.append(t);
      b.append("\">");
      b.append(t);
      b.append("</a>");
    }
    return b.toString();
  }

  private String describeBinding(ElementDefn e) throws Exception {

	  if (!e.hasBinding())
	    return null;
	  
	  StringBuilder b = new StringBuilder();
	  BindingSpecification cd =  definitions.getBindingByName(e.getBindingName());
    b.append(cd.getName()+": ");
    b.append(TerminologyNotesGenerator.describeBinding(cd, page));
//    if (cd.getBinding() == Binding.Unbound)
//      b.append(" (Not Bound to any codes)");
//    else
//      b.append(" ("+(cd.getExtensibility() == null ? "--" : "<a href=\"terminologies.html#extensibility\">"+cd.getExtensibility().toString().toLowerCase())+"</a>/"+
//          "<a href=\"terminologies.html#conformance\">"+(cd.getBindingStrength() == null ? "--" : cd.getBindingStrength().toString().toLowerCase())+"</a>)");
    return b.toString();
  }

  private String describeBinding(ElementDefinitionComponent d) throws Exception {

    if (d.getBinding() == null)
      return null;
    else
      return TerminologyNotesGenerator.describeBinding(d.getBinding(), page);
  }

  private String invariants(Map<String, Invariant> invariants, List<Invariant> stated) {
	  StringBuilder s = new StringBuilder();
	  if (invariants.size() > 0) {
	    s.append("<b>Defined on this element</b><br/>\r\n");
	    List<Integer> ids = new ArrayList<Integer>();
	    for (String id : invariants.keySet())
	      ids.add(Integer.parseInt(id));
	    Collections.sort(ids);
	    boolean b = false;
	    for (Integer i : ids) {
	      Invariant inv = invariants.get(i.toString());
	      if (b)
	        s.append("<br/>");
	      s.append("<b title=\"Formal Invariant Identifier\">Inv-"+i.toString()+"</b>: "+Utilities.escapeXml(inv.getEnglish())+" (xpath: "+Utilities.escapeXml(inv.getXpath())+")");
	      b = true;
	    }
	  }
    if (stated.size() > 0) {
      if (s.length() > 0)
        s.append("<br/>");
      s.append("<b>Affect this element</b><br/>\r\n");
      boolean b = false;
      for (Invariant id : stated) {
        if (b)
          s.append("<br/>");
        s.append("<b>Inv-"+id.getId().toString()+"</b>: "+Utilities.escapeXml(id.getEnglish())+" (xpath: "+Utilities.escapeXml(id.getXpath())+")");
        b = true;
      }
    }
	  
    return s.toString();
  }

  private String toSeperatedString(List<String> list) {
	  if (list.size() == 0)
	    return "";
	  else {
	    StringBuilder s = new StringBuilder();
	    boolean first = true;
	    for (String v : list) {
	      if (!first)
	        s.append("; ");
	      first = false;
	      s.append(v);
	    }
	    return s.toString();
	  }
	  
  }

  private String displayBoolean(boolean mustUnderstand) {
		if (mustUnderstand)
			return "true";
		else
			return null;
	}

  private void tableRowMarkdown(String name, String value) throws Exception {
    String text;
    if (value == null)
      text = "";
    else {
      text = value.replace("||", "\r\n\r\n");
      while (text.contains("[[[")) {
        String left = text.substring(0, text.indexOf("[[["));
        String linkText = text.substring(text.indexOf("[[[")+3, text.indexOf("]]]"));
        String right = text.substring(text.indexOf("]]]")+3);
        String url = "";
        String[] parts = linkText.split("\\#");
        Profile p = definitions.getProfileByURL(parts[0]);
        if (p != null)
          url = p.getTag("filename")+".html";
        else if (definitions.hasReference(linkText)) {
          url = linkText.toLowerCase()+".html#";
        } else if (definitions.hasElementDefn(linkText)) {
          url = GeneratorUtils.getSrcFile(linkText, false)+".html#"+linkText;
        } else if (definitions.hasPrimitiveType(linkText)) {
          url = "datatypes.html#"+linkText;
        } else {
          System.out.println("Error: Unresolved logical URL "+linkText);
          //        throw new Exception("Unresolved logical URL "+url);
        }
        text = left+"["+linkText+"]("+url+")"+right;
      }
    }
    write("  <tr><td>"+name+"</td><td>"+Processor.process(Utilities.escapeXml(text))+"</td></tr>\r\n");
  }
	private void tableRow(String name, String defRef, String value) throws IOException {
		if (value != null && !"".equals(value)) {
		  if (defRef != null) 
	      write("  <tr><td><a href=\""+defRef+"\">"+name+"</a></td><td>"+Utilities.escapeXml(value)+"</td></tr>\r\n");
		  else
		    write("  <tr><td>"+name+"</td><td>"+Utilities.escapeXml(value)+"</td></tr>\r\n");
		}
	}

	
  private void tableRowNE(String name, String defRef, String value) throws IOException {
    if (value != null && !"".equals(value))
      if (defRef != null) 
        write("  <tr><td><a href=\""+defRef+"\">"+name+"</a></td><td>"+value+"</td></tr>\r\n");
      else
        write("  <tr><td>"+name+"</td><td>"+value+"</td></tr>\r\n");
  }


	private String describeType(ElementDefn e) throws Exception {
		StringBuilder b = new StringBuilder();
		boolean first = true;
		if (e.typeCode().startsWith("@")) {
      b.append("<a href=\"#"+e.typeCode().substring(1)+"\">See "+e.typeCode().substring(1)+"</a>");		  
		} else {
		  for (TypeRef t : e.getTypes())
		  {
		    if (!first)
		      b.append("|");
		    if (t.getName().equals("*"))
		      b.append("<a href=\"datatypes.html#open\">*</a>");
		    else
		      b.append("<a href=\""+typeLink(t.getName())+"\">"+t.getName()+"</a>");
		    if (t.hasParams()) {
		      b.append("(");
		      boolean firstp = true;
		      for (String p : t.getParams()) {
		        if (!firstp)
		          b.append(" | ");
		        if (definitions.getFutureResources().containsKey(p))
		          b.append("<span title=\"This resource is not been defined yet\">"+p+"</span>");
		        else
		          b.append("<a href=\""+typeLink(p)+"\">"+p+"</a>");
		        firstp = false;
		      }
		      b.append(")");
		    }		  first = false;
		  }
		}
		return b.toString();
	}

  private String typeLink(String name) throws Exception {
    String srcFile = GeneratorUtils.getSrcFile(name, false);
    if (srcFile.equalsIgnoreCase(name))
      return srcFile+ ".html";
    else
      return srcFile+ ".html#" + name;
  }
	
}

#endif
