﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Metadata.Reader.Impl;
using JetBrains.ReSharper.Psi;

namespace Machine.Specifications.Runner.ReSharper.Reflection;

public class PsiTypeInfoAdapter(ITypeElement type, IDeclaredType? declaredType = null) : ITypeInfo
{
    public string FullyQualifiedName => type.GetClrName().FullName;

    public bool IsAbstract => type is IModifiersOwner {IsAbstract: true};

    public ITypeInfo? GetContainingType()
    {
        return type.GetContainingType()?.AsTypeInfo();
    }

    public IEnumerable<IFieldInfo> GetFields()
    {
        if (type is not IClass classType)
        {
            return Enumerable.Empty<IFieldInfo>();
        }

        return classType.Fields
            .Where(x => !x.IsStatic)
            .Select(x => x.AsFieldInfo());
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(string typeName, bool inherit)
    {
        return type.GetAttributeInstances(new ClrTypeName(typeName), inherit)
            .Select(x => x.AsAttributeInfo());
    }

    public IEnumerable<ITypeInfo> GetGenericArguments()
    {
        if (declaredType != null)
        {
            var substitution = declaredType.GetSubstitution();

            return substitution.Domain
                .Select(x => substitution.Apply(x).GetScalarType())
                .Where(x => x != null)
                .Select(x => x?.GetTypeElement())
                .OfType<IClass>()
                .Select(x => x.AsTypeInfo());
        }

        if (type is ITypeParametersOwner owner)
        {
            return owner.TypeParameters.Select(x => x.AsTypeInfo());
        }

        return Enumerable.Empty<ITypeInfo>();
    }
}
