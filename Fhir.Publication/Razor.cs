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

namespace Hl7.Fhir.Publication
{
    public static class Razor
    {

        public static void Render(Context context, Stream input, Stream output)
        {
            var reader = new StreamReader(input);
            var writer = new StreamWriter(output);
            
            Assembly assembly = Assemble(reader);
            RazorTemplate<Context> template = CreateTemplateInstance(assembly, context);
            writer.Write(template.Render());
            writer.Flush();
            
        }
       
        public static Assembly Assemble(StreamReader reader)
        {
            RazorTemplateEngine engine = CreateEngine();
            CodeCompileUnit code = engine.GenerateCode(reader).GeneratedCode;
            //string name = CreateAssemblyName();
            Assembly assembly = Compile(code);
            //Assembly assembly = Assembly.LoadFrom(name);
            return assembly;
        }

        public static Assembly Compile(CodeCompileUnit code)
        {
  
            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters(); 
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add(typeof(Razor).Assembly.Location);

            CompilerResults results = codeProvider.CompileAssemblyFromDom(parameters, code);
            if (results.Errors.HasErrors)
            {
                CompilerError error = results.Errors
                                           .OfType<CompilerError>()
                                           .Where(e => !e.IsWarning)
                                           .First();
                
                string s = string.Format("Error Compiling Template: ({0}, {1}) {2}",
                                              error.Line, error.Column, error.ErrorText);
                throw new Exception(s);
            }
            return results.CompiledAssembly;
        }

        public static RazorTemplateEngine CreateEngine()
        {
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language);
            host.DefaultBaseClass = typeof(RazorTemplate<Context>).FullName;
            host.DefaultNamespace = "RazorOutput";
            host.DefaultClassName = "Template";
            host.NamespaceImports.Add("System");
            host.NamespaceImports.Add("System.IO");

            var engine = new RazorTemplateEngine(host);
            return engine;
        }
        
        public static RazorTemplate<Context> CreateTemplateInstance(Assembly assembly, Context context)
        {
            //Type type = assembly.GetType("RazorOutput.Template");
            Type type = assembly.GetExportedTypes().Single(); // there is only one
            RazorTemplate<Context> template = Activator.CreateInstance(type) as RazorTemplate<Context>;

            var property = type.GetProperty("Model");
            property.SetValue(template, context, null);

            return template;
        }

        public static string CreateAssemblyName()
        {
            return String.Format("Temp_{0}.dll", Guid.NewGuid().ToString("N"));
        }
    }

}
