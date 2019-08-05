using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CallerInfoEx.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var rngset = new HashSet<long>();
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[64];
            var nullableulongconstructor = typeof(ulong?).GetConstructor(new[] { typeof(ulong) });
            var allmethods = this.ModuleDefinition.GetAllTypes().SelectMany(x => x.Methods.AsEnumerable()).Where(x => x.HasBody ).Where(x=>x.Body.Instructions.Any(p => p.OpCode == OpCodes.Callvirt));
            var allinstructions = allmethods.ToDictionary( t=> t, X => X.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt && (x.Operand as MethodReference).Resolve().HasParameters).Where(x => (x.Operand as MethodReference).Resolve().Parameters.Last().HasCustomAttributes).Where(x => (x.Operand as MethodReference).Resolve().Parameters.Last().CustomAttributes.Any(p => p.AttributeType.Name == "GenIDAttribute")).Reverse()) ;
            var calledmethods = new List<string>();
            calledmethods.AddRange(allinstructions.SelectMany(x=>x.Value).Select(x => (x.Operand as MethodReference).Resolve().ToString()));
            /*
            var file = System.IO.File.CreateText("test.txt");
            foreach (var item in calledmethods)
            {
                file.WriteLine(item);
            }
            file.Flush();
            file.Close();
            */
            foreach (var methodinstructions in allinstructions)
            {
                var method = methodinstructions.Key;
                var IL = method.Body.GetILProcessor();
                method.Body.SimplifyMacros();
                if (methodinstructions.Value.Count() > 0)
                {
                    var methodclass = method.DeclaringType.DeclaringType;
                    foreach (var instruction in methodinstructions.Value)
                    {
                        var methodref = (instruction.Operand as MethodReference);
                        rng.GetBytes(bytes, 0, 64);
                        var randomnumber = BitConverter.ToInt64(bytes, 0);
                        while (rngset.Add(randomnumber))
                        {
                            randomnumber = BitConverter.ToInt64(bytes, 0);
                        }
                        randomnumber = BitConverter.ToInt64(bytes, 0);
                        var IL0 = IL.Create(OpCodes.Ldc_I8, randomnumber);
                        var IL1 = IL.Create(OpCodes.Newobj, ModuleDefinition.ImportReference(nullableulongconstructor));
                        calledmethods.Add(instruction.Operand.ToString());
                        if(method.Body.Variables.Contains(instruction.Previous.Operand as VariableReference))
                        {
                            method.Body.Variables.Remove((instruction.Previous.Operand as VariableReference).Resolve());
                        }
                        IL.Remove(instruction.Previous);
                        IL.Remove(instruction.Previous);
                        IL.Remove(instruction.Previous);
                        IL.InsertBefore(instruction, IL0);
                        IL.InsertBefore(instruction, IL1);
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "netstandard";
        }

        public override bool ShouldCleanReference => true;
    }
}
