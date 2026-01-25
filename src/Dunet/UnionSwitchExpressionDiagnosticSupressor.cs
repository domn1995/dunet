using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dunet;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnionSwitchExpressionDiagnosticSupressor : DiagnosticSuppressor
{
    private static readonly SuppressionDescriptor descriptor = new(
        id: "DUNET1",
        suppressedDiagnosticId: "CS8509",
        justification: "The switch expression arms handle all union variants; therefore, a default case is unnecessary."
    );

    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [descriptor];

    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var location = diagnostic.Location;

            if (location.SourceTree is not SyntaxTree tree)
            {
                continue;
            }

            if (tree.GetRoot().FindNode(location.SourceSpan) is not SwitchExpressionSyntax node)
            {
                continue;
            }

            var model = context.GetSemanticModel(tree);
            var switchTypeInfo = model.GetTypeInfo(node.GoverningExpression);
            var type = switchTypeInfo.Type;

            if (type is null)
            {
                continue;
            }

            bool isNullable = type.NullableAnnotation == NullableAnnotation.Annotated;

            var isUnion = type.GetAttributes()
                .Any(static attr =>
                    attr.AttributeClass
                        is {
                            // Must be a class or record decorated with [Union].
                            Name: nameof(UnionAttribute),
                            TypeKind: TypeKind.Class,
                            ContainingNamespace.Name: nameof(Dunet),
                        }
                );

            if (!isUnion)
            {
                continue;
            }

            // Create a set containing all union variants. Whenever we prove that each variant is
            // handled by a switch expression arm, we remove it from the set. If the set is empty
            // at the end, we know that each variant has been handled and that the switch
            // expression is exhaustive.
            var unsatisfiedVariants = new HashSet<INamedTypeSymbol>(
                type.GetTypeMembers()
                    .Where(member =>
                        member.BaseType?.Equals(type, SymbolEqualityComparer.Default) ?? false
                    ),
                SymbolEqualityComparer.Default
            );

            var isNullHandled = false;

            foreach (var arm in node.Arms)
            {
                // If the arm has a when clause, we cannot statically prove exhaustiveness for it,
                // so we skip it and move on.
                if (arm.WhenClause is not null)
                {
                    continue;
                }

                // Check for the `null` constant pattern.
                if (
                    arm.Pattern is ConstantPatternSyntax
                    {
                        Expression: LiteralExpressionSyntax
                        {
                            RawKind: (int)SyntaxKind.NullLiteralExpression
                        }
                    }
                )
                {
                    isNullHandled = true;
                    continue;
                }

                // Check for the discard/var pattern which handles everything, including null.
                if (arm.Pattern is DiscardPatternSyntax or VarPatternSyntax)
                {
                    isNullHandled = true;
                }

                if (arm.Pattern is ConstantPatternSyntax typePattern)
                {
                    var symbol = model.GetSymbolInfo(typePattern.Expression).Symbol;

                    if (symbol is INamedTypeSymbol namedType)
                    {
                        unsatisfiedVariants.Remove(namedType);
                        continue;
                    }
                }

                if (arm.Pattern is DeclarationPatternSyntax { Type: TypeSyntax patternSyntax })
                {
                    var symbol = model.GetSymbolInfo(patternSyntax).Symbol;

                    if (symbol is INamedTypeSymbol namedType)
                    {
                        unsatisfiedVariants.Remove(namedType);
                        continue;
                    }
                }

                if (
                    arm.Pattern is RecursivePatternSyntax
                    {
                        Type: TypeSyntax patternTypeSyntax,
                        PositionalPatternClause: PositionalPatternClauseSyntax
                        {
                            Subpatterns: SeparatedSyntaxList<SubpatternSyntax> subpatterns
                        }
                    }
                )
                {
                    var symbol = model.GetSymbolInfo(patternTypeSyntax).Symbol;

                    if (
                        symbol is INamedTypeSymbol patternType
                        && IsHandledByPositionalPattern(subpatterns, patternType, model)
                    )
                    {
                        unsatisfiedVariants.Remove(patternType);
                        continue;
                    }
                }

                // Property patterns without positional patterns are too complex to determine
                // exhaustiveness, so we do not suppress warnings for them. This errs on the
                // side of caution to avoid false negatives.
                if (
                    arm.Pattern is RecursivePatternSyntax
                    {
                        Type: TypeSyntax,
                        PropertyPatternClause: not null
                    }
                )
                {
                    // Do not suppress - we cannot prove exhaustiveness with property patterns
                    continue;
                }
            }

            // If the type was nullable but the `null` case was not handled, we bail early so we
            // don't erroneously suppress the diagnostic.
            if (isNullable && !isNullHandled)
            {
                break;
            }

            // If all variants have been satisfied, we have proven exhaustiveness and can suppress
            // the diagnostic.
            if (unsatisfiedVariants.Count is 0)
            {
                context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
                break;
            }
        }
    }

    private static bool IsHandledByPositionalPattern(
        SeparatedSyntaxList<SubpatternSyntax> subpatterns,
        INamedTypeSymbol patternType,
        SemanticModel model
    )
    {
        var desconstructors = patternType
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.Name is "Deconstruct" && m.Parameters.Length == subpatterns.Count);

        foreach (var deconstructor in desconstructors)
        {
            bool matched = true;

            for (var i = 0; i < deconstructor.Parameters.Length; ++i)
            {
                var isHandledBySubpattern = IsHandledBySubpattern(
                    subpatterns[i].Pattern,
                    deconstructor.Parameters[i].Type,
                    model
                );

                if (!isHandledBySubpattern)
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsHandledBySubpattern(
        PatternSyntax pattern,
        ITypeSymbol paramType,
        SemanticModel model
    )
    {
        if (pattern is DiscardPatternSyntax or VarPatternSyntax)
        {
            return true;
        }

        if (pattern is DeclarationPatternSyntax { Type: TypeSyntax typeSyntax })
        {
            var deconstructParamType = model.GetTypeInfo(typeSyntax).Type;

            if (paramType.Equals(deconstructParamType, SymbolEqualityComparer.Default))
            {
                return true;
            }
        }

        return false;
    }
}
