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

// Classes in this file are updated to match instance/utils/ProfileUtilities.java commit #5080 in the Java tooling trunk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Specification.Navigation;

namespace Hl7.Fhir.Publication.Profile
{
    internal class StructureTableGenerator
    {
        ProfileKnowledgeProvider _pkp;

        internal StructureTableGenerator(ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
        }

        private class UnusedTracker 
        {
		    public bool used;
	    }

        public XElement generateStructureTable(StructureDefinition structure, bool diff, bool snapshot) 
        {
            HierarchicalTableGenerator gen = new HierarchicalTableGenerator(_pkp);
            var model = TableModel.CreateNormalTable();

            List<ElementDefinition> list = diff ? structure.Differential.Element : structure.Snapshot.Element;
            List<StructureDefinition> structures = new List<StructureDefinition>();
            structures.Add(structure);

            var nav = new ElementNavigator(structure);
            nav.MoveToFirstChild();

            genElement(null, gen, model.Rows, nav, _pkp, null, snapshot);
            return gen.generate(model);    
        }


        private void genElement(string defPath, HierarchicalTableGenerator gen, List<Row> rows, ElementNavigator nav, ProfileKnowledgeProvider pkp, bool? extensions, bool snapshot)
        {
            var element = nav.Current;
            String s = element.GetNameFromPath();

            if (!snapshot && extensions != null && extensions != (s=="extension" || s=="modifierExtension"))
                return;

            if(onlyInformationIsMapping(nav)) return;  // we don't even show it in this case

            Row row = new Row();
            row.setAnchor(element.Path);
            
            bool hasDef = element.Definition != null;
            bool ext = false;
    
            if (s == "extension" || s == "modifierExtension")
            {
                row.setIcon("icon_extension_simple.png", HierarchicalTableGenerator.TEXT_ICON_EXTENSION_SIMPLE);
                ext = true;
            }
            else if (!hasDef || element.Type == null || !element.Type.Any())
            {
                row.setIcon("icon_element.gif", HierarchicalTableGenerator.TEXT_ICON_ELEMENT);
            }
            else if (hasDef && element.Type.Count > 1)
            {
                if (allTypesAre(element.Type, "Reference"))
                    row.setIcon("icon_reference.png", HierarchicalTableGenerator.TEXT_ICON_REFERENCE);
                else
                    row.setIcon("icon_choice.gif", HierarchicalTableGenerator.TEXT_ICON_CHOICE);
            }
            else if (hasDef && element.Type[0].Code.StartsWith("@"))
            {
                //TODO: That's not a legal code, will this ever appear?
                //I am pretty sure this depends on ElementDefn.NameReference
                row.setIcon("icon_reuse.png", HierarchicalTableGenerator.TEXT_ICON_REUSE);
            }
            else if (hasDef && _pkp.isPrimitive(element.Type[0].Code))
                row.setIcon("icon_primitive.png", HierarchicalTableGenerator.TEXT_ICON_PRIMITIVE);
            else if (hasDef && _pkp.isReference(element.Type[0].Code))
                row.setIcon("icon_reference.png", HierarchicalTableGenerator.TEXT_ICON_REFERENCE);
            else if (hasDef && _pkp.isDataType(element.Type[0].Code))
                row.setIcon("icon_datatype.gif", HierarchicalTableGenerator.TEXT_ICON_DATATYPE);
            else
                row.setIcon("icon_resource.png", HierarchicalTableGenerator.TEXT_ICON_RESOURCE);

            var reference = _pkp.GetLinkForElementDefinition(nav.Structure, element);

            UnusedTracker used = new UnusedTracker();
            used.used = true;
            
            Cell left = new Cell(null, reference, s, !hasDef ? null : element.Definition, null);
            row.getCells().Add(left);
            Cell gc = new Cell();
            row.getCells().Add(gc);

            if (element.IsModifier.GetValueOrDefault())
                checkForNoChange(element.IsModifierElement, gc.addImage("modifier.png", "This element is a modifier element", "?!"));
            if (element.MustSupport.GetValueOrDefault()) 
                checkForNoChange(element.MustSupportElement, gc.addImage("mustsupport.png", "This element must be supported", "S"));
            if (element.IsSummary.GetValueOrDefault()) 
                checkForNoChange(element.IsSummaryElement, gc.addImage("summary.png", "This element is included in summaries", "Î£"));
            if (!element.Constraint.IsNullOrEmpty() || !element.ConditionElement.IsNullOrEmpty()) 
                gc.addImage("lock.png", "This element has or is affected by some invariants", "I");

            StructureDefinition extDefn = null;

    
            if (ext)
            {
                // If this element (row) in the table is an extension...
                if (element.Type != null && element.Type.Count == 1 && element.Type[0].Profile != null) 
                {
                    extDefn = _pkp.GetExtensionDefinition(element.Type[0].Profile);
        
                    if (extDefn == null) 
                    {
                        genCardinality(gen, element, row, hasDef, used, null);
                        row.getCells().Add(new Cell(null, null, "?? " + element.Type[0].Profile, null, null));
                        generateDescription(gen, row, element, null, used.used, extDefn.Url, pkp);
                    }
                    else 
                    {
                        row.getCells().Add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, extDefn.Definition, used), null, null));
                        genTypes(gen, row, extDefn.Definition, profile);
                        generateDescription(gen, row, element, extDefn.Definition, used.used, element.Definition.Type[0].Profile, profile);
                    } 
                }
                else if (element.Definition != null) 
                {
                    row.getCells().Add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
                    genTypes(gen, row, element.Definition, profile);
                    generateDescription(gen, row, element, null, used.used, null, profile);
                } 
                else 
                {
                    row.getCells().Add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
                    row.getCells().Add(new Cell());
                    generateDescription(gen, row, element, null, used.used, null, profile);
                }
            } 
            else 
            {
                row.getCells().Add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
                
                if (element.Definition != null)
                    genTypes(gen, row, element.Definition, profile);
                else
                    row.getCells().Add(new Cell());
        
                generateDescription(gen, row, element, null, used.used, null, profile);
            }
      
            if (element.Slicing != null) 
            {
                row.setIcon("icon_slice.png");
                row.getCells()[2].getPieces().Clear();
        
                foreach (Cell cell in row.getCells())
                    foreach (Piece p in cell.getPieces())
                    {
                        p.addStyle("font-style: italic");
                    }        
            }

            if (used.used || showMissing)
                rows.Add(row);
      
            if (!used.used) 
            {
                foreach (Cell cell in row.getCells())
                foreach (Piece p in cell.getPieces()) 
                {
                    p.setStyle("text-decoration:line-through");
                    p.setReference(null);
                }
            } 
            else
            {
                if (nav.MoveToFirstChild())
                {
                    do
                    {
                        genElement(gen, row.getSubRows(), nav, profile, showMissing);
                    } while (nav.MoveToNext());

                   nav.MoveToParent();
                }
            }
        }

  
        private bool onlyInformationIsMapping(ElementNavigator nav)
        {
            return false;
            //TODO: Port
            //return (!e.hasName() && !e.hasSlicing() && (onlyInformationIsMapping(e))) &&
            //    getChildren(list, e).isEmpty();

        }

        //TODO: Port
        //private boolean onlyInformationIsMapping(ElementDefinition d) {
        //return !d.hasShort() && !d.hasDefinition() && 
        //    !d.hasRequirements() && !d.getAlias().isEmpty() && !d.hasMinElement() &&
        //    !d.hasMax() && !d.getType().isEmpty() && !d.hasNameReference() && 
        //    !d.hasExample() && !d.hasFixed() && !d.hasMaxLengthElement() &&
        //    !d.getCondition().isEmpty() && !d.getConstraint().isEmpty() && !d.hasMustSupportElement() &&
        //    !d.hasBinding();
        //}


        private bool allTypesAre(List<ElementDefinition.TypeRefComponent> types, String name) 
        {
            return types.All( t => t.Code == name );
        }


        //TODO: Maybe copy Forge code? That also traces whether there are changes to an element...
        private Piece checkForNoChange(Element source, Piece piece)
        {
            //if (source.hasUserData(DERIVATION_EQUALS))            
            //{
            //    piece.addStyle("opacity: 0.5");
            //}
            return piece;
        }

        private Piece checkForNoChange(Element src1, Element src2, Piece piece)
        {
            //if (src1.hasUserData(DERIVATION_EQUALS) && src2.hasUserData(DERIVATION_EQUALS))
            //{
            //    piece.addStyle("opacity: 0.5");
            //}
            return piece;
        }


        private void genTypes(HierarchicalTableGenerator gen, Row r, Profile.ElementDefinitionComponent elementDefn, Profile profile)
        {
            Cell c = new Cell();
            r.getCells().Add(c);

            if (elementDefn.Type == null) return;

            bool first = true;
            foreach (Profile.TypeRefComponent t in elementDefn.Type)
            {
                if (first)
                    first = false;
                else
                    c.addPiece(new Piece(null, ", ", null));

                if (t.Code == "ResourceReference" || (t.Code == "Resource" && t.Profile != null))
                {
                    var reference = _pkp.GetLinkForProfileReference(profile, t.Profile);
                    var label = _pkp.GetLabelForProfileReference(profile, t.Profile);

                    if (t.Profile.StartsWith("http://hl7.org/fhir/Profile/"))
                    {
                        String rn = t.Profile.Substring(28);
                        c.addPiece(new Piece(_pkp.GetLinkForTypeDocu(rn), rn, null));
                    }
                    else if (t.Profile.StartsWith("#"))
                        c.addPiece(new Piece(_pkp.GetLinkForLocalStructure(profile, t.Profile.Substring(1)), t.Profile, null));
                    else
                        c.addPiece(new Piece(t.Profile, t.Profile, null));
                }
                else if (t.Profile != null)
                { // a profiled type
                    var reference = _pkp.GetLinkForProfileReference(profile, t.Profile);
                    var label = _pkp.GetLabelForProfileReference(profile, t.Profile);

                    if (reference != null)
                    {
                        String[] parts = reference.Split('|');      //TODO: Not too sure, was: String[] parts = ref.split("\\|"); in Java
                        c.addPiece(new Piece(reference,label,t.Code));
                    }
                    else
                        c.addPiece(new Piece(reference, t.Code, null));
                }
                else if (_pkp.HasLinkForTypeDocu(t.Code))
                {
                    c.addPiece(new Piece(_pkp.GetLinkForTypeDocu(t.Code), t.Code, null));
                }
                else
                    c.addPiece(new Piece(null, t.Code, null));
            }
        }


        private Cell generateDescription(HierarchicalTableGenerator gen, Row row, ElementDefinition element,
            ElementDefinition fallback,
            bool used, string extensionUrl, ProfileKnowledgeProvider pkp)
        {
            Cell c = new Cell();
            row.getCells().Add(c);

            if (used)
            {
                if (element.Short != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.addPiece(checkForNoChange(element.ShortElement, new Piece(null, element.Short, null)));
                }
                else if (fallback != null && fallback.Short != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.addPiece(checkForNoChange(fallback.ShortElement, new Piece(null, fallback.Short, null)));
                }

                if (extensionUrl != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    String reference = _pkp.GetLinkForExtensionDefinition(extensionUrl);
                    c.getPieces().Add(new Piece(null, "URL: ", null).addStyle("font-weight:bold"));
                    c.getPieces().Add(new Piece(reference, reference, null));
                }

                if (element.Slicing != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(new Piece(null, "Slice: ", null).addStyle("font-weight:bold"));
                    c.getPieces().Add(new Piece(null, describeSlice(element.Slicing), null));
                }

                if (element.Binding != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    String reference = _pkp.GetLinkForBinding(element.Binding);
                    c.getPieces().Add(checkForNoChange(element.Binding,
                        new Piece(null, "Binding: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Binding, new Piece(reference, element.Binding.Name, null)));

                    if (element.Binding.Strength.HasValue)
                    {
                        c.getPieces().Add(checkForNoChange(element.Binding, new Piece(null, " (", null)));
                        c.getPieces().Add(checkForNoChange(element.Binding, new Piece(null,
                                bindingStrengthToCode(element.Binding.Strength),
                                bindingStrengthToDefinition(element.Binding.Strength))));
                        c.getPieces().Add(new Piece(null, ")", null));
                    }
                }

                if (element.Constraint != null)
                {
                    foreach (var inv in element.Constraint)
                    {
                        if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                        c.getPieces().Add(checkForNoChange(inv, new Piece(null, inv.Key + ": ", null).addStyle("font-weight:bold")));
                        c.getPieces().Add(checkForNoChange(inv, new Piece(null, inv.Human, null)));
                    }
                }

                if (element.Fixed != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(checkForNoChange(element.Fixed,
                        new Piece(null, "Fixed Value: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Fixed, new Piece(null, buildJson(element.Fixed), null)
                            .addStyle("color: darkgreen")));
                }
                else if (element.Pattern != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(checkForNoChange(element.Pattern,
                        new Piece(null, "Required Pattern: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Pattern, new Piece(null, buildJson(element.Pattern), null)
                        .addStyle("color: darkgreen")));
                }
                else if (element.Example != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(checkForNoChange(element.Example, new Piece(null, "Example: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Example, new Piece(null, buildJson(element.Example), null).addStyle("color: darkgreen")));
                }
            }

            return c;
        }

        private String buildJson(Element value)
        {
            if (value is Primitive)
                return Hl7.Fhir.Serialization.PrimitiveTypeConverter.GetValueAsString((Primitive)value);

            return Hl7.Fhir.Serialization.FhirSerializer.SerializeToJson(value, root:"root");
        }

        private string bindingStrengthToCode(ElementDefinition.BindingStrength? strength)
        {
            if(strength == null) return "?";

            return strength.ToString().ToLower();
        }

        private string bindingStrengthToDefinition(ElementDefinition.BindingStrength? strength)
        {
            if(strength == null) return "?";

            switch (strength) 
            {
            case ElementDefinition.BindingStrength.Required: return "To be conformant, instances of this element SHALL include a code from the specified value set.";
            case ElementDefinition.BindingStrength.Extensible: return "To be conformant, instances of this element SHALL include a code from the specified value set if any of the codes within the value set can apply to the concept being communicated.  If the valueset does not cover the concept (based on human review), alternate codings (or, data type allowing, text) may be included instead.";
            case ElementDefinition.BindingStrength.Preferred: return "Instances are encouraged to draw from the specified codes for interoperability purposes but are not required to do so to be considered conformant.";
            case ElementDefinition.BindingStrength.Example: return "Instances are not expected or even encouraged to draw from the specified value set.  The value set merely provides examples of the types of concepts intended to be included.";
            default: return "?";
          }
        }


        private String describeSlice(ElementDefinition.ElementDefinitionSlicingComponent slicing)
        {
            string rules;

            switch (slicing.Rules)
            {
                case ElementDefinition.SlicingRules.Closed: rules = "Closed"; break;
                case ElementDefinition.SlicingRules.Open: rules = "Open"; break;
                case ElementDefinition.SlicingRules.OpenAtEnd: rules = "Open At End"; break;
                default: rules = "??"; break;
            }

            return (slicing.Ordered == true ? "Ordered, " : "Unordered, ") + rules + ", by " + String.Join(", ",slicing.Discriminator);
        }


        private void genCardinality(HierarchicalTableGenerator gen, ElementDefinition definition, Row row, bool hasDef, UnusedTracker tracker, ElementDefinition fallback) 
        {
            var min = definition.MinElement ?? new Integer();
            var max = definition.MaxElement ?? new FhirString();

            if (min.Value == null && fallback != null)
                min = fallback.MinElement ?? new Integer();
            if (max.Value == null && fallback != null)
                max = fallback.MaxElement ?? new FhirString();

            tracker.used = max == null || !(max.Value == "0");

            Cell cell = new Cell(null, null, null, null, null);
            row.getCells().Add(cell);
    
            if (min != null || max != null)
            {
                cell.addPiece(checkForNoChange(min, new Piece(null, !min.Value.HasValue ? "" : min.Value.ToString(), null)));
                cell.addPiece(checkForNoChange(min,max, new Piece(null, "..", null)));
                cell.addPiece(checkForNoChange(min, new Piece(null, max.Value != null ? "" : max.Value, null)));
            } 
        }


        private String describeCardinality(Profile.ElementDefinitionComponent definition, Profile.ElementDefinitionComponent fallback, UnusedTracker tracker)
        {
            var min = definition.Min;
            var max = definition.Max;

            if (min == null && fallback != null)
                min = fallback.Min;
            if (max == null && fallback != null)
                max = fallback.Max;

            tracker.used = max == null || !(max == "0");

            if (min == null && max == null)
                return null;
            else
                return (min == null ? "" : min.ToString() + ".." + (max == null ? "" : max));
        }
    }
}


#if ORIGINAL_JAVA_CODE
public XhtmlNode generateTable(String defFile, ProfileStructureComponent structure, boolean diff, String imageFolder, boolean inlineGraphics, Profile profile, ProfileKnowledgeProvider pkp, String profileBaseFileName) throws Exception {
    HeirarchicalTableGenerator gen = new HeirarchicalTableGenerator(imageFolder, inlineGraphics);
    TableModel model = gen.initNormalTable();
    List<ElementComponent> list = diff ? structure.getDifferential().getElement() : structure.getSnapshot().getElement();
    genElement(defFile == null ? null : defFile+"#"+structure.getNameSimple()+".", gen, model.getRows(), list.get(0), list, profile, pkp, diff, profileBaseFileName);
    return gen.generate(model);
  }

  private void genElement(String defPath, HeirarchicalTableGenerator gen, List<Row> rows, ElementComponent element, List<ElementComponent> all, Profile profile, ProfileKnowledgeProvider pkp, boolean showMissing, String profileBaseFileName) throws Exception {
    if (!onlyInformationIsMapping(all, element)) { // we don't even show it in this case
    Row row = new Row();
    row.setAnchor(element.getPathSimple());
    String s = tail(element.getPathSimple());
    boolean hasDef = element.Definition != null;
    boolean ext = false;
    if (s.equals("extension") || s.equals("modifierExtension")) { 
      row.setIcon("icon_extension_simple.png");
      ext = true;
    } else if (!hasDef || element.Definition.getType().size() == 0)
      row.setIcon("icon_element.gif");
    else if (hasDef && element.Definition.getType().size() > 1) {
      if (allTypesAre(element.Definition.getType(), "Reference"))
        row.setIcon("icon_reference.png");
      else
        row.setIcon("icon_choice.gif");
    } else if (hasDef && element.Definition.Type[0].getCode().getValue().startsWith("@"))
      row.setIcon("icon_reuse.png");
    else if (hasDef && isPrimitive(element.Definition.Type[0].getCode().getValue()))
      row.setIcon("icon_primitive.png");
    else if (hasDef && isReference(element.Definition.Type[0].getCode().getValue()))
      row.setIcon("icon_reference.png");
    else if (hasDef && isDataType(element.Definition.Type[0].getCode().getValue()))
      row.setIcon("icon_datatype.gif");
    else
      row.setIcon("icon_resource.png");
    String ref = defPath == null ? null : defPath + makePathLink(element);
    UnusedTracker used = new UnusedTracker();
    used.used = true;
    Cell left = new Cell(null, ref, s, !hasDef ? null : element.Definition.getFormalSimple(), null);
    row.getCells().add(left);
    if (ext) {
      if (element.Definition != null && element.Definition.getType().size() == 1 && element.Definition.Type[0].getProfile() != null) {
        ExtensionDefinition extDefn = pkp.getExtensionDefinition(profile, element.Definition.Type[0].Profile);
        if (extDefn == null) {
            row.getCells().add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
            row.getCells().add(new Cell(null, null, "?? "+element.Definition.Type[0].Profile, null, null));
            generateDescription(gen, row, element, null, used.used, profile.getUrlSimple(), element.Definition.Type[0].Profile, pkp, profile);
          } else {
            row.getCells().add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, extDefn.getDefn().getElement().get(0).getDefinition(), used), null, null));
            genTypes(gen, pkp, row, extDefn.getDefn().getElement().get(0), profileBaseFileName, profile);
            generateDescription(gen, row, element, extDefn.getDefn().getElement().get(0), used.used, profile.getUrlSimple(), element.Definition.Type[0].Profile, pkp, profile);
        }
      } else if (element.Definition != null) {
          row.getCells().add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
          genTypes(gen, pkp, row, element, profileBaseFileName, profile);
          generateDescription(gen, row, element, null, used.used, null, null, pkp, profile);
      } else {
          row.getCells().add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
          row.getCells().add(new Cell());
          generateDescription(gen, row, element, null, used.used, null, null, pkp, profile);
      }
    } else {
        row.getCells().add(new Cell(null, null, !hasDef ? null : describeCardinality(element.Definition, null, used), null, null));
        if (hasDef)
          genTypes(gen, pkp, row, element, profileBaseFileName, profile);
        else
          row.getCells().add(new Cell());
        generateDescription(gen, row, element, null, used.used, null, null, pkp, profile);
      }
      if (element.getSlicing() != null) {
        row.setIcon("icon_slice.png");
        row.getCells().get(2).getPieces().clear();
        for (Cell cell : row.getCells())
          for (Piece p : cell.getPieces()) {
            p.addStyle("font-style: italic");
          }
        
      }
      if (used.used || showMissing)
        rows.add(row);
      if (!used.used) {
        for (Cell cell : row.getCells())
          for (Piece p : cell.getPieces()) {
            p.setStyle("text-decoration:line-through");
            p.setReference(null);
          }
      } else{
        List<ElementComponent> children = getChildren(all, element);
        for (ElementComponent child : children)
          genElement(defPath, gen, row.getSubRows(), child, all, profile, pkp, showMissing, profileBaseFileName);
      }
    }
  }


  private String makePathLink(ElementComponent element) {
    if (element.getName() == null)
      return element.getPathSimple();
    if (!element.getPathSimple().contains("."))
      return element.getNameSimple();
    return element.getPathSimple().substring(0, element.getPathSimple().lastIndexOf("."))+"."+element.getNameSimple();
  }

  private Cell generateDescription(HeirarchicalTableGenerator gen, Row row, ElementComponent definition, ElementComponent fallback, boolean used, String baseURL, String url, ProfileKnowledgeProvider pkp, Profile profile) {
    // TODO Auto-generated method stub
    Cell c = new Cell();
    row.getCells().add(c);                

    if (used) {
      if (definition.getDefinition() != null && definition.getDefinition().getShort() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
        c.addPiece(new Piece(null, definition.getDefinition().Short, null));
      } else if (fallback != null && fallback.getDefinition() != null && fallback.getDefinition().getShort() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
        c.addPiece(new Piece(null, fallback.getDefinition().Short, null));
      }
      if (url != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
        String fullUrl = url.startsWith("#") ? baseURL+url : url;
        String ref = pkp.getLinkForExtension(profile, url);
        c.getPieces().add(new Piece(null, "URL: ", null).addStyle("font-weight:bold"));
        c.getPieces().add(new Piece(ref, fullUrl, null));
      }

      if (definition.getSlicing() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
        c.getPieces().add(new Piece(null, "Slice: ", null).addStyle("font-weight:bold"));
        c.getPieces().add(new Piece(null, describeSlice(definition.getSlicing()), null));
      }
      if (definition.getDefinition() != null) {
        if (definition.getDefinition().getBinding() != null) {
          if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
          String ref = pkp.resolveBinding(definition.getDefinition().getBinding());
          c.getPieces().add(new Piece(null, "Binding: ", null).addStyle("font-weight:bold"));
          c.getPieces().add(new Piece(ref, definition.getDefinition().getBinding().getNameSimple(), null));
        }
        for (ElementDefinitionConstraintComponent inv : definition.getDefinition().getConstraint()) {
          if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
          c.getPieces().add(new Piece(null, "Inv-"+inv.getKeySimple()+": ", null).addStyle("font-weight:bold"));
          c.getPieces().add(new Piece(null, inv.getHumanSimple(), null));
        }
        if (definition.getDefinition().getValue() != null) {        
          if (!c.getPieces().isEmpty()) c.addPiece(new Piece("br"));
          c.getPieces().add(new Piece(null, "Fixed Value: ", null).addStyle("font-weight:bold"));
          c.getPieces().add(new Piece(null, "(todo)", null));
        }
        // ?? example from definition    
      }
    }
    return c;
  }

  public String describeSlice(ElementSlicingComponent slicing) {
    return (slicing.getOrderedSimple() ? "Ordered, " : "Unordered, ")+describe(slicing.getRulesSimple())+", by "+slicing.getDiscriminatorSimple();
  }

  private String describe(ResourceSlicingRules rules) {
    switch (rules) {
    case closed : return "Closed";
    case open : return "Open";
    case openAtEnd : return "Open At End";
    default:
      return "??";
    }
  }

  private boolean onlyInformationIsMapping(List<ElementComponent> list, ElementComponent e) {
    return (e.getName() == null && e.getSlicing() == null && (e.getDefinition() == null || onlyInformationIsMapping(e.getDefinition()))) &&
        getChildren(list, e).isEmpty();
  }

  private boolean onlyInformationIsMapping(ElementDefinitionComponent d) {
    return d.getShort() == null && d.getFormal() == null && 
        d.getRequirements() == null && d.getSynonym().isEmpty() && d.getMin() == null &&
        d.getMax() == null && d.getType().isEmpty() && d.getNameReference() == null && 
        d.getExample() == null && d.getValue() == null && d.getMaxLength() == null &&
        d.getCondition().isEmpty() && d.getConstraint().isEmpty() && d.getMustSupport() == null &&
        d.getBinding() == null;
  }

  private boolean allTypesAre(List<TypeRefComponent> types, String name) {
    for (TypeRefComponent t : types) {
      if (!t.Code.equals(name))
        return false;
    }
    return true;
  }

  private List<ElementComponent> getChildren(List<ElementComponent> all, ElementComponent element) {
    List<ElementComponent> result = new ArrayList<Profile.ElementComponent>();
    int i = all.indexOf(element)+1;
    while (i < all.size() && all.get(i).getPathSimple().length() > element.getPathSimple().length()) {
      if ((all.get(i).getPathSimple().substring(0, element.getPathSimple().length()+1).equals(element.getPathSimple()+".")) && !all.get(i).getPathSimple().substring(element.getPathSimple().length()+1).contains(".")) 
        result.add(all.get(i));
      i++;
    }
    return result;
  }

  private String tail(String path) {
    if (path.contains("."))
      return path.substring(path.lastIndexOf('.')+1);
    else
      return path;
  }

  private boolean isDataType(String value) {
    return Utilities.existsInList(value, "Identifier", "HumanName", "Address", "ContactPoint", "Timing", "Quantity", "Attachment", "Range", 
          "Period", "Ratio", "CodeableConcept", "Coding", "SampledData", "Age", "Distance", "Duration", "Count", "Money");
  }

  private boolean isReference(String value) {
    return value.equals("Reference");
  }

  private boolean isPrimitive(String value) {
    return Utilities.existsInList(value, "boolean", "integer", "decimal", "base64Binary", "instant", "string", "date", "dateTime", "code", "oid", "uuid", "id");
  }

  public static String summarise(Profile p, ProfileKnowledgeProvider pkp) throws Exception {
    if (p.getExtensionDefn().isEmpty())
      return "This profile has constraints on the following resources: "+listStructures(p, pkp);
    else if (p.getStructure().isEmpty())
      return "This profile defines "+Integer.toString(p.getExtensionDefn().size())+" extensions.";
    else
      return "This profile defines "+Integer.toString(p.getExtensionDefn().size())+" extensions and has constraints on the following resources: "+listStructures(p, pkp);
  }

  private static String listStructures(Profile p, ProfileKnowledgeProvider pkp) throws Exception {
    StringBuilder b = new StringBuilder();
    boolean first = true;
    for (ProfileStructureComponent s : p.getStructure()) {
      if (first)
        first = false;
      else
        b.append(", ");
      if (pkp != null && pkp.hasLinkFor(s.getTypeSimple()))
        b.append("<a href=\""+pkp.getLinkFor(s.getTypeSimple())+"\">"+s.getTypeSimple()+"</a>");
      else
        b.append(s.getTypeSimple());
    }
    return b.toString();
  }


  public StrucResult getStructure(Profile source, String url) throws Exception {
    Profile profile;
    String code;
    if (url.startsWith("#")) {
      profile = source;
      code = url.substring(1);
    } else {
      String[] parts = url.split("\\#");
      if (!context.getProfiles().containsKey(parts[0])) {
      	if (parts[0].startsWith("http:") || parts[0].startsWith("https:")) {
        	String[] ps = parts[0].split("\\/Profile\\/");
        	if (ps.length != 2)
        		throw new Exception("Unable to understand address of profile: "+parts[0]);
        	FHIRClient client = new FHIRSimpleClient();
        	client.initialize(ps[0]);
        	AtomEntry<Profile> ae = client.read(Profile.class, ps[1]);
        	context.getProfiles().put(parts[0], ae);
      	} else
      		return null;
      }
      profile = context.getProfiles().get(parts[0]).getResource();
      code = parts.length < 2 ? null : parts[1];
    }

    if (profile != null) {
      ProfileStructureComponent structure = null;
      for (ProfileStructureComponent s : profile.getStructure()) {
        if (s.getNameSimple().equals(code) || s.getPublishSimple()) 
          structure = s;
      }
      if (structure != null)
        return new StrucResult(profile, structure);
    }
    return null;
  }

  public ExtensionResult getExtensionDefn(Profile source, String url) {
    Profile profile;
    String code;
    if (url.startsWith("#")) {
      profile = source;
      code = url.substring(1);
    } else {
      String[] parts = url.split("\\#");
      profile = context.getProfiles().get(parts[0]).getResource();
      code = parts[1];
    }

    if (profile != null) {
      ProfileExtensionDefnComponent defn = null;
      for (ProfileExtensionDefnComponent s : profile.getExtensionDefn()) {
        if (s.getCodeSimple().equals(code)) 
          defn = s;
      }
      if (defn != null)
        return new ExtensionResult(profile, defn);
    }
    return null;
  }

private Cell generateDescription(HeirarchicalTableGenerator gen, Row row, ElementComponent definition, ElementComponent fallback, boolean used, String baseURL, String url, ProfileKnowledgeProvider pkp, Profile profile) {
    // TODO Auto-generated method stub
    Cell c = gen.new Cell();
    row.getCells().add(c);                

    if (used) {
      if (definition.getDefinition() != null && definition.getDefinition().getShort() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
        c.addPiece(gen.new Piece(null, definition.getDefinition().getShortSimple(), null));
      } else if (fallback != null && fallback.getDefinition() != null && fallback.getDefinition().getShort() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
        c.addPiece(gen.new Piece(null, fallback.getDefinition().getShortSimple(), null));
      }
      if (url != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
        String fullUrl = url.startsWith("#") ? baseURL+url : url;
        String ref = pkp.getLinkForExtension(profile, url);
        c.getPieces().add(gen.new Piece(null, "URL: ", null).addStyle("font-weight:bold"));
        c.getPieces().add(gen.new Piece(ref, fullUrl, null));
      }

      if (definition.getSlicing() != null) {
        if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
        c.getPieces().add(gen.new Piece(null, "Slice: ", null).addStyle("font-weight:bold"));
        c.getPieces().add(gen.new Piece(null, describeSlice(definition.getSlicing()), null));
      }
      if (definition.getDefinition() != null) {
        if (definition.getDefinition().getBinding() != null) {
          if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
          String ref = pkp.resolveBinding(definition.getDefinition().getBinding());
          c.getPieces().add(gen.new Piece(null, "Binding: ", null).addStyle("font-weight:bold"));
          c.getPieces().add(gen.new Piece(ref, definition.getDefinition().getBinding().getNameSimple(), null));
        }
        for (ElementDefinitionConstraintComponent inv : definition.getDefinition().getConstraint()) {
          if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
          c.getPieces().add(gen.new Piece(null, "Inv-"+inv.getKeySimple()+": ", null).addStyle("font-weight:bold"));
          c.getPieces().add(gen.new Piece(null, inv.getHumanSimple(), null));
        }
        if (definition.getDefinition().getValue() != null) {        
          if (!c.getPieces().isEmpty()) c.addPiece(gen.new Piece("br"));
          c.getPieces().add(gen.new Piece(null, "Fixed Value: ", null).addStyle("font-weight:bold"));
          c.getPieces().add(gen.new Piece(null, "(todo)", null));
        }
        // ?? example from definition    
      }
    }
    return c;
  }
#endif
