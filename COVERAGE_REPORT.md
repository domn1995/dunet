# Code Coverage Report for Dunet Projects

**Report Generated:** January 27, 2026  
**Coverage Tool:** Coverlet with ReportGenerator  
**Test Framework:** xUnit

## Executive Summary

The Dunet solution consists of two main projects:
- **Dunet**: Runtime library containing the Union attribute and diagnostic suppressors
- **Dunet.Generator**: Source generator for creating discriminated unions

### Overall Coverage Statistics

| Metric | Coverage | Details |
|--------|----------|---------|
| **Line Coverage** | **92.5%** | 934 of 1,009 lines covered |
| **Branch Coverage** | **76.8%** | 243 of 316 branches covered |
| **Method Coverage** | **92.4%** | 86 of 93 methods covered |
| **Full Method Coverage** | **78.4%** | 73 of 93 methods fully covered |

### Per-Project Coverage

| Project | Line Coverage | Summary |
|---------|---------------|---------|
| **Dunet** | **86.4%** | 22 uncovered lines |
| **Dunet.Generator** | **93.7%** | 53 uncovered lines |

## Detailed Coverage Analysis by Component

### 1. Dunet Project (86.4% coverage)

#### 1.1 UnionSwitchExpressionDiagnosticSupressor (86.4% coverage)
**Purpose:** Suppresses CS8509 warnings for exhaustive switch expressions on union types.

**Uncovered Lines:** 22 lines (Lines: 27-28, 32-33, 41-42, 59-60, 96-98, 103-105, 116, 127, 238-239, 241-243, 245)

**Missing Coverage Details:**
- **Early return paths** (Lines 27-28, 32-33, 41-42, 59-60): Edge cases where diagnostic analysis fails gracefully
  - When source tree is null
  - When node is not a switch expression
  - When type information is unavailable
  - When type is not a union
  
- **Null handling logic** (Lines 96-98, 103-105): Code paths for handling nullable union types
  - `null` constant pattern handling
  - Discard/var pattern handling for null cases
  
- **Type pattern matching** (Line 116, 127): Alternative pattern matching approaches
  - Constant pattern with named types
  - Declaration pattern symbol resolution failures
  
- **Subpattern validation** (Lines 238-239, 241-245): Complex positional pattern deconstruction
  - DeclarationPatternSyntax type checking
  - Parameter type equality validation

### 2. Dunet.Generator Project (93.7% coverage)

#### 2.1 UnionGenerator (88.8% coverage)
**Purpose:** Main incremental source generator for union types.

**Uncovered Lines:** 8 lines (Lines 41-42, 52-53, 75-76, 83-84)

**Missing Coverage Details:**
- **Cancellation handling** (Lines 41-42, 52-53, 75-76): Cancellation token checks during source generation
- **Null symbol handling** (Lines 83-84): Edge case when record symbol cannot be resolved

#### 2.2 RecordDeclarationSyntaxParser (94.4% coverage)
**Purpose:** Parses record declarations to extract union metadata.

**Uncovered Lines:** 4 lines (Lines 151-152, 159-160)

**Missing Coverage Details:**
- **Top-level statement handling** (Lines 151-152): Synthetic program class filtering
- **Non-partial parent validation** (Lines 159-160): Nested union in non-partial parent type

#### 2.3 UnionExtensionsSourceBuilder (91.5% coverage)
**Purpose:** Generates async match extension methods for union types.

**Uncovered Lines:** 16 lines (Lines 14-17, 103-105, 163-165, 230-232, 286-288)

**Missing Coverage Details:**
- **Null namespace validation** (Lines 14-17): Exception when union has no namespace
- **Generic type constraints** (Lines 103-105, 163-165, 230-232, 286-288): Output of type parameter constraints for:
  - MatchAsync methods with Funcs
  - MatchAsync methods with Actions
  - Match{Variant}Async methods with Funcs
  - Match{Variant}Async methods with Actions

#### 2.4 ImmutableEquatableArray&lt;T&gt; (55.8% coverage)
**Purpose:** Immutable list with sequence equality for incremental generator caching.

**Uncovered Lines:** 15 lines (Lines 29, 32, 35-40, 43, 46-48, 50-51, 57)

**Missing Coverage Details:**
- **Equality methods** (Lines 29, 32): `Equals` implementations for comparison
- **Hash code generation** (Lines 35-40, 43, 46-48, 50-51): Custom hash code calculation
- **IEnumerable interface** (Line 57): Non-generic enumerator

#### 2.5 SyntaxExtensions (77.4% coverage)
**Purpose:** Extension methods for Roslyn syntax analysis.

**Uncovered Lines:** 7 lines (Lines 25, 56, 58, 60-63)

**Missing Coverage Details:**
- **Empty using directives** (Line 25): When syntax tree root is not CompilationUnitSyntax
- **Accessibility keyword mapping** (Lines 56, 58, 60-63): Rare accessibility levels:
  - `ProtectedOrInternal`/`ProtectedOrFriend`
  - `Protected`
  - `ProtectedAndInternal`/`ProtectedAndFriend`
  - `NotApplicable` and default cases

#### 2.6 TypeParameter & TypeParameterConstraint (50% coverage each)
**Purpose:** Data records for type parameter metadata.

**Uncovered Lines:** 1 line each (Lines 71, 99)

**Missing Coverage Details:**
- **ToString methods**: Both types have uncovered `ToString()` implementations used for code generation

#### 2.7 IdentifierExtensions (92.8% coverage)
**Purpose:** Converts identifiers to different naming conventions.

**Uncovered Lines:** 1 line (Line 20)

**Missing Coverage Details:**
- **Edge case handling**: When string length is less than 2 in `ToMethodParameterCase`

## Prioritized Coverage Gaps (Most to Least Important)

### Priority 1: CRITICAL - Core Functionality
These gaps could hide bugs in primary use cases and should be addressed first.

1. **UnionSwitchExpressionDiagnosticSupressor - Null handling** (Lines 96-98, 103-105)
   - **Why:** Nullable unions are a common pattern. Missing tests here could lead to incorrect compiler warnings.
   - **Risk:** High - Could suppress warnings incorrectly or fail to suppress when appropriate.
   - **Impact:** User-facing compilation experience.

2. **UnionSwitchExpressionDiagnosticSupressor - Subpattern validation** (Lines 238-245)
   - **Why:** Positional pattern matching is a key feature for deconstructing union variants.
   - **Risk:** High - Incorrect implementation could lead to false positives/negatives in exhaustiveness checking.
   - **Impact:** Core type safety guarantees.

### Priority 2: HIGH - Error Paths and Edge Cases
Important for robustness but less likely to be encountered in normal usage.

3. **RecordDeclarationSyntaxParser - Non-partial parent validation** (Lines 159-160)
   - **Why:** Ensures proper error handling for misconfigured nested unions.
   - **Risk:** Medium - Could allow invalid code generation or produce confusing errors.
   - **Impact:** Developer experience with nested unions.

4. **UnionExtensionsSourceBuilder - Null namespace validation** (Lines 14-17)
   - **Why:** Validates assumptions about union structure.
   - **Risk:** Medium - Should never happen in practice, but good defensive programming.
   - **Impact:** Generator stability.

5. **UnionSwitchExpressionDiagnosticSupressor - Early return paths** (Lines 27-28, 32-33, 41-42, 59-60)
   - **Why:** Graceful handling of malformed or incomplete code during analysis.
   - **Risk:** Medium - Edge cases during development or incremental compilation.
   - **Impact:** IDE experience and robustness.

6. **SyntaxExtensions - Rare accessibility levels** (Lines 56, 58, 60-63)
   - **Why:** Ensures all accessibility modifiers are correctly handled.
   - **Risk:** Medium - Less common modifiers like `protected internal` could be used.
   - **Impact:** Code generation correctness for all access levels.

### Priority 3: MEDIUM - Generic Type Constraints
Important for generic unions but well-isolated functionality.

7. **UnionExtensionsSourceBuilder - Type parameter constraints** (Lines 103-105, 163-165, 230-232, 286-288)
   - **Why:** Ensures generic type constraints are properly propagated to extension methods.
   - **Risk:** Low-Medium - Constrained generics are less common but important when used.
   - **Impact:** Correctness of generated extension methods for constrained generic unions.

8. **TypeParameter & TypeParameterConstraint - ToString methods** (Lines 71, 99)
   - **Why:** Used in code generation for type parameters.
   - **Risk:** Low - Simple formatting methods.
   - **Impact:** String representation in generated code.

### Priority 4: LOW - Internal Implementation Details
Nice to have but low risk.

9. **ImmutableEquatableArray&lt;T&gt; - Equality and hashing** (Lines 29, 32, 35-51, 57)
   - **Why:** Critical for incremental generator performance but well-tested pattern.
   - **Risk:** Low - Standard equality pattern; bugs would likely be caught by generator behavior.
   - **Impact:** Incremental compilation performance.

10. **IdentifierExtensions - Edge case** (Line 20)
    - **Why:** Handles identifiers shorter than 2 characters.
    - **Risk:** Very Low - Single character identifiers are extremely rare.
    - **Impact:** Naming convention edge case.

### Priority 5: VERY LOW - Infrastructure
Unlikely to cause issues in practice.

11. **UnionGenerator - Cancellation handling** (Lines 41-42, 52-53, 75-76)
    - **Why:** Allows clean cancellation of source generation.
    - **Risk:** Very Low - Cancellation is framework-handled; missing coverage doesn't indicate bugs.
    - **Impact:** Generator responsiveness during cancellation.

12. **RecordDeclarationSyntaxParser - Top-level statement handling** (Lines 151-152)
    - **Why:** Filters synthetic classes from top-level statements.
    - **Risk:** Very Low - Very specific C# 9+ feature interaction.
    - **Impact:** Edge case handling.

13. **UnionGenerator - Null symbol handling** (Lines 83-84)
    - **Why:** Defensive check for compiler issues.
    - **Risk:** Very Low - Should never happen with valid syntax.
    - **Impact:** Generator robustness.

14. **SyntaxExtensions - Empty using directives** (Line 25)
    - **Why:** Handles non-standard compilation unit structures.
    - **Risk:** Very Low - Rare edge case in syntax tree structure.
    - **Impact:** Using directive handling.

15. **UnionSwitchExpressionDiagnosticSupressor - Alternative pattern matching** (Lines 116, 127)
    - **Why:** Fallback paths for type pattern resolution.
    - **Risk:** Very Low - Backup paths for symbol resolution.
    - **Impact:** Pattern matching robustness.

## Recommendations

### Immediate Actions (Priority 1-2)
1. Add tests for nullable union switch expression handling
2. Add tests for positional pattern deconstruction with various subpattern types
3. Add tests for nested unions in non-partial parent types
4. Add tests for unions with various accessibility modifiers (protected internal, private protected)

### Short-term Actions (Priority 3-4)
5. Add tests for generic unions with complex type constraints
6. Add tests for ImmutableEquatableArray equality and hashing behavior
7. Add edge case tests for identifier naming conventions

### Long-term Improvements (Priority 5)
8. Add integration tests that trigger cancellation during generation
9. Add tests for unusual syntax tree structures
10. Consider property-based testing for edge cases in pattern matching

## Testing Gaps Summary

The current test suite of 185 tests provides excellent coverage (92.5% line coverage) for the primary use cases. The missing coverage is primarily in:

1. **Error handling and edge cases** - Defensive code paths that handle malformed input
2. **Nullable type handling** - Tests for nullable unions in switch expressions
3. **Complex pattern matching** - Positional patterns with nested deconstruction
4. **Rare accessibility modifiers** - Less common access levels
5. **Cancellation scenarios** - Generator cancellation handling

The high branch coverage (76.8%) indicates good testing of conditional logic, though there's room for improvement in testing alternative code paths.

## Conclusion

The Dunet project demonstrates strong test coverage with 92.5% line coverage and 76.8% branch coverage. The core functionality is well-tested, with most uncovered code in error handling, edge cases, and rarely-used features. 

**Key Strengths:**
- Core union generation and matching logic is thoroughly tested
- Most common usage patterns are covered
- Good separation of concerns enables targeted testing

**Key Opportunities:**
- Enhance testing for nullable unions and null pattern handling
- Add tests for positional pattern deconstruction scenarios
- Cover rare accessibility modifiers and edge cases
- Improve testing of type constraint propagation

The prioritization above helps focus testing efforts on areas that provide the most confidence in correctness, starting with nullable type handling and complex pattern matching that users are most likely to encounter.
