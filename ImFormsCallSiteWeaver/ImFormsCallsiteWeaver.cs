using System;
using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using ImForms;

namespace Weavers
{
    public class ImFormsCallSiteWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            var newguidmethod = typeof(Guid).GetMethod("NewGuid");
            var guidtype = typeof(Guid);
            var nullableguidconstructor = typeof(Guid?).GetConstructor(new Type[] { typeof(Guid) });
            var allmethods = this.ModuleDefinition.GetAllTypes().SelectMany(x => x.Methods.AsEnumerable()).Where(x => x.HasBody);
            var imformsclassmethods = typeof(ImFormsMgr).GetMethods().Where(x => x.IsPublic && x.CustomAttributes.Any(p => p.AttributeType.Name == "CheckIDAttribute")).Select(x => ModuleDefinition.ImportReference(x));
            var calledmethods = new List<string>();
            if (allmethods.Count() > 0)
            {
                foreach (var method in allmethods)
                {
                    var IL = method.Body.GetILProcessor();
                    var imformsinstructions = method.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt)
                        .Where(x => imformsclassmethods.Any(y => y.FullName == (x.Operand as MethodReference).FullName));
                    foreach (var imins in imformsinstructions)
                    {
                        var methodref = (imins.Operand as MethodReference);
                        calledmethods.Add(methodref.FullName);
                        var startinstruction = imins.Previous.Previous.Previous.Previous.Previous.Previous;
                        var IL0 = IL.Create(OpCodes.Ldarg_0);
                        var IL1 = IL.Create(OpCodes.Call, ModuleDefinition.ImportReference(newguidmethod));
                        //var IL2 = IL.Create(OpCodes.Stfld, ModuleDefinition.ImportReference(guidtype));
                        //IL.InsertBefore(startinstruction, IL0);
                        //IL.InsertAfter(IL0, IL1);
                        //IL.InsertAfter(IL1, IL2);

                    }
                }
            }

            var allmethodnames = string.Join(",", calledmethods);
            var ns = GetNamespace();
            var type = new TypeDefinition(ns, "Hello", TypeAttributes.Public, TypeSystem.ObjectReference);

            AddConstructor(type);

            AddHelloWorld(type, allmethodnames);

            ModuleDefinition.Types.Add(type);

        }



        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "ImForms";
            yield return "mscorlib";
            yield return "netstandard";
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

        void AddHelloWorld(TypeDefinition newType, string str)
        {
            var method = new MethodDefinition("World", MethodAttributes.Public, TypeSystem.StringReference);
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldstr, str);
            processor.Emit(OpCodes.Ret);
            newType.Methods.Add(method);
        }

        public override bool ShouldCleanReference => true;
    }

}
