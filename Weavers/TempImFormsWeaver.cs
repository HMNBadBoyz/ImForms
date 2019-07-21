using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Weavers
{
    public class TempImFormsWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var methodref = typeof(Guid?).GetProperty("HasValue").GetMethod;
            var methodref2 = typeof(Guid).GetMethod("NewGuid");
            var methodref3 = typeof(Guid?).GetConstructor(new Type[] { typeof(Guid) });
            var classmgrtype = this.ModuleDefinition.GetType("ImForms.ImFormsMgr");
            var classmethods = classmgrtype.GetMethods().Where(x => x.IsPublic && x.HasCustomAttributes && x.CustomAttributes.Any(p => p.AttributeType.Name == "GenIDAttribute"));
            foreach (var method in classmethods)
            {
                method.Body.SimplifyMacros();
                method.CustomAttributes.Clear();
                method.Body.InitLocals = true;
                method.Body.Variables.Add(new VariableDefinition(ModuleDefinition.ImportReference(typeof(bool))));
                method.Body.LocalVarToken = method.Body.Variables.Last().VariableType.MetadataToken;
                var IL = method.Body.GetILProcessor();
                var firstinstruction = method.Body.Instructions[0];
                var secondinstruction = method.Body.Instructions[1];
                var idparamindex = method.Body.Variables.Count - 1;
                var stloc = OpCodes.Stloc;
                var ldloc = OpCodes.Ldloc;
                switch (idparamindex)
                {
                    case 0:
                        stloc = OpCodes.Stloc_0;
                        ldloc = OpCodes.Ldloc_0;
                        break;
                    case 1:
                        stloc = OpCodes.Stloc_1;
                        ldloc = OpCodes.Ldloc_1;
                        break;
                    case 2:
                        stloc = OpCodes.Stloc_2;
                        ldloc = OpCodes.Ldloc_2;
                        break;
                    case 3:
                        stloc = OpCodes.Stloc_3;
                        ldloc = OpCodes.Ldloc_3;
                        break;
                    default:
                        stloc = OpCodes.Stloc_S;
                        ldloc = OpCodes.Ldloc_S;
                        break;
                }
                var IL0 = Instruction.Create(OpCodes.Ldarga_S, method.Parameters.Last());
                var IL1 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref));
                var IL2 = Instruction.Create(OpCodes.Ldc_I4_0);
                var IL3 = Instruction.Create(OpCodes.Ceq);
                Instruction IL4;
                Instruction IL5;
                if (idparamindex <= 3)
                {
                    IL4 = Instruction.Create(stloc);
                    IL5 = Instruction.Create(ldloc);
                }
                else
                {
                    IL4 = Instruction.Create(stloc, method.Body.Variables.Last());
                    IL5 = Instruction.Create(ldloc, method.Body.Variables.Last());
                }

                var IL6 = Instruction.Create(OpCodes.Brfalse_S, secondinstruction);
                var IL7 = Instruction.Create(OpCodes.Ldarga_S, method.Parameters.Last());
                var IL8 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref2));
                var IL9 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref3));

                IL.InsertAfter(firstinstruction, IL0);
                IL.InsertAfter(IL0, IL1);
                IL.InsertAfter(IL1, IL2);
                IL.InsertAfter(IL2, IL3);
                IL.InsertAfter(IL3, IL4);
                IL.InsertAfter(IL4, IL5);
                IL.InsertAfter(IL5, IL6);
                IL.InsertAfter(IL6, IL7);
                IL.InsertAfter(IL7, IL8);
                IL.InsertAfter(IL8, IL9);
                method.Body.OptimizeMacros();
                method.Body.Optimize();
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
