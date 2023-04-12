using System;

namespace PitchBlack;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
public sealed class AllowNullAttribute : Attribute { }