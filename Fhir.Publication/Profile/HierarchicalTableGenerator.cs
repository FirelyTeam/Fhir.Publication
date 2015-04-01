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


// Classes in this file are updated to match utilities/xhtml/HeirarchicalTableGenerator.java commit #5035 in the Java tooling trunk

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Support;

namespace Hl7.Fhir.Publication.Profile
{
    internal class HierarchicalTableGenerator
    {
        public const String TEXT_ICON_REFERENCE = "Reference to another Resource";
        public const String TEXT_ICON_PRIMITIVE = "Primitive Data Type";
        public const String TEXT_ICON_DATATYPE = "Data Type";
        public const String TEXT_ICON_RESOURCE = "Resource";
        public const String TEXT_ICON_ELEMENT = "Element";
        public const String TEXT_ICON_REUSE = "Reference to another Element";
        public const String TEXT_ICON_EXTENSION = "Extension";
        public const String TEXT_ICON_CHOICE = "Choice of Types";
        public const String TEXT_ICON_SLICE = "Slice Definition";
        public const String TEXT_ICON_EXTENSION_SIMPLE = "Simple Extension";
        public const String TEXT_ICON_PROFILE = "Profile";
        public const String TEXT_ICON_EXTENSION_COMPLEX = "Complex Extension";

        private ProfileKnowledgeProvider _pkp;

        public HierarchicalTableGenerator(ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
        }

        public XElement generate(TableModel model)
        {
            var table = new XElement(XmlNs.XHTMLNS + "table").SetAttribute("border", "0").SetAttribute("cellspacing", "0").SetAttribute("cellpadding", "0");
            table.SetAttribute("style", "border: 0px; font-size: 11px; font-family: verdana; vertical-align: top;");

            var tr = table.AddTag("tr");
            tr.SetAttribute("style", "border: 1px #F0F0F0 solid; font-size: 11px; font-family: verdana; vertical-align: top;");

            XElement tc = null;

            foreach (Title t in model.Titles)
            {
                tc = renderCell(tr, t, "th", null, null, null, false, null);
                if (t.width != 0)
                    tc.SetAttribute("style", "width: " + t.width.ToString() + "px");
            }

            if (tc != null)
                tc.AddTag("span").SetAttribute("style", "float: right")
                        .AddTag("a").SetAttribute("title", "Legend for this format").SetAttribute("href", model.DocoRef)
                        .AddTag("img").SetAttribute("alt", "doco").SetAttribute("src", _pkp.GetDistImageLink(model.DocoImg));

            foreach (Row r in model.Rows)
            {
                renderRow(table, r, 0, new List<bool>());
            }

            return table;
        }


        private void renderRow(XElement table, Row r, int indent, List<Boolean> indents)
        {
            var tr = table.AddTag("tr");
            tr.SetAttribute("style", "border: 0px; padding:0px; vertical-align: top; background-color: white;");
            bool first = true;

            foreach (Cell t in r.getCells())
            {
                renderCell(tr, t, "td", first ? r.getIcon() : null, first ? r.Hint : null, first ? indents : null, r.getSubRows().Any(), first ? r.getAnchor() : null);
                first = false;
            }

            table.AddText("\r\n");

            for (int i = 0; i < r.getSubRows().Count; i++)
            {
                Row c = r.getSubRows()[i];
                var ind = new List<Boolean>();
                ind.AddRange(indents);
                if (i == r.getSubRows().Count - 1)
                    ind.Add(true);
                else
                    ind.Add(false);
                renderRow(table, c, indent + 1, ind);
            }
        }


        private XElement renderCell(XElement tr, Cell c, String name, String icon, String hint, List<Boolean> indents, bool hasChildren, String anchor)
        {
            XElement tc = tr.AddTag(name);
            tc.SetAttribute("class", "heirarchy");

            if (indents != null)
            {
                tc.AddTag("img").SetAttribute("src", srcFor("tbl_spacer.png")).SetAttribute("class", "heirarchy").SetAttribute("alt", ".");
                tc.SetAttribute("style", "vertical-align: top; text-align : left; padding:0px 4px 0px 4px; white-space: nowrap; background-image: url(" + checkExists(indents, hasChildren) + ")");

                for (int i = 0; i < indents.Count - 1; i++)
                {
                    if (indents[i])
                        tc.AddTag("img").SetAttribute("src", srcFor("tbl_blank.png")).SetAttribute("class", "heirarchy").SetAttribute("alt", ".");
                    else
                        tc.AddTag("img").SetAttribute("src", srcFor("tbl_vline.png")).SetAttribute("class", "heirarchy").SetAttribute("alt", ".");
                }

                if (indents.Any())
                {
                    if (indents.Last())
                        tc.AddTag("img").SetAttribute("src", srcFor("tbl_vjoin_end.png")).SetAttribute("class", "heirarchy").SetAttribute("alt", ".");
                    else
                        tc.AddTag("img").SetAttribute("src", srcFor("tbl_vjoin.png")).SetAttribute("class", "heirarchy").SetAttribute("alt", ".");
                }
            }
            else
                tc.SetAttribute("style", "vertical-align: top; text-align : left; padding:0px 4px 0px 4px");

            if (!String.IsNullOrEmpty(icon))
            {
                XElement img = tc.AddTag("img").SetAttribute("src", srcFor(icon)).SetAttribute("class", "heirarchy").SetAttribute("style", "background-color: white;")
                    .SetAttribute("alt", ".");

                if (hint != null) img.SetAttribute("title", hint);

                tc.AddText(" ");
            }

            foreach (Piece p in c.pieces)
            {
                if (!String.IsNullOrEmpty(p.getTag()))
                {
                    var tag = tc.AddTag(p.getTag());

                    if (p.attributes != null)
                    {
                        foreach (String n in p.attributes.Keys)
                            tag.SetAttribute(n, p.attributes[n]);
                    }

                    if (p.getHint() != null)
                        tag.SetAttribute("title", p.getHint());

                    addStyle(tag, p);
                }
                else if (!String.IsNullOrEmpty(p.getReference()))
                {
                    var a = addStyle(tc.AddTag("a"), p);
                    a.SetAttribute("href", p.getReference());
                    if (!String.IsNullOrEmpty(p.getHint()))
                        a.SetAttribute("title", p.getHint());
                    a.AddText(p.getText());
                }
                else
                {
                    if (!String.IsNullOrEmpty(p.getHint()))
                    {
                        var s = addStyle(tc.AddTag("span"), p);
                        s.SetAttribute("title", p.getHint());
                        s.AddText(p.getText());
                    }
                    else if (p.getStyle() != null)
                    {
                        var s = addStyle(tc.AddTag("span"), p);
                        s.AddText(p.getText());
                    }
                    else
                        tc.AddText(p.getText());
                }
            }

            if (!String.IsNullOrEmpty(anchor))
                tc.AddTag("a").SetAttribute("name", nmTokenize(anchor)).AddText(" ");

            return tc;
        }


        private XElement addStyle(XElement node, Piece p)
        {
            if (p.getStyle() != null)
                node.Add(new XAttribute("style", p.getStyle()));
            return node;
        }

        private String nmTokenize(String anchor)
        {
            return anchor.Replace("[", "_").Replace("]", "_");
        }


        private Dictionary<string, string> _files = new Dictionary<string, string>();

        private String srcFor(String filename)
        {
            if (_pkp.InlineGraphics)
            {
                if (_files.ContainsKey(filename)) return _files[filename];

                StringBuilder b = new StringBuilder();
                b.Append("data: image/png;base64,");
                var bytes = File.ReadAllBytes(_pkp.GetDistImageLink(filename));

                b.Append(Convert.ToBase64String(bytes));
                _files.Add(filename, b.ToString());

                return b.ToString();
            }
            else
                return _pkp.GetDistImageLink(filename);
        }


        private void checkModel(TableModel model)
        {
            check(model.Rows.Any(), "Must have rows");
            check(model.Titles.Any(), "Must have titles");
            foreach (Cell c in model.Titles)
                check(c);

            int i = 0;
            foreach (Row r in model.Rows)
            {
                check(r, "rows", model.Titles.Count, i.ToString());
                i++;
            }
        }


        private void check(Cell c)
        {
            bool hasText = false;
            foreach (Piece p in c.pieces)
                if (!String.IsNullOrEmpty(p.getText()))
                    hasText = true;
            check(hasText, "Title cells must have text");
        }


        private void check(Row r, String str, int size, String path)
        {
            check(r.getCells().Count == size, "All rows must have the same number of columns (" + size + ") as the titles but row " + path +
                        " doesn't (" + r.getCells()[0].text() + ")");

            int i = 0;
            foreach (Row c in r.getSubRows())
            {
                check(c, "rows", size, path + "." + i);
                i++;
            }
        }


        private String checkExists(List<Boolean> indents, bool hasChildren)
        {
            String filename = makeName(indents);

            StringBuilder b = new StringBuilder();

            if (_pkp.InlineGraphics)
            {
                if (_files.ContainsKey(filename)) return _files[filename];

                MemoryStream bytes = new MemoryStream();
                genImage(indents, hasChildren, bytes);
                b.Append("data: image/png;base64,");
                var encodeBase64 = Convert.ToBase64String(bytes.ToArray());
                b.Append(encodeBase64);
                _files.Add(filename, b.ToString());
                return b.ToString();
            }
            else
            {
                b.Append("tbl_bck");
                foreach (Boolean i in indents)
                    b.Append(i ? "0" : "1");

                if (hasChildren)
                    b.Append("1");
                else
                    b.Append("0");

                b.Append(".png");

                String file = _pkp.GetGenImagePath(b.ToString());

                if (!File.Exists(file))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(file)))
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                    var stream = new FileStream(file, FileMode.Create);
                    genImage(indents, hasChildren, stream);
                }
            }

            return _pkp.GetGenImageLink(b.ToString());
        }


        private void genImage(List<Boolean> indents, bool hasChildren, Stream stream)
        {
            var bi = new Bitmap(400, 2);
            var graphics = Graphics.FromImage(bi);

            //graphics.DrawRectangle(new Pen(Color.White), 0, 0, 400, 2);
            graphics.DrawRectangle(new Pen(Color.White), 0, 0, 600, 2);     // java code has 600?

            for (int i = 0; i < indents.Count; i++)
            {
                if (!indents[i])
                    bi.SetPixel(12 + (i * 16), 0, Color.Black);
            }

            if (hasChildren)
            {
                bi.SetPixel(12 + (indents.Count * 16), 0, Color.Black);
            }

            bi.Save(stream, ImageFormat.Png);
        }

        private String makeName(List<Boolean> indents)
        {
            StringBuilder b = new StringBuilder();
            b.Append("indents:");
            foreach (Boolean i in indents)
                b.Append(i ? "1" : "0");

            return b.ToString();
        }


        private void check(bool check, String message)
        {
            if (!check)
                throw new Exception(message);
        }
    }


    internal class Piece
    {
        private String tag;
        private String reference;
        private String text;
        private String hint;
        private String style;
        public Dictionary<string, string> attributes;

        public Piece(String tag)
        {
            this.tag = tag;
        }

        public Piece(String reference, String text, String hint)
        {
            this.reference = reference;
            this.text = text;
            this.hint = hint;
        }
        public String getReference()
        {
            return reference;
        }
        public void setReference(String value)
        {
            reference = value;
        }
        public String getText()
        {
            return text;
        }

        internal void setText(string t)
        {
            text = t;
        }

        public String getHint()
        {
            return hint;
        }

        public void setHint(string hint)
        {
            this.hint = hint;
        }

        public String getTag()
        {
            return tag;
        }

        public String getStyle()
        {
            return style;
        }

        public Piece setStyle(String style)
        {
            this.style = style;
            return this;
        }

        public Piece addStyle(String style)
        {
            if (this.style != null)
                this.style = this.style + "; " + style;
            else
                this.style = style;
            return this;
        }

        public void addToHint(String text)
        {
            if (this.hint == null)
                this.hint = text;
            else
                this.hint += (this.hint.EndsWith(".") || this.hint.EndsWith("?") ? " " : ". ") + text;
        }

    }


    internal class Cell
    {
        internal List<Piece> pieces = new List<Piece>();

        public Cell()
        {
        }

        public Cell(String prefix, String reference, String text, String hint, String suffix)
        {

            if (!String.IsNullOrEmpty(prefix)) pieces.Add(new Piece(null, prefix, null));
            pieces.Add(new Piece(reference, text, hint));
            if (!String.IsNullOrEmpty(suffix)) pieces.Add(new Piece(null, suffix, null));
        }

        public List<Piece> getPieces()
        {
            return pieces;
        }

        public Cell addPiece(Piece piece)
        {
            pieces.Add(piece);
            return this;
        }

        public void addStyle(String style)
        {
            foreach (Piece p in pieces)
                p.addStyle(style);
        }

        public void addToHint(String text)
        {
            foreach (Piece p in pieces)
                p.addToHint(text);
        }

        public Piece addImage(String src, String hint, String alt)
        {
            if (pieces.Any() && pieces[0].getTag() == null)
                pieces[0].setText(pieces[0].getText() + " ");
            //      Piece img = new Piece("img");
            Piece img = new Piece(null, alt, hint);
            //      img.attributes = new HashMap<String, String>();
            //      img.attributes.put("src", src);
            //      img.attributes.put("alt", alt);
            //      img.hint = hint;
            pieces.Add(img);
            return img;
        }

        public String text()
        {
            StringBuilder b = new StringBuilder();
            foreach (Piece p in pieces)
                b.Append(p.getText());

            return b.ToString();
        }
    }



    internal class Title : Cell
    {
        internal int width;

        public Title(String prefix, String reference, String text, String hint, String suffix, int width)
            : base(prefix, reference, text, hint, suffix)
        {
            this.width = width;
        }
    }


    internal class Row
    {
        private List<Row> subRows = new List<Row>();
        private List<Cell> cells = new List<Cell>();
        private String icon;
        private String anchor;

        public string Hint;

        public List<Row> getSubRows()
        {
            return subRows;
        }
        public List<Cell> getCells()
        {
            return cells;
        }
        public String getIcon()
        {
            return icon;
        }
        public void setIcon(String icon, string hint)
        {
            this.icon = icon;
            this.Hint = hint;
        }
        public String getAnchor()
        {
            return anchor;
        }
        public void setAnchor(String anchor)
        {
            this.anchor = anchor;
        }
    }


    internal class TableModel
    {
        public static TableModel CreateNormalTable(ProfileKnowledgeProvider pkp)
        {
            TableModel model = new TableModel();

            model.DocoImg = "help16.png";
            model.DocoRef = pkp.MakeSpecLink("formats.html#table");
            model.Titles.Add(new Title(null, model.DocoRef, "Name", "The logical name of the element", null, 0));
            model.Titles.Add(new Title(null, model.DocoRef, "Flags", "Information about the use of the element", null, 0));
            model.Titles.Add(new Title(null, model.DocoRef, "Card.", "Minimum and Maximum # of times the the element can appear in the instance", null, 0));
            model.Titles.Add(new Title(null, model.DocoRef, "Type", "Reference to the type of the element", null, 100));
            model.Titles.Add(new Title(null, model.DocoRef, "Description & Constraints", "Additional information about the element", null, 0));

            return model;
        }

        public List<Title> Titles = new List<Title>();
        public List<Row> Rows = new List<Row>();
        public string DocoRef;
        public string DocoImg;
    }
}


