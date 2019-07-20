﻿using System;
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
    public class ModuleWeaver : BaseModuleWeaver
    {

        public override void Execute()
        {
            var methodref = typeof(Guid?).GetProperty("HasValue").GetMethod;
            var methodref2 = typeof(Guid).GetMethod("NewGuid");
            var methodref3 = typeof(Guid?).GetConstructor(new Type[] { typeof(Guid) });
            var classmgrtype = this.ModuleDefinition.GetType("ImForms.ImFormsMgr");
            var classmethods = classmgrtype.GetMethods().Where(x => x.IsPublic && x.HasCustomAttributes && x.CustomAttributes.Any(p => p.AttributeType.Name == "GenID"));
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
                var IL0 = Instruction.Create(OpCodes.Ldarga_S, method.Parameters.Last());
                IL.InsertAfter(firstinstruction, IL0);
                var IL1 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref));
                IL.InsertAfter(IL0, IL1);
                var IL2 = Instruction.Create(OpCodes.Ldc_I4_0);
                IL.InsertAfter(IL1, IL2);
                var IL3 = Instruction.Create(OpCodes.Ceq);
                IL.InsertAfter(IL2, IL3);
                var IL4 = Instruction.Create(OpCodes.Stloc_0);
                IL.InsertAfter(IL3, IL4);
                var IL5 = Instruction.Create(OpCodes.Ldloc_0);
                IL.InsertAfter(IL4, IL5);
                var IL6 = Instruction.Create(OpCodes.Brfalse_S, secondinstruction);
                IL.InsertAfter(IL5, IL6);
                IL.InsertAfter(IL6, IL0);
                var IL7 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref2));
                IL.InsertAfter(IL6, IL7);
                var IL8 = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(methodref3));
                IL.InsertAfter(IL7, IL8);
                method.Body.OptimizeMacros();
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "ImForms";
            yield return "mscorlib";
        }

        string GetNamespace()
        {
            var namespaceFromConfig = GetNamespaceFromConfig();
            var namespaceFromAttribute = GetNamespaceFromAttribute();
            if (namespaceFromConfig != null && namespaceFromAttribute != null)
            {
                throw new WeavingException("Configuring namespace from both Config and Attribute is not supported.");
            }

            if (namespaceFromAttribute != null)
            {
                return namespaceFromAttribute;
            }

            return namespaceFromConfig;
        }

        string GetNamespaceFromConfig()
        {
            var attribute = Config?.Attribute("Namespace");
            if (attribute == null)
            {
                return null;
            }

            var value = attribute.Value;
            ValidateNamespace(value);
            return value;
        }

        string GetNamespaceFromAttribute()
        {
            var attributes = ModuleDefinition.Assembly.CustomAttributes;
            var namespaceAttribute = attributes
                .SingleOrDefault(x => x.AttributeType.FullName == "NamespaceAttribute");
            if (namespaceAttribute == null)
            {
                return null;
            }

            attributes.Remove(namespaceAttribute);
            var value = (string)namespaceAttribute.ConstructorArguments.First().Value;
            ValidateNamespace(value);
            return value;
        }

        static void ValidateNamespace(string value)
        {
            if (value is null || string.IsNullOrWhiteSpace(value))
            {
                throw new WeavingException("Invalid namespace");
            }
        }

        void AddConstructor(TypeDefinition newType)
        {
            var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            var method = new MethodDefinition(".ctor", attributes, TypeSystem.VoidReference);
            var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, objectConstructor);
            processor.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }

        void AddHelloWorld(TypeDefinition newType , string Methods)
        {
            var method = new MethodDefinition("World", MethodAttributes.Public, TypeSystem.StringReference);
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldstr, Methods);
            processor.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }

        public override bool ShouldCleanReference => true;
    }
}
