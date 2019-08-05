using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Weavers
{
    public class ImFormsWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var methodref = typeof(ulong?).GetProperty("HasValue").GetMethod;
            var methodref2 = typeof(Exception).GetConstructor(new Type[] { typeof(string) });
            var classmgrtype = this.ModuleDefinition.GetType("ImForms.ImFormsMgr");
            var classmethods = classmgrtype.GetMethods().Where(x => x.IsPublic && x.HasCustomAttributes && x.CustomAttributes.Any(p => p.AttributeType.Name == "CheckIDAttribute"));
            
            {
                foreach (var method in classmethods)
                {
                    method.Body.SimplifyMacros();
                    method.Body.InitLocals = true;
                    method.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(typeof(bool))));
                    method.Body.LocalVarToken = method.Body.Variables.Last().VariableType.MetadataToken;
                    var IL = method.Body.GetILProcessor();
                    var firstinstruction = method.Body.Instructions[0];
                    var secondinstruction = method.Body.Instructions[1];
                    var IL0 = Instruction.Create(OpCodes.Ldarga_S, method.Parameters.Last());
                    var IL1 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref));
                    var IL2 = Instruction.Create(OpCodes.Brtrue_S, secondinstruction);
                    var IL3 = Instruction.Create(OpCodes.Ldstr, $"Called {method.Name} with { method.Parameters.Last().Name} == null. Is ImForms.Fody correctly configured ?");
                    var IL4 = Instruction.Create(OpCodes.Newobj, ModuleDefinition.ImportReference(methodref2));
                    var IL5 = Instruction.Create(OpCodes.Throw);
                    IL.InsertAfter(firstinstruction, IL0);
                    IL.InsertAfter(IL0, IL1);
                    IL.InsertAfter(IL1, IL2);
                    IL.InsertAfter(IL2, IL3);
                    IL.InsertAfter(IL3, IL4);
                    IL.InsertAfter(IL4, IL5);
                    method.Body.OptimizeMacros();
                    method.Body.Optimize();
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
