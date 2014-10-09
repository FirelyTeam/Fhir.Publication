﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Publication
{

    
    public class TestProcessor : IProcessor
    {
        Document template;

        public TestProcessor(Context context, string filename)
        {
            template = Document.CreateInContext(context, filename);
        }

        public void Process(Document source, Stage stage)
        {
            string t = template.Text;
            string s = source.Text;
            Document d = stage.CloneAndPost(source);
            d.Text = t + s;
        }
    }

    public class Tester
    {
        public void Test(Context context)
        {
            Document.CreateInContext(context, "template.html");
        }
    }

    

      

   
}