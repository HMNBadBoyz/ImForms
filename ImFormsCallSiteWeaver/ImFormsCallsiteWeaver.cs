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
            var allmethodnames = string.Join(" , ", calledmethods);
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

    public class TagAttribute : Attribute
    {
    }
}
