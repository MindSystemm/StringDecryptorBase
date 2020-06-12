using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StringDecryptorBase
{
    class Program
    {
        //Defining to approaches 
        public enum Mode
        {
            Static,
            Dynamic
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Written for purpose by MindSystem");
            Console.WriteLine("don't forget to give credit if you use it !");
            Mode mode = Mode.Dynamic;
            //Defining a module to inspect
            ModuleDefMD module = null;
            //This is for the dynamic approach, pay attention if can be dangerous to load
            //assembly you do not trust !
            //If the assembly was made using .net core, this code should be adapted ! 
            Assembly asm = null;
            //Loading module from args
            try
            {
                module = ModuleDefMD.Load(args[0]);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return;
            }
            try
            {
                asm = Assembly.LoadFrom(args[0]);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                mode = Mode.Static;
            }
            //Also loading the assembly, if it fails, apply static approach
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Console.WriteLine("Using {0} approach", mode);
            StringDecryptor decryptor = new StringDecryptor(FindDecryptionMethods(module), module, asm, mode);
            int decryptedstring = 0;
            decryptor.DecryptString(ref decryptedstring);
            watch.Stop();
            Console.WriteLine("Done ! Elapsed time : {0}",  watch.Elapsed.TotalSeconds);
            Console.WriteLine("Decrypted : {0}", decryptedstring);
            //Replacing the path 
            string SavingPath =module.Kind == ModuleKind.Dll ?args[0].Replace(".dll", "-Deobfuscated.dll") : args[0].Replace(".exe", "-Deobfuscated.exe");
            //Check to see if asm is mixed mode or not
            if (module.IsILOnly)
            {
                //Saving option
                var opts = new ModuleWriterOptions(module);
                opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                opts.Logger = DummyLogger.NoThrowInstance;
                //Saving the deobfuscated assembly
               module.Write(SavingPath, opts);
            }
            else
            {
                //Same here but for mixed mode assembly
                var opts = new NativeModuleWriterOptions(module, false);
                opts.MetadataOptions.Flags = MetadataFlags.PreserveAll;
                opts.Logger = DummyLogger.NoThrowInstance;
               module.NativeWrite(SavingPath, opts);
            }
            Console.ReadLine();
        }
        public static List<MethodDef> FindDecryptionMethods(ModuleDefMD module)
        {
            //Creating a list of method to return
            List<MethodDef> list = new List<MethodDef>();
            //Looping through the type
            foreach(TypeDef type in module.Types)
            {
                //Looping through the methods
                foreach (MethodDef method in type.Methods)
                {
                    //Checking if method.HasBody
                    if(method.HasBody)
                    {
                        //Now we loop through each instruction
                        for(int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            //Just set here the condition(s)
                            bool condition = false;
                            if (condition)
                            {
                                //Adding the method to the list of decryptino method
                                list.Add(method);
                            }
                        }
                        
                    }
                }
            }
            return list;
        }
    }
}
