# Code Coverage Priority Summary

Quick reference guide for addressing coverage gaps in Dunet projects.

**Overall Coverage:** 92.5% lines, 76.8% branches  
**Full Report:** See [COVERAGE_REPORT.md](COVERAGE_REPORT.md)

## Priority Rankings

### 🔴 Priority 1: CRITICAL - Core Functionality

| # | Component | Lines | Description | Risk Level |
|---|-----------|-------|-------------|------------|
| 1 | UnionSwitchExpressionDiagnosticSupressor | 96-98, 103-105 | Null handling for nullable unions | **HIGH** |
| 2 | UnionSwitchExpressionDiagnosticSupressor | 238-245 | Positional pattern deconstruction validation | **HIGH** |

**Impact:** Core type safety guarantees and compilation experience.

### 🟠 Priority 2: HIGH - Error Paths and Edge Cases

| # | Component | Lines | Description | Risk Level |
|---|-----------|-------|-------------|------------|
| 3 | RecordDeclarationSyntaxParser | 159-160 | Non-partial parent validation | **MEDIUM** |
| 4 | UnionExtensionsSourceBuilder | 14-17 | Null namespace validation | **MEDIUM** |
| 5 | UnionSwitchExpressionDiagnosticSupressor | 27-28, 32-33, 41-42, 59-60 | Early return paths | **MEDIUM** |
| 6 | SyntaxExtensions | 56, 58, 60-63 | Rare accessibility modifiers | **MEDIUM** |

**Impact:** Robustness and developer experience.

### 🟡 Priority 3: MEDIUM - Generic Type Constraints

| # | Component | Lines | Description | Risk Level |
|---|-----------|-------|-------------|------------|
| 7 | UnionExtensionsSourceBuilder | 103-105, 163-165, 230-232, 286-288 | Type parameter constraint propagation | **LOW-MEDIUM** |
| 8 | TypeParameter & TypeParameterConstraint | 71, 99 | ToString implementations | **LOW** |

**Impact:** Correctness for constrained generic unions.

### 🟢 Priority 4: LOW - Internal Implementation Details

| # | Component | Lines | Description | Risk Level |
|---|-----------|-------|-------------|------------|
| 9 | ImmutableEquatableArray&lt;T&gt; | 29, 32, 35-51, 57 | Equality and hashing | **LOW** |
| 10 | IdentifierExtensions | 20 | Single character identifier edge case | **VERY LOW** |

**Impact:** Internal utilities with low user impact.

### ⚪ Priority 5: VERY LOW - Infrastructure

| # | Component | Lines | Description | Risk Level |
|---|-----------|-------|-------------|------------|
| 11 | UnionGenerator | 41-42, 52-53, 75-76 | Cancellation token handling | **VERY LOW** |
| 12 | RecordDeclarationSyntaxParser | 151-152 | Top-level statement filtering | **VERY LOW** |
| 13 | UnionGenerator | 83-84 | Null symbol defensive check | **VERY LOW** |
| 14 | SyntaxExtensions | 25 | Empty using directives | **VERY LOW** |
| 15 | UnionSwitchExpressionDiagnosticSupressor | 116, 127 | Pattern matching fallbacks | **VERY LOW** |

**Impact:** Framework infrastructure and defensive code.

## Recommended Test Additions

### Immediate (Priority 1-2)
- [ ] Nullable union switch expressions with null pattern
- [ ] Nullable union with discard/var patterns
- [ ] Positional patterns with nested deconstruction
- [ ] Nested unions in non-partial parent types
- [ ] Unions with `protected internal` and `private protected` modifiers

### Short-term (Priority 3-4)
- [ ] Generic unions with multiple type constraints
- [ ] ImmutableEquatableArray equality edge cases
- [ ] Single character identifiers in naming conversions

### Long-term (Priority 5)
- [ ] Generator cancellation integration tests
- [ ] Unusual syntax tree structures
- [ ] Property-based testing for pattern matching edge cases

## Quick Statistics

| Metric | Value |
|--------|-------|
| Total Uncovered Lines | 75 |
| Critical Priority Lines | 10 (13.3%) |
| High Priority Lines | 33 (44.0%) |
| Medium Priority Lines | 17 (22.7%) |
| Low Priority Lines | 15 (20.0%) |

**Conclusion:** Focus testing efforts on the 43 lines (57.3%) in Priority 1-2 to maximize confidence in correctness.
