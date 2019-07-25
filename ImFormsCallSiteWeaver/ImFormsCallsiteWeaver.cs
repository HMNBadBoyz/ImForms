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
            var newguidmethod = typeof(Guid).GetMethod("NewGuid");
            var guidtype = typeof(Guid);
            var nullableguidtype = typeof(Guid?);
            var tagaatr = typeof(TagAttribute).GetConstructor(Type.EmptyTypes);
            var nullableguidconstructor = typeof(Guid?).GetConstructor(new[] { typeof(Guid) });
            var allmethods = this.ModuleDefinition.GetAllTypes().SelectMany(x => x.Methods.AsEnumerable()).Where(x => x.HasBody);
            var imformsclassmethods = typeof(ImFormsMgr).GetMethods().Where(x => x.IsPublic && x.CustomAttributes.Any(p => p.AttributeType.Name == "CheckIDAttribute")).Select(x => ModuleDefinition.ImportReference(x));
            var calledmethods = new List<string>();
            if (allmethods.Count() > 0)
            {
                var fieldinsdeict = new Dictionary<Instruction, FieldDefinition>();
                foreach (var method in allmethods)
                {
                    
                    var IL = method.Body.GetILProcessor();
                    method.Body.SimplifyMacros();
                    var imformsinstructions = method.Body.Instructions.Where(x => x.OpCode == OpCodes.Callvirt)
                        .Where(x => imformsclassmethods.Any(y => y.FullName == (x.Operand as MethodReference).FullName));
                    if (imformsinstructions.Count() > 0)
                    {
                        var methodclass = method.DeclaringType.DeclaringType;
                        var hascctor = methodclass.Methods.Any(x => x.Name == ".cctor");
                        if (!hascctor)
                        {
                            methodclass.IsBeforeFieldInit = false;
                            var cctormethoddef = new MethodDefinition(".cctor",  MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static , TypeSystem.VoidReference);
                            cctormethoddef.Body = new MethodBody(cctormethoddef);
                            cctormethoddef.Body.GetILProcessor().Emit(OpCodes.Ret);
                            cctormethoddef.Body.Optimize();
                            methodclass.Methods.Add(cctormethoddef);
                        }
                        var cctormethod = methodclass.Methods.Where(x => x.Name == ".cctor").Single();
                        var cctormethodIL = cctormethod.Body.GetILProcessor();
                        var cctorretIL = cctormethod.Body.Instructions.Last();
                        foreach (var imins in imformsinstructions)
                        {
                            var methodref = (imins.Operand as MethodReference);
                            var field = new FieldDefinition("ImFormsCallsiteID_" + methodclass.Name + "__" + method.Name + "__" + imins.Offset, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, ModuleDefinition.ImportReference(guidtype));
                            methodclass.Fields.Add(field);
                            fieldinsdeict.Add(imins, field);
                            var CCIL0 = cctormethodIL.Create(OpCodes.Call,ModuleDefinition.ImportReference( newguidmethod));
                            var CCIL1 = cctormethodIL.Create(OpCodes.Stsfld,field);
                            cctormethodIL.InsertBefore(cctorretIL,CCIL0);
                            cctormethodIL.InsertAfter(CCIL0,CCIL1);
                            
                           
                        }
                        cctormethodIL.Body.Optimize();

                    }

                    if (imformsinstructions.Count() > 0)
                    {
                       var methodclass = method.DeclaringType.DeclaringType;
                       foreach (var imins in imformsinstructions)
                       {

                           var methodref = (imins.Operand as MethodReference);
                            if (fieldinsdeict.TryGetValue(imins,out var field))
                            {
                                var IL0 = IL.Create(OpCodes.Ldsfld, ( field as FieldReference));
                                var IL1 = IL.Create(OpCodes.Newobj, ModuleDefinition.ImportReference(nullableguidconstructor));
                                //calledmethods.Add(imins.ToString());
                                //calledmethods.Add(IL0.ToString());
                                //calledmethods.Add(IL1.ToString());
                                IL.InsertBefore(imins,IL0);
                                IL.InsertBefore(IL0, IL1);
                                IL.Remove(imins.Previous.Previous.Previous);
                                IL.Remove(imins.Previous.Previous.Previous);
                                IL.Remove(imins.Previous.Previous.Previous);
                                
                            }
                            method.Body.Optimize();
                            method.Body.OptimizeMacros();
                       }
                    }
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
