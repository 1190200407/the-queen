using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

string assemblyPath = args.Length > 0
    ? args[0]
    : @"D:\SteamLibrary\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64\sts2.dll";

using FileStream stream = File.OpenRead(assemblyPath);
using PEReader peReader = new(stream);
MetadataReader reader = peReader.GetMetadataReader();
StringSignatureProvider provider = new(reader);

foreach (TypeDefinitionHandle typeHandle in reader.TypeDefinitions)
{
    TypeDefinition type = reader.GetTypeDefinition(typeHandle);
    string typeName = reader.GetString(type.Name);
    string typeNamespace = reader.GetString(type.Namespace);
    if (typeName is not ("CardModel" or "Hook" or "ModCardTemplate" or "QueenCardModel" or "CreatureCmd" or "CardPileCmd" or "CardCreationOptions" or "CardFactory"))
    {
        continue;
    }

    Console.WriteLine($"{typeNamespace}.{typeName}");
    foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
    {
        MethodDefinition method = reader.GetMethodDefinition(methodHandle);
        string methodName = reader.GetString(method.Name);
        if (typeName is "CreatureCmd" && methodName != "Damage")
        {
            continue;
        }

        if (typeName is "CardPileCmd" && methodName != "Add")
        {
            continue;
        }

        if (typeName is "CardFactory" && !methodName.Contains("Create", StringComparison.Ordinal))
        {
            continue;
        }

        if (typeName is not ("CreatureCmd" or "CardPileCmd" or "CardCreationOptions" or "CardFactory")
            && !methodName.Contains("ResultPile", StringComparison.Ordinal)
            && !methodName.Contains("CardPlay", StringComparison.Ordinal)
            && methodName != "ModifyDamage")
        {
            continue;
        }

        MethodSignature<string> signature = method.DecodeSignature(provider, genericContext: null);
        List<string> parameterNames = [];
        foreach (ParameterHandle parameterHandle in method.GetParameters())
        {
            Parameter parameter = reader.GetParameter(parameterHandle);
            if (parameter.SequenceNumber > 0)
            {
                parameterNames.Add(reader.GetString(parameter.Name));
            }
        }

        string parameters = string.Join(", ", signature.ParameterTypes.Zip(parameterNames, (typeText, name) => $"{typeText} {name}"));
        Console.WriteLine($"  {signature.ReturnType} {methodName}({parameters})");
    }
}

internal sealed class StringSignatureProvider : ISignatureTypeProvider<string, object?>
{
    private readonly MetadataReader reader;

    public StringSignatureProvider(MetadataReader reader)
    {
        this.reader = reader;
    }

    public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[]";
    public string GetByReferenceType(string elementType) => $"{elementType}&";
    public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => $"{genericType}<{string.Join(", ", typeArguments)}>";
    public string GetGenericMethodParameter(object? genericContext, int index) => $"!!{index}";
    public string GetGenericTypeParameter(object? genericContext, int index) => $"!{index}";
    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
    public string GetPinnedType(string elementType) => elementType;
    public string GetPointerType(string elementType) => $"{elementType}*";
    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
    public string GetSZArrayType(string elementType) => $"{elementType}[]";
    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        TypeDefinition type = reader.GetTypeDefinition(handle);
        string ns = reader.GetString(type.Namespace);
        string name = reader.GetString(type.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        TypeReference type = reader.GetTypeReference(handle);
        string ns = reader.GetString(type.Namespace);
        string name = reader.GetString(type.Name);
        return string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
    }

    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
        reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
}
