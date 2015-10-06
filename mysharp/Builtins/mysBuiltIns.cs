namespace mysharp
{
	public static class mysBuiltins {
		public static void Setup(
			mysSymbolSpace global
		) {
			Builtins.Core.Load.Setup( global );
			Builtins.Core.Lambda.Setup( global );
			Builtins.Core.Assign.Setup( global );
			Builtins.Core.Eval.Setup( global );
			Builtins.Core.ToString.Setup( global );
			Builtins.Core.InNamespace.Setup( global );

			Builtins.Arithmetic.Addition.Setup( global );
			Builtins.Arithmetic.Subtraction.Setup( global );
			Builtins.Arithmetic.Multiplication.Setup( global );
			Builtins.Arithmetic.Division.Setup( global );

			Builtins.Collections.Range.Setup( global );

			Builtins.ListHandling.Reverse.Setup( global );
			Builtins.ListHandling.Car.Setup( global );
			Builtins.ListHandling.Cdr.Setup( global );
			Builtins.ListHandling.Cons.Setup( global );
			Builtins.ListHandling.Len.Setup( global );

			Builtins.Flow.If.Setup( global );
			Builtins.Flow.When.Setup( global );

			Builtins.Comparison.Equals.Setup( global );
			Builtins.Comparison.GreaterThan.Setup( global );

			Builtins.Looping.For.Setup( global );
			Builtins.Looping.While.Setup( global );

			Builtins.Clr.ClrType.Setup( global );
			Builtins.Clr.NewClrObject.Setup( global );
			Builtins.Clr.ClrDot.Setup( global );
			Builtins.Clr.Set.Setup( global );
		}
	}
}
