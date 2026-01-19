using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dunet.Generator.UnionGeneration;

/// <summary>
/// Retrieves semantic information from record declarations.
/// </summary>
internal static class RecordDeclarationSyntaxParser
{
    extension(RecordDeclarationSyntax self)
    {
        /// <summary>
        /// Gets the type parameters of this record declaration.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <returns>The sequence of type parameters, if any. Otherwise, <see langword="null"/>.</returns>
        public IEnumerable<TypeParameter> GetTypeParameters() =>
            self.TypeParameterList?.Parameters.Select(static typeParam => new TypeParameter(
                typeParam.Identifier.ToString()
            ))
            ?? [];

        /// <summary>
        /// Gets the type parameter constraints of this record declaration.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <returns>The sequence of type parameter constraints, if any. Otherwise, <see langword="null"/>.</returns>
        public IEnumerable<TypeParameterConstraint> GetTypeParameterConstraints() =>
            self.ConstraintClauses.Select(static constraint => new TypeParameterConstraint(
                constraint.ToString()
            ));

        /// <summary>
        /// Gets the parameters in this record's primary constructor.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
        /// <returns>The sequence of parameters, if any. Otherwise, <see langword="null"/>.</returns>
        public IEnumerable<Parameter> GetParameters(SemanticModel semanticModel) =>
            self.ParameterList?.Parameters.Select(parameter => new Parameter(
                Type: new ParameterType(
                    Identifier: parameter.Type?.ToString() ?? "",
                    IsInterface: parameter.Type.IsInterfaceType(semanticModel)
                ),
                Identifier: parameter.Identifier.ToString()
            ))
            ?? [];

        /// <summary>
        /// Gets the properties declared in this record.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
        /// <returns>The sequence of properties.</returns>
        public IEnumerable<Property> GetProperties(SemanticModel semanticModel) =>
            self
                .Members.OfType<PropertyDeclarationSyntax>()
                .Select(propertyDeclaration => new Property(
                    Type: new PropertyType(
                        Identifier: propertyDeclaration.Type.ToString(),
                        IsInterface: propertyDeclaration.Type.IsInterfaceType(semanticModel)
                    ),
                    Identifier: propertyDeclaration.Identifier.ToString(),
                    IsRequired: propertyDeclaration.Modifiers.Any(static modifier =>
                        modifier.Value is "required"
                    )
                ));

        /// <summary>
        /// Gets the record declarations within this record declaration.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
        /// <returns>The sequence of nested record declarations, if any. Otherwise, <see langword="null"/>.</returns>
        public IEnumerable<VariantDeclaration> GetNestedRecordDeclarations(
            SemanticModel semanticModel
        ) =>
            self.DescendantNodes()
                .Where(static node => node.IsKind(SyntaxKind.RecordDeclaration))
                .OfType<RecordDeclarationSyntax>()
                .Select(nestedRecord => new VariantDeclaration()
                {
                    Identifier = nestedRecord.Identifier.ToString(),
                    TypeParameters = nestedRecord.GetTypeParameters().ToImmutableEquatableArray(),
                    Parameters = nestedRecord
                        .GetParameters(semanticModel)
                        .ToImmutableEquatableArray(),
                });

        /// <summary>
        /// Determines whether this record declaration is decorated by this library's marker attribute:
        /// <see cref="UnionAttributeSource.FullyQualifiedName"/>.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
        /// <returns>
        /// A boolean <see langword="true"/> if decorated with the union attribute; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsDecoratedWithUnionAttribute(SemanticModel semanticModel)
        {
            var getDecoratedType = (AttributeSyntax attributeSyntax) =>
                semanticModel.GetSymbolInfo(attributeSyntax).Symbol?.ContainingType;

            return self
                .AttributeLists.SelectMany(static attributeListSyntax =>
                    attributeListSyntax.Attributes
                )
                .Select(getDecoratedType)
                .Select(static attributeSymbol => attributeSymbol?.ToDisplayString())
                .Any(static attributeName => attributeName is "Dunet.UnionAttribute");
        }

        /// <summary>
        /// Gets a stack representation of the types that nest this record declaration.
        /// </summary>
        /// <param name="record">This record declaration.</param>
        /// <param name="semanticModel">The semantic model associated with this record declaration.</param>
        /// <returns>
        /// A stack containing the types surrounding this record declaration. The type declared closest
        /// to this record declaration will be at the top of the stack, and the type declared closest
        /// to the namespace will be at the bottom of the stack.
        /// </returns>
        public Stack<ParentType> GetParentTypes(SemanticModel semanticModel)
        {
            var parentTypes = new Stack<ParentType>();
            RecursivelyAddParentTypes(semanticModel, self, parentTypes);
            return parentTypes;
        }
    }

    private static void RecursivelyAddParentTypes(
        SemanticModel semanticModel,
        SyntaxNode declaration,
        Stack<ParentType> parentTypes
    )
    {
        var parent = declaration.Parent;

        if (parent?.IsClassOrRecordDeclaration() is not true)
        {
            return;
        }

        var parentSymbol = semanticModel.GetDeclaredSymbol(parent);

        // Ignore top level statement synthetic program class.
        if (parentSymbol?.ToDisplayString() is null or "<top-level-statements-entry-point>")
        {
            return;
        }

        var parentDeclaration = (TypeDeclarationSyntax)parent;

        // We can only declare a nested union type within a partial parent type declaration.
        if (!parentDeclaration.IsPartial())
        {
            return;
        }

        var parentType = new ParentType(
            IsRecord: parent.IsRecordDeclaration(),
            Identifier: parentSymbol.Name
        );

        parentTypes.Push(parentType);

        RecursivelyAddParentTypes(semanticModel, parent, parentTypes);
    }
}
