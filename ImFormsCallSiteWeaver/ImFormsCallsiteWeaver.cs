using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using ImForms;
using System.Diagnostics;

namespace Weavers
{
    public class ImFormsCallSiteWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var rngset = new HashSet<long>();
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[64];
            var nullableulongconstructor = typeof(ulong?).GetConstructor(new[] { typeof(ulong) });
            var allmethods = this.ModuleDefinition.GetAllTypes().SelectMany(x => x.Methods.AsEnumerable()).Where(x => x.HasBody);
            var imformsclassmethods = typeof(ImFormsMgr).GetMethods().Where(x => x.IsPublic && x.CustomAttributes.Any(p => p.AttributeType.Name == "CheckIDAttribute")).Select(x => ModuleDefinition.ImportReference(x));
            var calledmethods = new List<string>();
            if (allmethods.Count() > 0 )
            {
                foreach (var method in allmethods)
                {

                    var IL = method.Body.GetILProcessor();
                    method.Body.SimplifyMacros();
                    var imformsinstructions = method.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt)
                        .Where(x => imformsclassmethods.Any(y => y.FullName == (x.Operand as MethodReference).FullName)).Reverse();
                    if (imformsinstructions.Count() > 0)
                    {
                        var methodclass = method.DeclaringType.DeclaringType;
                        
                        foreach (var imins in imformsinstructions)
                        {
                            var methodref = (imins.Operand as MethodReference);
                            rng.GetBytes(bytes, 0, 64);
                            var randomnumber = BitConverter.ToInt64(bytes, 0);
                            while(rngset.Add(randomnumber))
                            {
                                randomnumber = BitConverter.ToInt64(bytes, 0);
                            }
                            randomnumber = BitConverter.ToInt64(bytes, 0);
                            var IL0 = IL.Create(OpCodes.Ldc_I8, randomnumber);
                            var IL1 = IL.Create(OpCodes.Newobj, ModuleDefinition.ImportReference(nullableulongconstructor));
                            calledmethods.Add(imins.Operand.ToString());
                            IL.Remove(imins.Previous);
                            IL.Remove(imins.Previous);
                            IL.Remove(imins.Previous);
                            IL.InsertBefore(imins, IL0);
                            IL.InsertBefore(imins, IL1);

                        }

                    }
                    method.Body.Optimize();
                    method.Body.OptimizeMacros();
                }
            }

        }



        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "ImForms";
            yield return "mscorlib";
            yield return "netstandard";
        }

      
        public override bool ShouldCleanReference => true;
    }

    public class TagAttribute : Attribute
    {
    }
}
