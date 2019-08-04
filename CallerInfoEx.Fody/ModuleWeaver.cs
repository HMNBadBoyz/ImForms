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
            var allmethods = this.ModuleDefinition.GetAllTypes().SelectMany(x => x.Methods.AsEnumerable()).Where(x => x.HasBody);
            var allinstructions = allmethods.SelectMany(X => X.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt && (x.Operand as MethodReference).HasParameters)).Where(x => (x.Operand as MethodReference).Parameters.Last().HasCustomAttributes);//.Where(x=> (x.Operand as MethodReference).Parameters.Last().CustomAttributes.Any(p=> p.AttributeType.Name == "GenIDAttribute")).Reverse();
            var calledmethods = new List<string>();
            calledmethods.AddRange(allinstructions.Select(x => x.ToString()));
            var file = System.IO.File.CreateText("test.txt");
            foreach (var item in calledmethods)
            {
                file.WriteLine(item);
            }
            file.Flush();
            file.Close();
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "mscorlib";
            yield return "netstandard";
        }

        public override bool ShouldCleanReference => true;
    }
}
