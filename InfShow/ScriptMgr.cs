using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace InfShow
{
    class ScriptMgr
    {
        Hashtable scripts = new Hashtable();
        List<string> Names = new List<string>();
        public List<string> GetScriptsName()
        {
            return Names;
        }
        public void Load()
        {
            try
            {
                string str = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                string[] files = Directory.GetFiles(str, "*.cs");
                foreach (string f in files)
                {
                    if (f.Length > 3)
                    {
                        string code = File.ReadAllText(f);
                        string n = Path.GetFileNameWithoutExtension(f);

                        Names.Add(n);
                        // Compiler and CompilerParameters
                        CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                        CompilerParameters compParameters = new CompilerParameters();
                        compParameters.ReferencedAssemblies.Add("System.dll");
                        compParameters.GenerateExecutable = false;
                        compParameters.GenerateInMemory = true;

                        // Compile the code
                        CompilerResults res = codeProvider.CompileAssemblyFromSource(compParameters, code);
                        
                        // Create a new instance of the class 'MyClass'　　　　// 有命名空间的，需要命名空间.类名
                        scripts.Add(n, res);
                    }
                }
            }catch(Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        public string RunScript(string script)
        {
            string txt = Clipboard.GetText();
            CompilerResults compiler = scripts[script] as CompilerResults;
            if(compiler!=null)
            {
                object o = compiler.CompiledAssembly.CreateInstance(script);
                object ret = o.GetType().GetMethod("Convert").Invoke(o, new object[] { txt });
                string convertString = ret as string;
                return convertString;
            }
            else
            {
                return null;
            }
        }
    }
}