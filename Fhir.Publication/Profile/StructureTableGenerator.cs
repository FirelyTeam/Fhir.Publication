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
using Hl7.Fhir.Rest;
using Hl7.Fhir.Specification.Navigation;
using Newtonsoft.Json.Linq;
using Hl7.Fhir.Support;

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
            if (structure.Snapshot != null && structure.Snapshot.Element[0].Short == null)
                throw new InvalidOperationException("Structure generator is ran on profile data that misses essential documentation data - are you using validation-min.zip instead of validation.zip?");

            HierarchicalTableGenerator gen = new HierarchicalTableGenerator(_pkp);
            var model = TableModel.CreateNormalTable(_pkp);

            List<ElementDefinition> list = diff ? structure.Differential.Element : structure.Snapshot.Element;
            List<StructureDefinition> structures = new List<StructureDefinition>();
            structures.Add(structure);

            var nav = new ElementNavigator(structure);
            nav.MoveToFirstChild();

            genElement(gen, model.Rows, nav, _pkp, diff, null, snapshot);

            if (!_pkp.StandAlone)
                return gen.generate(model);
            else
                return wrap(gen.generate(model), structure.Name);
        }

        private XElement wrap(XElement table, string name)
        {
            string head =
                "<head><title>StructureDefinition: " + name + "</title>" +
                "<meta content='width=device-width, initial-scale=1.0' name='viewport'/>" +
                "<meta content='http://hl7.org/fhir' name='author'/>" +
                "<link rel='stylesheet' href='../dist/css/fhir.css'/>" +
                "<link rel='Prev' href='http://hl7.org/implement/standards/fhir/lipid-report-lipidprofile'/>" +
                "<link rel='stylesheet' href='../dist/css/bootstrap.css'/>" +
                "<link rel='stylesheet' href='../dist/css/bootstrap-fhir.css'/>" +
                "<link rel='stylesheet' href='../dist/css/project.css'/>" +
                "</head>";

            string page = "<html>" + head + "<body /></html>";

            XElement result = XElement.Parse(page);
            var body = result.Element("body");
            body.Add(table);

            return result;
        }


        private void genElement(HierarchicalTableGenerator gen, List<Row> rows, ElementNavigator nav, ProfileKnowledgeProvider pkp, bool showMissing, bool? extensions, bool snapshot)
        {
            var element = nav.Current;
            String s = nav.PathName;

            if (!snapshot && extensions != null && extensions != (s=="extension" || s=="modifierExtension"))
                return;

            if(onlyInformationIsMapping(nav)) return;  // we don't even show it in this case

            Row row = new Row();
            row.setAnchor(element.Path);
            
            bool ext = false;
    
            if (s == "extension" || s == "modifierExtension")
            {
                row.setIcon("icon_extension_simple.png", HierarchicalTableGenerator.TEXT_ICON_EXTENSION_SIMPLE);
                ext = true;
            }
            else if (element.Type == null || !element.Type.Any())
            {
                row.setIcon("icon_element.gif", HierarchicalTableGenerator.TEXT_ICON_ELEMENT);
            }
            else if (element.Type.Count > 1)
            {
                if (allTypesAre(element.Type, "Reference"))
                    row.setIcon("icon_reference.png", HierarchicalTableGenerator.TEXT_ICON_REFERENCE);
                else
                    row.setIcon("icon_choice.gif", HierarchicalTableGenerator.TEXT_ICON_CHOICE);
            }
            else if (element.Type[0].Code.StartsWith("@"))
            {
                //TODO: That's not a legal code, will this ever appear?
                //I am pretty sure this depends on ElementDefn.NameReference
                row.setIcon("icon_reuse.png", HierarchicalTableGenerator.TEXT_ICON_REUSE);
            }
            else if ( _pkp.isPrimitive(element.Type[0].Code))
                row.setIcon("icon_primitive.png", HierarchicalTableGenerator.TEXT_ICON_PRIMITIVE);
            else if (_pkp.isReference(element.Type[0].Code))
                row.setIcon("icon_reference.png", HierarchicalTableGenerator.TEXT_ICON_REFERENCE);
            else if (_pkp.isDataType(element.Type[0].Code))
                row.setIcon("icon_datatype.gif", HierarchicalTableGenerator.TEXT_ICON_DATATYPE);
            else
                row.setIcon("icon_resource.png", HierarchicalTableGenerator.TEXT_ICON_RESOURCE);

            var reference = _pkp.GetLinkForElementDefinition(nav.Structure, element);

            UnusedTracker used = new UnusedTracker();
            used.used = true;
            
            Cell left = new Cell(null, reference, s, element.Definition, null);
            row.getCells().Add(left);
            Cell gc = new Cell();
            row.getCells().Add(gc);

            if (element.IsModifier.GetValueOrDefault())
                checkForNoChange(element.IsModifierElement, gc.addImage("modifier.png", "This element is a modifier element", "?!"));
            if (element.MustSupport.GetValueOrDefault()) 
                checkForNoChange(element.MustSupportElement, gc.addImage("mustsupport.png", "This element must be supported", "S"));
            if (element.IsSummary.GetValueOrDefault())
                checkForNoChange(element.IsSummaryElement, gc.addImage("summary.png", "This element is included in summaries", "Σ"));
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
                        genCardinality(gen, element, row, used, null);
                        row.getCells().Add(new Cell(null, null, "?? " + element.Type[0].Profile, null, null));
                        generateDescription(gen, row, element, null, used.used, extDefn.Url, pkp);
                    }
                    else 
                    {
                        var extNav = new ElementNavigator(extDefn);
                        extNav.MoveToFirstChild();

                        var name = new ResourceIdentity(element.Type[0].Profile).Id;
                        left.getPieces()[0].setText(name);
                        left.getPieces()[0].setHint("Extension URL = "+element.Type[0].Profile);
                        genCardinality(gen, element, row, used, extNav.Current);
                        var valueDefn = extNav.JumpToFirst("Extension.value") ? extNav.Current : null;
            
                        if (valueDefn != null && valueDefn.Max != "0")
                            genTypes(gen, row, valueDefn, nav.Structure);
                        else // if it's complex, we just call it nothing                            
                            row.getCells().Add(new Cell(null, null, "(Complex)", null, null));
            
                        generateDescription(gen, row, element, extDefn.Snapshot.Element[0], used.used, null, pkp);
                    } 
                }
                else
                {
                    genCardinality(gen, element, row, used, null);
                    genTypes(gen, row, element, nav.Structure);
                    generateDescription(gen, row, element, null, used.used, null, pkp);
                }
            } 
            else 
            {
                genCardinality(gen, element, row, used, null);
                genTypes(gen, row, element, nav.Structure);
                generateDescription(gen, row, element, null, used.used, null, pkp);
            }
      
            if (element.Slicing != null) 
            {
                if (standardExtensionSlicing(element)) 
                {
                    used.used = false;
                    showMissing = false;
                }
                else 
                {
                    row.setIcon("icon_slice.png", HierarchicalTableGenerator.TEXT_ICON_SLICE);
                    row.getCells()[2].getPieces().Clear();
                    foreach (var cell in row.getCells())
                        foreach (var p in cell.getPieces()) 
                        {
                            p.addStyle("font-style: italic");
                        }
                }
            }

            if (used.used || showMissing)
                rows.Add(row);
      
            if (!used.used && element.Slicing == null) 
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
                        if(nav.PathName != "id")
                        {
                            if(snapshot)
                                genElement(gen, row.getSubRows(), nav, pkp, showMissing, false, snapshot);
                            else
                                genElement(gen, row.getSubRows(), nav, pkp, showMissing, true, false);
                        }
                    } while (nav.MoveToNext());

                   nav.MoveToParent();
                }
            }
        }

        private bool standardExtensionSlicing(ElementDefinition element)
        {
            var t = element.GetNameFromPath();
            return (t == "extension" || t == "modifierExtension") && element.Slicing.Rules != ElementDefinition.SlicingRules.Closed &&
                element.Slicing.Discriminator.Count() == 1 && element.Slicing.Discriminator.Single() == "url";
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


        private Cell genTypes(HierarchicalTableGenerator gen, Row r, ElementDefinition elementDefn, StructureDefinition structure)
        {
            Cell c = new Cell();
            r.getCells().Add(c);

            if(elementDefn.Type.IsNullOrEmpty()) return c;

            bool first = true;
            var source = elementDefn.Type[0];

            foreach (var t in elementDefn.Type)
            {
                if (first)
                    first = false;
                else
                    c.addPiece(checkForNoChange(source, new Piece(null,", ", null)));

                if (t.Code == "Reference" || (t.Code == "Resource" && t.Profile != null))
                {
                    var reference = _pkp.GetLinkForProfileReference(t.Profile);
                    var label = _pkp.GetLabelForProfileReference(t.Profile);
                    c.addPiece(new Piece(reference, label, null));
                }
                else if (t.Profile != null)
                { 
                    // a profiled type
                    var reference = _pkp.GetLinkForProfileReference(t.Profile);
                    var label = _pkp.GetLabelForProfileReference(t.Profile);

                    c.addPiece(new Piece(reference,label,t.Code));
                }
                else if (_pkp.HasLinkForCoreTypeDocu(t.Code))
                {
                    c.addPiece(new Piece(_pkp.GetLinkForCoreTypeDocu(t.Code), t.Code, null));
                }
                else
                    c.addPiece(new Piece(null, t.Code, null));
            }

            return c;
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
                            element.Binding.bindingStrengthToCode(), element.Binding.bindingStrengthToDefinition())));
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
                    c.getPieces().Add(checkForNoChange(element.Fixed, new Piece(null, element.Fixed.EncodeValue(), null)
                            .addStyle("color: darkgreen")));
                }
                else if (element.Pattern != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(checkForNoChange(element.Pattern,
                        new Piece(null, "Required Pattern: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Pattern, new Piece(null, element.Pattern.EncodeValue(), null)
                        .addStyle("color: darkgreen")));
                }
                else if (element.Example != null)
                {
                    if (c.getPieces().Any()) c.addPiece(new Piece("br"));
                    c.getPieces().Add(checkForNoChange(element.Example, new Piece(null, "Example: ", null).addStyle("font-weight:bold")));
                    c.getPieces().Add(checkForNoChange(element.Example, new Piece(null, element.Example.EncodeValue(), null).addStyle("color: darkgreen")));
                }
            }

            return c;
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


        private void genCardinality(HierarchicalTableGenerator gen, ElementDefinition definition, Row row, UnusedTracker tracker, ElementDefinition fallback) 
        {
            var min = definition.MinElement ?? new Integer();
            var max = definition.MaxElement ?? new FhirString();

            if (min.Value == null && fallback != null)
                min = fallback.MinElement ?? new Integer();
            if (max.Value == null && fallback != null)
                max = fallback.MaxElement ?? new FhirString();

            if(max.Value != null)
                tracker.used = !(max.Value == "0");

            Cell cell = new Cell(null, null, null, null, null);
            row.getCells().Add(cell);
    
            if (min.Value != null || max.Value != null)
            {
                cell.addPiece(checkForNoChange(min, new Piece(null, !min.Value.HasValue ? "" : min.Value.ToString(), null)));
                cell.addPiece(checkForNoChange(min,max, new Piece(null, "..", null)));
                cell.addPiece(checkForNoChange(max, new Piece(null, max.Value == null ? "" : max.Value, null)));
            } 
        }
    }
}
