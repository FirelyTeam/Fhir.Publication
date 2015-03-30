using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Model;
using Hl7.Fhir.Publication;
using System.IO;
using Hl7.Fhir.Serialization;
using System.Collections.Generic;

namespace Fhir.Profiling.Tests
{
    [TestClass]
    public class RenderValues
    {     
        [TestMethod]
        public void RenderCoding()
        {
            var c = new Coding("http://www.loinc.org/", "23456-5");
            Assert.AreEqual("23456-5@http://www.loinc.org/", c.ForDisplay());

            c.Display = "Testing rendering";
            Assert.AreEqual("23456-5@http://www.loinc.org/ (Testing rendering)", c.ForDisplay());

            var c2 = new Coding("http://www.loinc.org/", "12345-6");
            var cc = new CodeableConcept();
            cc.Coding = new List<Coding>() { c,c2 };            
            Assert.AreEqual("{23456-5@http://www.loinc.org/ (Testing rendering), 12345-6@http://www.loinc.org/}", cc.ForDisplay());

            cc.Text = "Unit test rendering";
            Assert.AreEqual("Unit test rendering {23456-5@http://www.loinc.org/ (Testing rendering), 12345-6@http://www.loinc.org/}", cc.ForDisplay());
        }

        [TestMethod]
        public void RenderIdentifier()
        {
            var i = new Identifier("http://somesystem.nl/ssn", "123456");
            Assert.AreEqual("123456@http://somesystem.nl/ssn", i.ForDisplay());
        }

        [TestMethod]
        public void RenderPeriod()
        {
            var p = new Period { Start = "200401" };
            Assert.AreEqual("[200401,∞]", p.ForDisplay());

            p.End = "20050212";
            Assert.AreEqual("[200401,20050212]", p.ForDisplay());

            p.Start = null;
            Assert.AreEqual("[∞,20050212]", p.ForDisplay());
        }

        [TestMethod]
        public void RenderQuantity()
        {
            var q = new Quantity();

            q.Value = 4.5m;
            q.Code = "kg";
            q.System = "http://unitsofmeasure.org";

            Assert.AreEqual("4.5kg", q.ForDisplay());

            q.Units = "kilo";
            Assert.AreEqual("4.5 kilo", q.ForDisplay());

            q.Comparator = Quantity.QuantityComparator.LessOrEqual;
            Assert.AreEqual("<=4.5 kilo", q.ForDisplay());
        }

        [TestMethod]
        public void RenderRatio()
        {
            var n = new Quantity() { Value = 5.0m, Units = "$" };
            var d = new Quantity() { Value = 1m, Units = "wk" };
            var r = new Ratio() { Numerator = n, Denominator = d };

            Assert.AreEqual("5.0$ : 1wk",r.ForDisplay());
        }

        [TestMethod]
        public void RenderRange()
        {
            var n = new Quantity() { Value = 5.0m, Units = "cm" };
            var r = new Range { Low = n };

            Assert.AreEqual("[5.0cm,∞]", r.ForDisplay());
        }

    }
}
