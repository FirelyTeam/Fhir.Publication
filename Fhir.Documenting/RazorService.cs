using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor;

namespace Hl7.Fhir.Documenting
{
    public static class Razor
    {
        public static void RenderFile(string source, string target)
        {
            Stream input = new FileStream(source, FileMode.Open);
            Stream output = File.OpenWrite(target);
            Render(input, output);
        }

        public static void Render(Stream input, Stream output)
        {
            using (var reader = new StreamReader(input))
            using (var writer = new StreamWriter(output))
            {
                Assembly assembly = Assemble(reader);
                RazorTemplate template = CreateTemplateInstance(assembly);
                writer.Write(template.Render());
                writer.Flush();
            }
        }
       
        public static Assembly Assemble(StreamReader reader)
        {
            RazorTemplateEngine engine = CreateEngine();
            CodeCompileUnit code = engine.GenerateCode(reader).GeneratedCode;
            string name = CreateAssemblyName();
            Compile(code, name);
            Assembly assembly = Assembly.LoadFrom(name);
            return assembly;
        }

        public static void Compile(CodeCompileUnit code, string name)
        {
  
            var codeProvider = new CSharpCodeProvider();

            string core = typeof(Razor).Assembly.CodeBase.Replace("file:///", "").Replace("/", "\\");
            var parameters = new CompilerParameters(new string[] { core }, name);

            CompilerResults results = codeProvider.CompileAssemblyFromDom(parameters, code);
            if (results.Errors.HasErrors)
            {
                CompilerError error = results.Errors
                                           .OfType<CompilerError>()
                                           .Where(e => !e.IsWarning)
                                           .First();
                
                string s = string.Format("Error Compiling Template: ({0}, {1}) {2}",
                                              error.Line, error.Column, error.ErrorText);
                Console.WriteLine(s);
            }
        }

        public static RazorTemplateEngine CreateEngine()
        {
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language);
            host.DefaultBaseClass = typeof(RazorTemplate).FullName;
            host.DefaultNamespace = "RazorOutput";
            host.DefaultClassName = "Template";
            host.NamespaceImports.Add("System");

            var engine = new RazorTemplateEngine(host);
            return engine;
        }
        
        public static RazorTemplate CreateTemplateInstance(Assembly assembly)
        {
            Type type = assembly.GetType("RazorOutput.Template");
            RazorTemplate template = Activator.CreateInstance(type) as RazorTemplate;
            return template;
        }

        public static string CreateAssemblyName()
        {
            return String.Format("Temp_{0}.dll", Guid.NewGuid().ToString("N"));
        }
    }

}
