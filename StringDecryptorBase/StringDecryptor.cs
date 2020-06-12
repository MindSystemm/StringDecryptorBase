using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static StringDecryptorBase.Program;

namespace StringDecryptorBase
{
    class StringDecryptor
    {
        //We define our module here to use it in the whole class
        public ModuleDefMD module = null;
        //We also define the main decryption method (I use a list if multiple decryption methods
        // are used
        public List<MethodDef> DecryptionMeth = null;
        //We also define an assembly to invoke method
        public Assembly asm = null;
        public Mode mode = Mode.Dynamic;
        public StringDecryptor(List<MethodDef> decryption, ModuleDefMD moduledef, Assembly assembly, Mode modee)
        {
            DecryptionMeth = decryption;
            module = moduledef;
            asm = assembly;
            mode = modee;
        }
        public void DecryptString(ref int amount)
        {
            //Checking if the method of decryption list is not empty
            if(DecryptionMeth.Count == 0)
            {
                Console.WriteLine("No decryption method found, please input token of meth : ");
            }
            try
            {
                //Removing 0x
                string corrected = Console.ReadLine().Remove(0, 2);
                //Converting token to rid by substracting 0x06000000
                MethodDef found = module.ResolveMethod(uint.Parse(corrected, System.Globalization.NumberStyles.HexNumber) - 0x06000000);
                Console.WriteLine("Found decryption method : {0}", found.Name);
                //If the method wasn't found, we ask to the use to input one
                DecryptionMeth.Add(found);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
          
            //Looping through the type
            foreach (TypeDef type in module.Types)
            {
                //Looping through the methods
                foreach (MethodDef method in type.Methods)
                {
                    //Checking if method.HasBody
                    if (method.HasBody)
                    {
                        //Now we loop through each instruction
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            /*
                            [*] First check -> checking if argument is ldstr (most of the time, ldstr become int 
                            so  you have to adapt this part
                            [*] Second check -> checking if next instr is call (sometimes, the ldstr is not right 
                            after the ldstr, see my toolYano deobfuscator for a more stable version
                            [*] Third check -> checking if the call is a methoddef
                            [*] Fourth check -> checking if the method is a decryption method
                            */
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr && method.Body.Instructions[i+1].OpCode == OpCodes.Call && method.Body.Instructions[i+1].Operand is MethodDef && DecryptionMeth.Contains((MethodDef)method.Body.Instructions[i+1].Operand))
                            {
                                //Getting the encrypted string
                                string argument = method.Body.Instructions[i].Operand.ToString();
                                string resolved;
                                if(mode == Mode.Static)
                                {
                                    //decrypting the string
                                    //STATIC APPROACH
                                    resolved = DecryptString(argument);
                                }
                                else
                                {
                                    //DYNAMIC APPROACH
                                    //We grab the methodDef to get its mdtoken
                                    MethodDef toinvoke = (MethodDef)method.Body.Instructions[i + 1].Operand;
                                    //Then we invoke the methodofdecryption to get the string
                                    //It's more reliable but can be dangerous as it loads the assembly which
                                    //may contains virus
                                    resolved = (string)asm.ManifestModule.ResolveMethod(toinvoke.MDToken.ToInt32()).Invoke(null, new object[] { argument });
                                }
                                //Replacing the decrypted string
                                //Here you can use resolve or resolved2, they should be the same 
                                method.Body.Instructions[i].Operand = resolved;
                                //Nop the call !! not deleting it ! 
                                method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                //Printing the decrypted strings
                                amount++;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Decrypted : {0}", resolved);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }

                    }
                }
            }
        }
        public static string DecryptString(string str)
        {
            //Here you can copy the string decryption routine
            return str;
        }
    }
}
